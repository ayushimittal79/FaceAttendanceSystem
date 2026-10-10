-- =============================================
-- Face Recognition Attendance System
-- Database Schema | SQL Server
-- BCA 301-4 Project
-- =============================================

CREATE DATABASE FaceAttendanceDB;
GO

USE FaceAttendanceDB;
GO

-- Admin Users Table
CREATE TABLE Admins (
    AdminID     INT IDENTITY(1,1) PRIMARY KEY,
    Username    VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(256) NOT NULL,
    FullName    VARCHAR(100),
    CreatedAt   DATETIME DEFAULT GETDATE()
);

-- Insert default admin (password: admin123)
INSERT INTO Admins (Username, PasswordHash, FullName)
VALUES ('admin', HASHBYTES('SHA2_256', 'admin123'), 'System Administrator');

-- Students Table
CREATE TABLE Students (
    StudentID       INT IDENTITY(1,1) PRIMARY KEY,
    RollNumber      VARCHAR(20) NOT NULL UNIQUE,
    FullName        VARCHAR(100) NOT NULL,
    Email           VARCHAR(100),
    Branch          VARCHAR(50),
    Semester        INT,
    FaceImagePath   VARCHAR(500),      -- Path to stored face image
    FaceEncoding    VARBINARY(MAX),    -- Stored face feature vector
    RegisteredAt    DATETIME DEFAULT GETDATE(),
    IsActive        BIT DEFAULT 1
);

-- Subjects Table
CREATE TABLE Subjects (
    SubjectID   INT IDENTITY(1,1) PRIMARY KEY,
    SubjectCode VARCHAR(20) NOT NULL UNIQUE,
    SubjectName VARCHAR(100) NOT NULL,
    Semester    INT,
    FacultyName VARCHAR(100)
);

-- Seed some subjects
INSERT INTO Subjects (SubjectCode, SubjectName, Semester, FacultyName) VALUES
('BCA301', 'Dot Net Technologies', 4, 'Dr. Madan Singh'),
('BCA302', 'Database Management', 4, 'Prof. Sarah Thomas'),
('BCA303', 'Computer Networks', 4, 'Prof. Rajan Mehta'),
('BCA304', 'Software Engineering', 4, 'Dr. Priya Nair'),
('BCA305', 'French Language', 4, 'Prof. Marie Dupont');

-- Attendance Table
CREATE TABLE Attendance (
    AttendanceID    INT IDENTITY(1,1) PRIMARY KEY,
    StudentID       INT NOT NULL FOREIGN KEY REFERENCES Students(StudentID),
    SubjectID       INT NOT NULL FOREIGN KEY REFERENCES Subjects(SubjectID),
    AttendanceDate  DATE NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    AttendanceTime  TIME NOT NULL DEFAULT CAST(GETDATE() AS TIME),
    Status          VARCHAR(10) CHECK (Status IN ('Present', 'Absent', 'Late')) DEFAULT 'Present',
    MarkedBy        VARCHAR(50) DEFAULT 'FaceRecognition',
    Confidence      FLOAT,             -- Face match confidence %
    CONSTRAINT UQ_StudentSubjectDate UNIQUE (StudentID, SubjectID, AttendanceDate)
);

-- Attendance Summary View
GO
CREATE VIEW vw_AttendanceSummary AS
SELECT 
    s.RollNumber,
    s.FullName,
    sub.SubjectCode,
    sub.SubjectName,
    COUNT(CASE WHEN a.Status = 'Present' THEN 1 END) AS PresentCount,
    COUNT(CASE WHEN a.Status = 'Absent'  THEN 1 END) AS AbsentCount,
    COUNT(a.AttendanceID) AS TotalClasses,
    CAST(
        COUNT(CASE WHEN a.Status = 'Present' THEN 1 END) * 100.0 
        / NULLIF(COUNT(a.AttendanceID), 0) 
    AS DECIMAL(5,2)) AS AttendancePercentage
FROM Students s
JOIN Attendance a   ON s.StudentID  = a.StudentID
JOIN Subjects sub   ON sub.SubjectID = a.SubjectID
GROUP BY s.RollNumber, s.FullName, sub.SubjectCode, sub.SubjectName;

GO

-- Stored Procedure: Get today's attendance for a subject
CREATE PROCEDURE sp_GetTodayAttendance
    @SubjectID INT
AS
BEGIN
    SELECT 
        s.RollNumber,
        s.FullName,
        ISNULL(a.Status, 'Absent') AS Status,
        a.AttendanceTime,
        a.Confidence
    FROM Students s
    LEFT JOIN Attendance a 
        ON s.StudentID = a.StudentID 
        AND a.SubjectID = @SubjectID
        AND a.AttendanceDate = CAST(GETDATE() AS DATE)
    WHERE s.IsActive = 1
    ORDER BY s.RollNumber;
END
GO

-- Stored Procedure: Mark Attendance
CREATE PROCEDURE sp_MarkAttendance
    @StudentID  INT,
    @SubjectID  INT,
    @Status     VARCHAR(10),
    @Confidence FLOAT
AS
BEGIN
    IF EXISTS (
        SELECT 1 FROM Attendance 
        WHERE StudentID = @StudentID 
          AND SubjectID = @SubjectID 
          AND AttendanceDate = CAST(GETDATE() AS DATE)
    )
    BEGIN
        UPDATE Attendance 
        SET Status = @Status, Confidence = @Confidence, AttendanceTime = CAST(GETDATE() AS TIME)
        WHERE StudentID = @StudentID AND SubjectID = @SubjectID 
          AND AttendanceDate = CAST(GETDATE() AS DATE);
    END
    ELSE
    BEGIN
        INSERT INTO Attendance (StudentID, SubjectID, Status, Confidence)
        VALUES (@StudentID, @SubjectID, @Status, @Confidence);
    END
END
GO
