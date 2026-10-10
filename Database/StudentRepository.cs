using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using FaceAttendance.Models;

namespace FaceAttendance.Database
{
    /// <summary>
    /// Student data access — demonstrates ADO.NET CRUD operations.
    /// CO3: Database integration with ADO.NET
    /// </summary>
    public class StudentRepository
    {
        // ── CREATE ──────────────────────────────────────────────────────────
        public int AddStudent(Student s)
        {
            string sql = @"
                INSERT INTO Students (RollNumber, FullName, Email, Branch, Semester, FaceImagePath, FaceEncoding)
                VALUES (@Roll, @Name, @Email, @Branch, @Sem, @ImgPath, @Encoding);
                SELECT SCOPE_IDENTITY();";

            var result = DBHelper.ExecuteScalar(sql,
                new SqlParameter("@Roll",     s.RollNumber),
                new SqlParameter("@Name",     s.FullName),
                new SqlParameter("@Email",    s.Email     ?? (object)DBNull.Value),
                new SqlParameter("@Branch",   s.Branch    ?? (object)DBNull.Value),
                new SqlParameter("@Sem",      s.Semester),
                new SqlParameter("@ImgPath",  s.FaceImagePath ?? (object)DBNull.Value),
                new SqlParameter("@Encoding", s.FaceEncoding  ?? (object)DBNull.Value)
            );
            return Convert.ToInt32(result);
        }

        // ── READ ─────────────────────────────────────────────────────────────
        public List<Student> GetAllStudents()
        {
            var dt = DBHelper.ExecuteQuery(
                "SELECT * FROM Students WHERE IsActive = 1 ORDER BY RollNumber");
            return MapStudents(dt);
        }

        public Student GetByRollNumber(string roll)
        {
            var dt = DBHelper.ExecuteQuery(
                "SELECT * FROM Students WHERE RollNumber = @Roll AND IsActive = 1",
                new SqlParameter("@Roll", roll));
            var list = MapStudents(dt);
            return list.Count > 0 ? list[0] : null;
        }

        public Student GetByID(int id)
        {
            var dt = DBHelper.ExecuteQuery(
                "SELECT * FROM Students WHERE StudentID = @ID",
                new SqlParameter("@ID", id));
            var list = MapStudents(dt);
            return list.Count > 0 ? list[0] : null;
        }

        /// <summary>Returns all students that have a stored face encoding.</summary>
        public List<Student> GetStudentsWithFace()
        {
            var dt = DBHelper.ExecuteQuery(
                "SELECT * FROM Students WHERE FaceEncoding IS NOT NULL AND IsActive = 1");
            return MapStudents(dt);
        }

        // ── UPDATE ───────────────────────────────────────────────────────────
        public void UpdateStudent(Student s)
        {
            string sql = @"
                UPDATE Students 
                SET FullName = @Name, Email = @Email, Branch = @Branch, Semester = @Sem
                WHERE StudentID = @ID";
            DBHelper.ExecuteNonQuery(sql,
                new SqlParameter("@Name",   s.FullName),
                new SqlParameter("@Email",  s.Email   ?? (object)DBNull.Value),
                new SqlParameter("@Branch", s.Branch  ?? (object)DBNull.Value),
                new SqlParameter("@Sem",    s.Semester),
                new SqlParameter("@ID",     s.StudentID)
            );
        }

        public void UpdateFaceData(int studentId, string imagePath, byte[] encoding)
        {
            string sql = @"
                UPDATE Students 
                SET FaceImagePath = @ImgPath, FaceEncoding = @Encoding
                WHERE StudentID = @ID";
            DBHelper.ExecuteNonQuery(sql,
                new SqlParameter("@ImgPath",  imagePath),
                new SqlParameter("@Encoding", encoding),
                new SqlParameter("@ID",       studentId)
            );
        }

        // ── DELETE (soft delete) ─────────────────────────────────────────────
        public void DeactivateStudent(int studentId)
        {
            DBHelper.ExecuteNonQuery(
                "UPDATE Students SET IsActive = 0 WHERE StudentID = @ID",
                new SqlParameter("@ID", studentId));
        }

        // ── CHECK ─────────────────────────────────────────────────────────────
        public bool RollNumberExists(string roll)
        {
            var result = DBHelper.ExecuteScalar(
                "SELECT COUNT(1) FROM Students WHERE RollNumber = @Roll",
                new SqlParameter("@Roll", roll));
            return Convert.ToInt32(result) > 0;
        }

        // ── MAPPER ────────────────────────────────────────────────────────────
        private List<Student> MapStudents(DataTable dt)
        {
            var list = new List<Student>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Student
                {
                    StudentID     = Convert.ToInt32(row["StudentID"]),
                    RollNumber    = row["RollNumber"].ToString(),
                    FullName      = row["FullName"].ToString(),
                    Email         = row["Email"]    == DBNull.Value ? "" : row["Email"].ToString(),
                    Branch        = row["Branch"]   == DBNull.Value ? "" : row["Branch"].ToString(),
                    Semester      = Convert.ToInt32(row["Semester"]),
                    FaceImagePath = row["FaceImagePath"] == DBNull.Value ? null : row["FaceImagePath"].ToString(),
                    FaceEncoding  = row["FaceEncoding"]  == DBNull.Value ? null : (byte[])row["FaceEncoding"],
                    RegisteredAt  = Convert.ToDateTime(row["RegisteredAt"]),
                    IsActive      = Convert.ToBoolean(row["IsActive"])
                });
            }
            return list;
        }
    }
}
