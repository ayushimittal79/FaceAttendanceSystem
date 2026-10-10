using System;

namespace FaceAttendance.Models
{
    public class AttendanceRecord
    {
        public int      AttendanceID   { get; set; }
        public int      StudentID      { get; set; }
        public int      SubjectID      { get; set; }
        public string   RollNumber     { get; set; }
        public string   StudentName    { get; set; }
        public string   SubjectCode    { get; set; }
        public string   SubjectName    { get; set; }
        public DateTime AttendanceDate { get; set; }
        public TimeSpan AttendanceTime { get; set; }
        public string   Status         { get; set; }   // Present / Absent / Late
        public double   Confidence     { get; set; }   // Face match confidence %
    }

    public class Subject
    {
        public int    SubjectID   { get; set; }
        public string SubjectCode { get; set; }
        public string SubjectName { get; set; }
        public int    Semester    { get; set; }
        public string FacultyName { get; set; }

        public override string ToString() => $"{SubjectCode} — {SubjectName}";
    }

    public class AttendanceSummary
    {
        public string RollNumber            { get; set; }
        public string FullName              { get; set; }
        public string SubjectCode           { get; set; }
        public string SubjectName           { get; set; }
        public int    PresentCount          { get; set; }
        public int    AbsentCount           { get; set; }
        public int    TotalClasses          { get; set; }
        public double AttendancePercentage  { get; set; }

        // Highlight students below 75% threshold
        public bool BelowThreshold => AttendancePercentage < 75.0;
    }
}
