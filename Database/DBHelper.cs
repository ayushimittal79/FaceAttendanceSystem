using System;
using System.Data;
using System.Data.SqlClient;

namespace FaceAttendance.Database
{
    /// <summary>
    /// ADO.NET Database Helper — handles all SQL Server connectivity.
    /// Demonstrates: Connected Architecture, Connection pooling, Command execution.
    /// </summary>
    public static class DBHelper
    {
        // ✏️ UPDATE THIS: Replace with your SQL Server instance name
        private static readonly string ConnectionString =
            @"Server=localhost\SQLEXPRESS;Database=FaceAttendanceDB;Integrated Security=True;TrustServerCertificate=True;";

        /// <summary>Gets an open SqlConnection (caller must dispose)</summary>
        public static SqlConnection GetConnection()
        {
            var conn = new SqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        /// <summary>Execute INSERT / UPDATE / DELETE — returns rows affected</summary>
        public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Execute SELECT — returns DataTable (Disconnected Architecture)</summary>
        public static DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddRange(parameters);
                var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        /// <summary>Execute scalar — returns single value</summary>
        public static object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteScalar();
            }
        }

        /// <summary>Execute stored procedure — returns DataTable</summary>
        public static DataTable ExecuteStoredProcedure(string procName, params SqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(procName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(parameters);
                var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        /// <summary>Test connection — returns true if DB is reachable</summary>
        public static bool TestConnection()
        {
            try
            {
                using (var conn = GetConnection())
                    return conn.State == ConnectionState.Open;
            }
            catch { return false; }
        }
    }
}
