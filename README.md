# TaskUp - Project Management System

A modern Kanban-style project management system built with ASP.NET Core 8.0.

---

## 📋 Features

- **User Management**: Registration, login, email confirmation, password reset
- **Board Management**: Create, edit, delete boards with Kanban columns
- **Task Management**: Add, edit, delete, drag & drop tasks between columns
- **Real-time Chat**: SignalR-based instant messaging with file sharing
- **Admin Panel**: User and board management with statistics
- **Dark/Light Mode**: User preference-based theming
- **Email Notifications**: Welcome, confirmation, password reset, invitations

---

## 🛠️ Technologies

| Category | Technology |
|----------|------------|
| Backend | ASP.NET Core 8.0 |
| ORM | Entity Framework Core 8.0 |
| Database | SQL Server |
| Auth | ASP.NET Core Identity |
| Real-time | SignalR |
| Email | SMTP (Gmail) |
| Frontend | Tailwind CSS, JavaScript, jQuery |
| Icons | Font Awesome 6 |

---

## 🗄️ Database Setup (BACPAC)

The project includes a ready-to-use database backup file (`TaskUp.bacpac`).

### Prerequisites
- SQL Server Management Studio (SSMS) 18.0 or later
- SQL Server (LocalDB, Express, or full version)

### Import Database

1. Open **SQL Server Management Studio**
2. Connect to your SQL Server instance:
   - `(localdb)\MSSQLLocalDB` or `localhost`
3. Right-click on **Databases** → **Import Data-tier Application...**
4. Select **Import from local disk**
5. Browse and select `TaskUp.bacpac` from the project root
6. Set **Database name:** `TaskUpDb`
7. Click **Finish**

### Verify Installation
```sql
SELECT name FROM sys.databases WHERE name = 'TaskUpDb';
USE TaskUpDb;
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES;
Default Admin Account
Email	 Password	Role
admin@taskup.com	admin123	Admin
user@taskup.com   user1234  User

⚙️ Configuration
Update appsettings.json with your connection string:

json
{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\MSSQLLocalDB;Database=TaskUpDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true;"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "EnableSsl": true
  },
  "AppUrl": "https://localhost:7237"
}
🚀 Running the Project
bash
dotnet restore
dotnet run
Access the app at: https://localhost:7237

👨‍💻 Developer
Orkhan Mirzeyev

GitHub: @orkhan-mirzeyev

Email: mirzeyev05orxan@gmail.com

⭐ Star this repository if you find it useful!
