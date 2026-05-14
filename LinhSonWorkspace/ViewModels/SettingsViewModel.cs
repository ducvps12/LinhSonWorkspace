using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LinhSonWorkspace.Data;
using LinhSonWorkspace.Models;
using Microsoft.EntityFrameworkCore;

namespace LinhSonWorkspace.ViewModels
{
    /// <summary>
    /// ViewModel for the Settings page. Admin-only.
    /// Manages SMTP email configuration and general application settings.
    /// </summary>
    public class SettingsViewModel : BaseViewModel
    {
        // ========== SMTP Settings ==========
        private string _smtpHost = string.Empty;
        public string SmtpHost
        {
            get => _smtpHost;
            set => SetProperty(ref _smtpHost, value);
        }

        private string _smtpPort = "587";
        public string SmtpPort
        {
            get => _smtpPort;
            set => SetProperty(ref _smtpPort, value);
        }

        private string _smtpEmail = string.Empty;
        public string SmtpEmail
        {
            get => _smtpEmail;
            set => SetProperty(ref _smtpEmail, value);
        }

        private string _smtpPassword = string.Empty;
        public string SmtpPassword
        {
            get => _smtpPassword;
            set => SetProperty(ref _smtpPassword, value);
        }

        private string _smtpDisplayName = string.Empty;
        public string SmtpDisplayName
        {
            get => _smtpDisplayName;
            set => SetProperty(ref _smtpDisplayName, value);
        }

        private bool _smtpEnableSsl = true;
        public bool SmtpEnableSsl
        {
            get => _smtpEnableSsl;
            set => SetProperty(ref _smtpEnableSsl, value);
        }

        // ========== General Settings ==========
        private string _appTitle = "Linh Son Workspace";
        public string AppTitle
        {
            get => _appTitle;
            set => SetProperty(ref _appTitle, value);
        }

        private string _contactPhone = string.Empty;
        public string ContactPhone
        {
            get => _contactPhone;
            set => SetProperty(ref _contactPhone, value);
        }

        private string _contactEmail = string.Empty;
        public string ContactEmail
        {
            get => _contactEmail;
            set => SetProperty(ref _contactEmail, value);
        }

        private string _address = string.Empty;
        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        // ========== Google Auth Settings ==========
        private string _googleClientId = string.Empty;
        public string GoogleClientId
        {
            get => _googleClientId;
            set => SetProperty(ref _googleClientId, value);
        }

        private string _googleClientSecret = string.Empty;
        public string GoogleClientSecret
        {
            get => _googleClientSecret;
            set => SetProperty(ref _googleClientSecret, value);
        }

        // ========== Test Email ==========
        private string _testEmailAddress = string.Empty;
        public string TestEmailAddress
        {
            get => _testEmailAddress;
            set => SetProperty(ref _testEmailAddress, value);
        }

        // ========== Status ==========
        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private bool _isSuccess;
        public bool IsSuccess
        {
            get => _isSuccess;
            set => SetProperty(ref _isSuccess, value);
        }

        private bool _isSaving;
        public bool IsSaving
        {
            get => _isSaving;
            set => SetProperty(ref _isSaving, value);
        }

        // ========== User Management ==========
        private System.Collections.ObjectModel.ObservableCollection<User> _usersList;
        public System.Collections.ObjectModel.ObservableCollection<User> UsersList
        {
            get => _usersList;
            set => SetProperty(ref _usersList, value);
        }

        private System.Collections.ObjectModel.ObservableCollection<Role> _rolesList;
        public System.Collections.ObjectModel.ObservableCollection<Role> RolesList
        {
            get => _rolesList;
            set => SetProperty(ref _rolesList, value);
        }

        private User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                SetProperty(ref _selectedUser, value);
                if (value != null)
                {
                    EditUserId = value.UserId;
                    EditFullName = value.FullName;
                    EditUsername = value.Username;
                    EditEmail = value.Email;
                    EditPhone = value.Phone;
                    EditRoleId = value.RoleId;
                    EditIsActive = value.IsActive;
                }
            }
        }

        private bool _isEditingUser;
        public bool IsEditingUser
        {
            get => _isEditingUser;
            set => SetProperty(ref _isEditingUser, value);
        }

        private bool _isAddUserMode;
        public bool IsAddUserMode
        {
            get => _isAddUserMode;
            set => SetProperty(ref _isAddUserMode, value);
        }

        // Edit User properties
        private int _editUserId;
        public int EditUserId { get => _editUserId; set => SetProperty(ref _editUserId, value); }
        private string _editFullName;
        public string EditFullName { get => _editFullName; set => SetProperty(ref _editFullName, value); }
        private string _editUsername;
        public string EditUsername { get => _editUsername; set => SetProperty(ref _editUsername, value); }
        private string _editEmail;
        public string EditEmail { get => _editEmail; set => SetProperty(ref _editEmail, value); }
        private string _editPhone;
        public string EditPhone { get => _editPhone; set => SetProperty(ref _editPhone, value); }
        private int _editRoleId;
        public int EditRoleId { get => _editRoleId; set => SetProperty(ref _editRoleId, value); }
        private bool _editIsActive;
        public bool EditIsActive { get => _editIsActive; set => SetProperty(ref _editIsActive, value); }

        // ========== Commands ==========
        public ICommand SaveSmtpCommand { get; }
        public ICommand SaveGeneralCommand { get; }
        public ICommand SaveGoogleAuthCommand { get; }
        public ICommand SendTestEmailCommand { get; }

        public ICommand RefreshUsersCommand { get; }
        public ICommand AddUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand SaveUserCommand { get; }
        public ICommand CancelEditUserCommand { get; }
        public ICommand ResetPasswordCommand { get; }
        public ICommand ToggleStatusCommand { get; }

        public SettingsViewModel()
        {
            SaveSmtpCommand = new AsyncRelayCommand(ExecuteSaveSmtp);
            SaveGeneralCommand = new AsyncRelayCommand(ExecuteSaveGeneral);
            SaveGoogleAuthCommand = new AsyncRelayCommand(ExecuteSaveGoogleAuth);
            SendTestEmailCommand = new AsyncRelayCommand(ExecuteSendTestEmail);
            
            RefreshUsersCommand = new RelayCommand(LoadUsers);
            AddUserCommand = new RelayCommand(ExecuteAddUser);
            EditUserCommand = new RelayCommand(ExecuteEditUser, () => SelectedUser != null);
            SaveUserCommand = new AsyncRelayCommand(ExecuteSaveUser);
            CancelEditUserCommand = new RelayCommand(() => IsEditingUser = false);
            ResetPasswordCommand = new AsyncRelayCommand(ExecuteResetPassword, () => SelectedUser != null);
            ToggleStatusCommand = new AsyncRelayCommand(ExecuteToggleStatus, () => SelectedUser != null);

            // Load settings from database
            LoadSettings();
            LoadUsers();
        }

        /// <summary>
        /// Loads all settings from the AppSettings table.
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                using var context = new AppDbContext();
                var settings = context.AppSettings.ToList();

                SmtpHost = GetSetting(settings, "SmtpHost");
                SmtpPort = GetSetting(settings, "SmtpPort", "587");
                SmtpEmail = GetSetting(settings, "SmtpEmail");
                SmtpPassword = GetSetting(settings, "SmtpPassword");
                SmtpDisplayName = GetSetting(settings, "SmtpDisplayName");
                SmtpEnableSsl = GetSetting(settings, "SmtpEnableSsl", "true") == "true";

                AppTitle = GetSetting(settings, "AppTitle", "Linh Son Workspace");
                ContactPhone = GetSetting(settings, "ContactPhone");
                ContactEmail = GetSetting(settings, "ContactEmail");
                Address = GetSetting(settings, "Address");

                GoogleClientId = GetSetting(settings, "GoogleClientId");
                GoogleClientSecret = GetSetting(settings, "GoogleClientSecret");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi tải cấu hình: {ex.Message}";
                IsSuccess = false;
            }
        }

        /// <summary>
        /// Gets a setting value by key, returns defaultValue if not found.
        /// </summary>
        private static string GetSetting(List<AppSetting> settings, string key, string defaultValue = "")
        {
            return settings.FirstOrDefault(s => s.SettingKey == key)?.SettingValue ?? defaultValue;
        }

        /// <summary>
        /// Saves or updates a setting in the database.
        /// </summary>
        private static void SaveSetting(AppDbContext context, string key, string value, string category, string description = "")
        {
            var existing = context.AppSettings.FirstOrDefault(s => s.SettingKey == key);
            if (existing != null)
            {
                existing.SettingValue = value;
                existing.UpdatedAt = DateTime.Now;
            }
            else
            {
                context.AppSettings.Add(new AppSetting
                {
                    SettingKey = key,
                    SettingValue = value,
                    Category = category,
                    Description = description,
                    UpdatedAt = DateTime.Now
                });
            }
        }

        /// <summary>
        /// Saves SMTP configuration to database.
        /// </summary>
        private async Task ExecuteSaveSmtp()
        {
            IsSaving = true;
            StatusMessage = "";

            try
            {
                await Task.Run(() =>
                {
                    using var context = new AppDbContext();
                    SaveSetting(context, "SmtpHost", SmtpHost, "SMTP", "SMTP Server Host");
                    SaveSetting(context, "SmtpPort", SmtpPort, "SMTP", "SMTP Server Port");
                    SaveSetting(context, "SmtpEmail", SmtpEmail, "SMTP", "Email address for sending");
                    SaveSetting(context, "SmtpPassword", SmtpPassword, "SMTP", "Email password / App Password");
                    SaveSetting(context, "SmtpDisplayName", SmtpDisplayName, "SMTP", "Display name in emails");
                    SaveSetting(context, "SmtpEnableSsl", SmtpEnableSsl.ToString().ToLower(), "SMTP", "Enable SSL/TLS");
                    context.SaveChanges();
                });

                StatusMessage = "✅ Đã lưu cấu hình SMTP thành công!";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Lỗi: {ex.Message}";
                IsSuccess = false;
            }
            finally
            {
                IsSaving = false;
            }
        }

        /// <summary>
        /// Saves general settings to database.
        /// </summary>
        private async Task ExecuteSaveGeneral()
        {
            IsSaving = true;
            StatusMessage = "";

            try
            {
                await Task.Run(() =>
                {
                    using var context = new AppDbContext();
                    SaveSetting(context, "AppTitle", AppTitle, "General", "Application title");
                    SaveSetting(context, "ContactPhone", ContactPhone, "General", "Contact phone number");
                    SaveSetting(context, "ContactEmail", ContactEmail, "General", "Contact email");
                    SaveSetting(context, "Address", Address, "General", "Business address");
                    context.SaveChanges();
                });

                StatusMessage = "✅ Đã lưu cấu hình chung thành công!";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Lỗi: {ex.Message}";
                IsSuccess = false;
            }
            finally
            {
                IsSaving = false;
            }
        }

        /// <summary>
        /// Saves Google Auth settings to database.
        /// </summary>
        private async Task ExecuteSaveGoogleAuth()
        {
            IsSaving = true;
            StatusMessage = "";

            try
            {
                await Task.Run(() =>
                {
                    using var context = new AppDbContext();
                    SaveSetting(context, "GoogleClientId", GoogleClientId, "GoogleAuth", "Google OAuth Client ID");
                    SaveSetting(context, "GoogleClientSecret", GoogleClientSecret, "GoogleAuth", "Google OAuth Client Secret");
                    context.SaveChanges();
                });

                StatusMessage = "✅ Đã lưu cấu hình Google Auth thành công!";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Lỗi: {ex.Message}";
                IsSuccess = false;
            }
            finally
            {
                IsSaving = false;
            }
        }

        /// <summary>
        /// Sends a test email using current SMTP settings.
        /// </summary>
        private async Task ExecuteSendTestEmail()
        {
            if (string.IsNullOrWhiteSpace(TestEmailAddress))
            {
                StatusMessage = "⚠️ Vui lòng nhập email nhận thử!";
                IsSuccess = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(SmtpHost) || string.IsNullOrWhiteSpace(SmtpEmail))
            {
                StatusMessage = "⚠️ Vui lòng cấu hình SMTP trước!";
                IsSuccess = false;
                return;
            }

            IsSaving = true;
            StatusMessage = "📧 Đang gửi email thử...";

            try
            {
                await Task.Run(() =>
                {
                    int port = int.TryParse(SmtpPort, out int p) ? p : 587;

                    using var client = new SmtpClient(SmtpHost, port)
                    {
                        Credentials = new NetworkCredential(SmtpEmail, SmtpPassword),
                        EnableSsl = SmtpEnableSsl,
                        Timeout = 15000
                    };

                    var from = new MailAddress(SmtpEmail, string.IsNullOrEmpty(SmtpDisplayName) ? SmtpEmail : SmtpDisplayName);
                    var to = new MailAddress(TestEmailAddress);
                    var message = new MailMessage(from, to)
                    {
                        Subject = "[Linh Son Workspace] Test Email - Kiểm tra SMTP",
                        Body = $"Xin chào!\n\nĐây là email thử nghiệm từ hệ thống Linh Son Workspace.\nThời gian gửi: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n\nCấu hình SMTP hoạt động tốt! ✅\n\n---\nLinh Son Workspace Booking Management System",
                        IsBodyHtml = false
                    };

                    client.Send(message);
                });

                StatusMessage = $"✅ Gửi email thành công đến {TestEmailAddress}!";
                IsSuccess = true;
            }
            catch (SmtpException ex)
            {
                StatusMessage = $"❌ Lỗi SMTP: {ex.Message}";
                IsSuccess = false;
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Lỗi: {ex.Message}";
                IsSuccess = false;
            }
            finally
            {
                IsSaving = false;
            }
        }
        // ========== User Management Methods ==========
        private void LoadUsers()
        {
            try
            {
                using var context = new AppDbContext();
                var users = context.Users.Include(u => u.Role).ToList();
                UsersList = new System.Collections.ObjectModel.ObservableCollection<User>(users);

                var roles = context.Roles.ToList();
                RolesList = new System.Collections.ObjectModel.ObservableCollection<Role>(roles);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi tải danh sách người dùng: {ex.Message}";
                IsSuccess = false;
            }
        }

        private void ExecuteAddUser()
        {
            SelectedUser = null;
            EditUserId = 0;
            EditFullName = "";
            EditUsername = "";
            EditEmail = "";
            EditPhone = "";
            EditRoleId = RolesList?.FirstOrDefault()?.RoleId ?? 2; // Default to Staff
            EditIsActive = true;
            
            IsAddUserMode = true;
            IsEditingUser = true;
        }

        private void ExecuteEditUser()
        {
            if (SelectedUser == null) return;
            IsAddUserMode = false;
            IsEditingUser = true;
        }

        private async Task ExecuteSaveUser()
        {
            if (string.IsNullOrWhiteSpace(EditUsername) || string.IsNullOrWhiteSpace(EditFullName))
            {
                StatusMessage = "Tên đăng nhập và Họ tên không được để trống!";
                IsSuccess = false;
                return;
            }

            try
            {
                using var context = new AppDbContext();
                
                // Check username duplicate
                if (context.Users.Any(u => u.Username == EditUsername && u.UserId != EditUserId))
                {
                    StatusMessage = "Tên đăng nhập đã tồn tại!";
                    IsSuccess = false;
                    return;
                }

                if (IsAddUserMode)
                {
                    var newUser = new User
                    {
                        FullName = EditFullName,
                        Username = EditUsername,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), // Default password
                        Email = EditEmail,
                        Phone = EditPhone,
                        RoleId = EditRoleId,
                        IsActive = EditIsActive,
                        CreatedAt = DateTime.Now
                    };
                    context.Users.Add(newUser);
                    StatusMessage = "✅ Đã thêm người dùng mới! (Mật khẩu mặc định: 123456)";
                }
                else
                {
                    var user = await context.Users.FindAsync(EditUserId);
                    if (user != null)
                    {
                        user.FullName = EditFullName;
                        user.Username = EditUsername;
                        user.Email = EditEmail;
                        user.Phone = EditPhone;
                        user.RoleId = EditRoleId;
                        user.IsActive = EditIsActive;
                    }
                    StatusMessage = "✅ Đã cập nhật thông tin người dùng!";
                }

                await context.SaveChangesAsync();
                IsSuccess = true;
                IsEditingUser = false;
                LoadUsers();
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Lỗi lưu người dùng: {ex.Message}";
                IsSuccess = false;
            }
        }

        private async Task ExecuteResetPassword()
        {
            if (SelectedUser == null) return;
            
            // In a real app we'd ask for confirmation.
            try
            {
                using var context = new AppDbContext();
                var user = await context.Users.FindAsync(SelectedUser.UserId);
                if (user != null)
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456");
                    await context.SaveChangesAsync();
                    StatusMessage = $"✅ Đã reset mật khẩu của {user.Username} về '123456'";
                    IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Lỗi reset mật khẩu: {ex.Message}";
                IsSuccess = false;
            }
        }

        private async Task ExecuteToggleStatus()
        {
            if (SelectedUser == null) return;
            
            try
            {
                using var context = new AppDbContext();
                var user = await context.Users.FindAsync(SelectedUser.UserId);
                if (user != null)
                {
                    // Prevent admin from deactivating themselves
                    if (user.Username == "admin" && user.IsActive)
                    {
                        StatusMessage = "❌ Không thể vô hiệu hóa tài khoản admin chính!";
                        IsSuccess = false;
                        return;
                    }

                    user.IsActive = !user.IsActive;
                    await context.SaveChangesAsync();
                    
                    StatusMessage = $"✅ Đã {(user.IsActive ? "kích hoạt" : "vô hiệu hóa")} tài khoản {user.Username}!";
                    IsSuccess = true;
                    LoadUsers();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Lỗi đổi trạng thái: {ex.Message}";
                IsSuccess = false;
            }
        }
    }
}
