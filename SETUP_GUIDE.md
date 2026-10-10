# 🚀 Setup Guide — Face Attendance System
## Step-by-step for Visual Studio + Windows

---

## Step 1: Prerequisites

Install these if not already present:
- **Visual Studio 2022** (Community edition is free)
  - Workload: *.NET desktop development*
- **SQL Server Express** — free from microsoft.com/en-us/sql-server/
- **SQL Server Management Studio (SSMS)** — for running schema.sql

---

## Step 2: Create the Database

1. Open **SSMS**, connect to `.\SQLEXPRESS`
2. Open `Database/schema.sql`
3. Click **Execute** (F5)
4. Verify: You should see `FaceAttendanceDB` in Object Explorer

---

## Step 3: Open the Project

1. Open **Visual Studio 2022**
2. File → Open → Project/Solution → select `FaceAttendance.csproj`

---

## Step 4: Install NuGet Packages

In Visual Studio: **Tools → NuGet Package Manager → Package Manager Console**

```powershell
Install-Package Emgu.CV
Install-Package Emgu.CV.runtime.windows
Install-Package System.Data.SqlClient
```

Wait for installation to complete.

---

## Step 5: Download Haar Cascade XML

The face detection model file is needed:

1. Go to: https://github.com/opencv/opencv/tree/master/data/haarcascades
2. Download: `haarcascade_frontalface_default.xml`
3. Place it in the project's `Assets/` folder

OR run this PowerShell command from project root:
```powershell
Invoke-WebRequest -Uri "https://raw.githubusercontent.com/opencv/opencv/master/data/haarcascades/haarcascade_frontalface_default.xml" -OutFile "Assets/haarcascade_frontalface_default.xml"
```

---

## Step 6: Update Connection String (if needed)

Open `Database/DBHelper.cs`, line with `ConnectionString`:

```csharp
// Default (SQL Server Express):
@"Server=.\SQLEXPRESS;Database=FaceAttendanceDB;Integrated Security=True;"

// If using named instance (e.g. DESKTOP-ABC\SQLEXPRESS):
@"Server=DESKTOP-ABC\SQLEXPRESS;Database=FaceAttendanceDB;Integrated Security=True;"

// If using SQL auth:
@"Server=.\SQLEXPRESS;Database=FaceAttendanceDB;User Id=sa;Password=YourPassword;"
```

---

## Step 7: Run the App

1. Press **F5** in Visual Studio
2. Login: `admin` / `admin123`
3. You're in! 🎉

---

## Usage Flow

```
Login → Dashboard
  ↓
Register Student  →  Enter details + capture webcam photo
  ↓
Mark Attendance   →  Select subject → Start camera → auto-marks faces
  ↓
Reports           →  Filter by subject/date → export CSV
```

---

## Default Login
| Field    | Value      |
|----------|------------|
| Username | admin      |
| Password | admin123   |

---

## Troubleshooting

| Problem | Fix |
|---|---|
| "Webcam not found" | Check Device Manager, try index 1 instead of 0 in `VideoCapture(0)` |
| "Cannot connect to DB" | Start SQL Server service; verify server name in connection string |
| "No faces detected" | Ensure good lighting; lower `minNeighbors` in `DetectMultiScale` |
| Build errors with Emgu | Clean solution + rebuild; ensure NuGet packages restored |
| `haarcascade` not found | Place XML file in `bin/Debug/Assets/` folder too |

---

## Project covers these COs:
- **CO1** — .NET Framework, C#, Windows Forms basics
- **CO2** — Event-driven Forms, custom controls, DataGridView  
- **CO3** — ADO.NET, SQL Server, CRUD, Stored Procedures, Views
- **CO4** — File handling (face image I/O, CSV export via System.IO)
- **CO5** — Face recognition, biometrics via Emgu CV
