using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using FaceAttendance.Models;

namespace FaceAttendance.Database
{
    public class AttendanceRepository
    {
        // ── MARK ATTENDANCE (via stored procedure) ───────────────────────────
        public void MarkAttendance(int studentId, int subjectId, string status, double confidence)
        {
            DBHelper.ExecuteStoredProcedure("sp_MarkAttendance",
                new SqlParameter("@StudentID",  studentId),
                new SqlParameter("@SubjectID",  subjectId),
                new SqlParameter("@Status",     status),
                new SqlParameter("@Confidence", confidence)
            );
        }

        // ── GET TODAY'S ATTENDANCE ────────────────────────────────────────────
        public DataTable GetTodayAttendance(int subjectId)
        {
            return DBHelper.ExecuteStoredProcedure("sp_GetTodayAttendance",
                new SqlParameter("@SubjectID", subjectId));
        }

        // ── GET ALL SUBJECTS ──────────────────────────────────────────────────
        public List<Subject> GetAllSubjects()
        {
            var dt   = DBHelper.ExecuteQuery("SELECT * FROM Subjects ORDER BY SubjectCode");
            var list = new List<Subject>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Subject
                {
                    SubjectID   = Convert.ToInt32(row["SubjectID"]),
                    SubjectCode = row["SubjectCode"].ToString(),
                    SubjectName = row["SubjectName"].ToString(),
                    Semester    = Convert.ToInt32(row["Semester"]),
                    FacultyName = row["FacultyName"].ToString()
                });
            }
            return list;
        }

        // ── ATTENDANCE SUMMARY (for reports) ──────────────────────────────────
        public DataTable GetAttendanceSummary(int? subjectId = null)
        {
            string sql = "SELECT * FROM vw_AttendanceSummary";
            if (subjectId.HasValue)
                sql += " WHERE SubjectCode = (SELECT SubjectCode FROM Subjects WHERE SubjectID = @SubjectID)";

            var dt = subjectId.HasValue
                ? DBHelper.ExecuteQuery(sql, new SqlParameter("@SubjectID", subjectId.Value))
                : DBHelper.ExecuteQuery(sql);
            return dt;
        }

        // ── DATE RANGE REPORT ─────────────────────────────────────────────────
        public DataTable GetAttendanceByDateRange(DateTime from, DateTime to, int subjectId)
        {
            string sql = @"
                SELECT 
                    s.RollNumber, s.FullName, sub.SubjectCode,
                    a.AttendanceDate, a.AttendanceTime, a.Status,
                    ROUND(a.Confidence, 2) AS Confidence
                FROM Attendance a
                JOIN Students s  ON s.StudentID  = a.StudentID
                JOIN Subjects sub ON sub.SubjectID = a.SubjectID
                WHERE a.AttendanceDate BETWEEN @From AND @To
                  AND a.SubjectID = @SubjectID
                ORDER BY a.AttendanceDate DESC, s.RollNumber";

            return DBHelper.ExecuteQuery(sql,
                new SqlParameter("@From",      from),
                new SqlParameter("@To",        to),
                new SqlParameter("@SubjectID", subjectId)
            );
        }

        // ── CHECK IF ALREADY MARKED ────────────────────────────────────────────
        public bool IsAlreadyMarked(int studentId, int subjectId)
        {
            var result = DBHelper.ExecuteScalar(@"
                SELECT COUNT(1) FROM Attendance 
                WHERE StudentID = @SID AND SubjectID = @SubID 
                  AND AttendanceDate = CAST(GETDATE() AS DATE)",
                new SqlParameter("@SID",   studentId),
                new SqlParameter("@SubID", subjectId)
            );
            return Convert.ToInt32(result) > 0;
        }

        // ── GET STUDENT ATTENDANCE % ──────────────────────────────────────────
        public double GetStudentAttendancePercent(int studentId, int subjectId)
        {
            var result = DBHelper.ExecuteScalar(@"
                SELECT CAST(
                    COUNT(CASE WHEN Status='Present' THEN 1 END) * 100.0 
                    / NULLIF(COUNT(*), 0) 
                AS FLOAT)
                FROM Attendance 
                WHERE StudentID = @SID AND SubjectID = @SubID",
                new SqlParameter("@SID",   studentId),
                new SqlParameter("@SubID", subjectId)
            );
            return result == DBNull.Value ? 0.0 : Convert.ToDouble(result);
        }
    }
}
