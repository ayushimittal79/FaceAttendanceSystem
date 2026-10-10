using System;

namespace FaceAttendance.Models
{
    public class Student
    {
        public int    StudentID      { get; set; }
        public string RollNumber     { get; set; }
        public string FullName       { get; set; }
        public string Email          { get; set; }
        public string Branch         { get; set; }
        public int    Semester       { get; set; }
        public string FaceImagePath  { get; set; }
        public byte[] FaceEncoding   { get; set; }
        public DateTime RegisteredAt { get; set; }
        public bool   IsActive       { get; set; }

        public override string ToString() => $"{RollNumber} - {FullName}";
    }
}
