using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LinhSonWorkspace.Data;
using LinhSonWorkspace.Models;
using Microsoft.EntityFrameworkCore;

namespace LinhSonWorkspace.Services
{
    /// <summary>
    /// Handles Google OAuth 2.0 authentication for desktop apps using loopback redirect.
    /// Credentials are loaded from AppSettings table in the database (configured via Settings panel).
    /// </summary>
    public class GoogleAuthService
    {
        private const string AuthEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
        private const string UserInfoEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";

        public GoogleAuthService()
        {
        }

        /// <summary>
        /// Initiates Google OAuth flow using loopback redirect.
        /// Opens browser, listens for callback, exchanges code for token, verifies user.
        /// Returns User if authorized, null otherwise.
        /// </summary>
        public async Task<(User? user, string? error)> LoginWithGoogleAsync()
        {
            string clientId = "";
            string clientSecret = "";

            try
            {
                using var settingsContext = new AppDbContext();
                var settings = settingsContext.AppSettings.ToList();
                clientId = settings.FirstOrDefault(s => s.SettingKey == "GoogleClientId")?.SettingValue ?? "";
                clientSecret = settings.FirstOrDefault(s => s.SettingKey == "GoogleClientSecret")?.SettingValue ?? "";
            }
            catch { }

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                return (null, "Google Auth chưa được cấu hình. Vui lòng thiết lập Client ID và Secret trong phần Cài đặt.");

            // Use fixed loopback redirect (must match Google Console's Authorized redirect URI)
            // Add "http://127.0.0.1:5000/callback" to your Google Cloud Console > Credentials > Authorized redirect URIs
            int port = 5000;
            string redirectUri = $"http://127.0.0.1:{port}/callback";

            // Generate PKCE code verifier and challenge
            string codeVerifier = GenerateCodeVerifier();
            string codeChallenge = GenerateCodeChallenge(codeVerifier);
            string state = Guid.NewGuid().ToString("N");

            // Build authorization URL
            string authUrl = $"{AuthEndpoint}?" +
                $"client_id={Uri.EscapeDataString(clientId)}" +
                $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                $"&response_type=code" +
                $"&scope={Uri.EscapeDataString("openid email profile")}" +
                $"&state={state}" +
                $"&code_challenge={codeChallenge}" +
                $"&code_challenge_method=S256" +
                $"&access_type=offline";

            // Start HTTP listener
            using var listener = new HttpListener();
            listener.Prefixes.Add($"http://127.0.0.1:{port}/callback/");
            listener.Start();

            // Open browser for user to authenticate
            Process.Start(new ProcessStartInfo(authUrl) { UseShellExecute = true });

            // Wait for callback
            var context = await listener.GetContextAsync();
            var query = context.Request.QueryString;
            string? code = query["code"];
            string? returnedState = query["state"];

            // Send success page to browser
            string responseHtml = @"<html><body style='font-family:Segoe UI;text-align:center;padding:60px;background:#F9FAFB'>
                <h1 style='color:#10B981'>✅ Đăng nhập thành công!</h1>
                <p style='color:#6B7280'>Bạn có thể đóng tab này và quay lại ứng dụng.</p>
                <script>setTimeout(()=>window.close(),2000)</script>
                </body></html>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseHtml);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.Close();
            listener.Stop();

            // Validate state
            if (returnedState != state)
                return (null, "Xác thực không hợp lệ (state mismatch).");

            if (string.IsNullOrEmpty(code))
                return (null, "Không nhận được mã xác thực từ Google.");

            // Exchange code for token
            using var httpClient = new HttpClient();
            var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code",
                ["code_verifier"] = codeVerifier
            });

            var tokenResponse = await httpClient.PostAsync(TokenEndpoint, tokenRequest);
            var tokenJson = await tokenResponse.Content.ReadAsStringAsync();

            if (!tokenResponse.IsSuccessStatusCode)
                return (null, $"Lỗi lấy token: {tokenJson}");

            var tokenData = JsonDocument.Parse(tokenJson);
            string accessToken = tokenData.RootElement.GetProperty("access_token").GetString()!;

            // Get user info
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var userInfoResponse = await httpClient.GetAsync(UserInfoEndpoint);
            var userInfoJson = await userInfoResponse.Content.ReadAsStringAsync();

            if (!userInfoResponse.IsSuccessStatusCode)
                return (null, "Không thể lấy thông tin từ Google.");

            var userInfo = JsonDocument.Parse(userInfoJson);
            string googleEmail = userInfo.RootElement.GetProperty("email").GetString()!;
            string googleName = userInfo.RootElement.GetProperty("name").GetString()!;
            string googleId = userInfo.RootElement.GetProperty("id").GetString()!;

            // Only allow users that already exist in the database (created by admin)
            using var dbContext = new AppDbContext();
            var user = dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Email == googleEmail);

            if (user == null)
                return (null, $"Email '{googleEmail}' chưa được đăng ký trong hệ thống.\nVui lòng liên hệ Admin để được tạo tài khoản.");

            if (!user.IsActive)
                return (null, "Tài khoản đã bị vô hiệu hóa.");

            // Link GoogleId if not yet linked
            if (string.IsNullOrEmpty(user.GoogleId))
            {
                user.GoogleId = googleId;
                user.FullName = string.IsNullOrEmpty(user.FullName) ? googleName : user.FullName;
                await dbContext.SaveChangesAsync();
            }

            // Reload with Role navigation
            user = dbContext.Users.Include(u => u.Role).First(u => u.UserId == user.UserId);

            return (user, null);
        }

        private static int GetAvailablePort()
        {
            var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private static string GenerateCodeVerifier()
        {
            var bytes = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static string GenerateCodeChallenge(string verifier)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hash = sha256.ComputeHash(System.Text.Encoding.ASCII.GetBytes(verifier));
            return Convert.ToBase64String(hash)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}
