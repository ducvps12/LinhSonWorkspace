# 🏢 Linh Son Workspace Booking Management System

**Linh Son Workspace Booking Management System** is a WPF desktop application that supports workspace rental management. The system allows staff and administrators to manage workspaces, customers, booking schedules, check-in/check-out, payment information, and booking reports. It helps prevent duplicated bookings and improves the workspace reservation process.

---

## 🎯 PRN212 Course Requirements Coverage

This project was built to meet the PRN212 Group Project requirements:

- **WPF UI**: Modern desktop application using Windows Presentation Foundation with MVVM architecture.
- **C# & OOP**: Object-Oriented Programming principles, Interfaces, Inheritance, and generic collections.
- **LINQ**: Extensive use of LINQ for data querying, filtering, and sorting.
- **EF Core**: Entity Framework Core for Data Access (Code-First approach) with SQL Server.
- **Stream I/O**: File export capabilities (CSV, JSON) and activity logging to text files.
- **Concurrency**: `async/await` implementation for non-blocking UI and Optimistic Concurrency control (handling database conflicts).
- **GenAI**: AI-assisted development for UI design and logic implementation.

---

## 🚀 Key Features

1. **Authentication & Authorization**
   - Role-based access control (Admin & Staff)
   - Secure password hashing (BCrypt)

2. **Workspace Management**
   - CRUD operations for workspaces (Hot Desks, Meeting Rooms, Private Offices)
   - Real-time status tracking (Available, Maintenance, Inactive)

3. **Booking System (Core Feature)**
   - Smart booking creation with auto price calculation
   - **Conflict Detection**: Prevents overlapping bookings for the same workspace
   - Check-in / Check-out workflow

4. **Reporting & Statistics**
   - Interactive charts using LiveCharts2
   - Revenue tracking and workspace utilization metrics

5. **Data Export & Logging**
   - Export bookings to `.csv` and `.json` formats
   - Write system activity logs to `.txt` files

---

## 🛠️ Technology Stack

- **Framework**: .NET 8.0 WPF
- **Architecture**: MVVM (Model-View-ViewModel)
- **Database**: SQL Server (LocalDB)
- **ORM**: Entity Framework Core 8.0.11
- **UI Components**: MaterialDesignThemes, LiveChartsCore

---

## 💻 Getting Started

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 (recommended) or VS Code
- SQL Server Express / LocalDB

### Running the App
1. Clone the repository
   ```bash
   git clone https://github.com/ducvps12/LinhSonWorkspace.git
   ```
2. Open the solution `LinhSonWorkspace.sln` in Visual Studio.
3. Build the project (NuGet packages will restore automatically).
4. Run the application. The database (`LinhSonWorkspaceDB`) and seed data will be created automatically on the first run.

### Default Accounts
- **Admin Role**: Username: `admin` | Password: `admin123`
- **Staff Role**: Username: `staff1` | Password: `staff123`

---
*Note: All data in this application is simulated for educational purposes only.*
