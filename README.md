# Face Recognition Attendance System
## BCA 301-4 .NET Mini Project | Christ University

### Tech Stack
- Language: C# (.NET Framework 4.8 / .NET 6+)
- UI: Windows Forms
- Database: SQL Server (ADO.NET)
- Face Recognition: Emgu CV (OpenCV wrapper for C#)
- Reports: DataGridView + CSV Export

---

## Setup Instructions

### 1. NuGet Packages to Install
```
Install-Package Emgu.CV
Install-Package Emgu.CV.runtime.windows
Install-Package System.Data.SqlClient
```

### 2. SQL Server Setup
Run the file: `Database/schema.sql` in SQL Server Management Studio

### 3. Connection String
Edit `Database/DBHelper.cs` — update your SQL Server instance name.

---

## Project Structure
```
FaceAttendance/
├── Forms/
│   ├── LoginForm.cs          ← Admin login
│   ├── DashboardForm.cs      ← Main hub
│   ├── RegisterStudentForm.cs ← Capture & register face
│   ├── MarkAttendanceForm.cs  ← Live camera + face match
│   └── ReportsForm.cs         ← View/export attendance
├── Models/
│   ├── Student.cs
│   └── AttendanceRecord.cs
├── Database/
│   ├── DBHelper.cs           ← ADO.NET connection
│   ├── StudentRepository.cs
│   ├── AttendanceRepository.cs
│   └── schema.sql
├── FaceRecognition/
│   └── FaceEngine.cs         ← Emgu CV logic
└── Program.cs
```

---

## Features
| Feature | Description |
|---|---|
| Admin Login | Secure login with hashed password |
| Register Student | Capture face via webcam, store encoding |
| Mark Attendance | Real-time face recognition via webcam |
| Reports | Filter by date/subject, export to CSV |

---

## SDG Alignment
- **SDG 4** (Quality Education): Automates attendance tracking
- **SDG 9** (Industry & Innovation): Uses biometric tech
