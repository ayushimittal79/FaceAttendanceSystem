using System;
using System.Windows.Forms;
using FaceAttendance.Database;
using FaceAttendance.Forms;

namespace FaceAttendance
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Check DB connection on startup
            if (!DBHelper.TestConnection())
            {
                MessageBox.Show(
                    "⚠ Cannot connect to SQL Server.\n\n" +
                    "Please check:\n" +
                    "1. SQL Server / SQLEXPRESS is running\n" +
                    "2. FaceAttendanceDB exists (run schema.sql)\n" +
                    "3. Connection string in DBHelper.cs\n\n" +
                    "The app will open in demo mode.",
                    "Database Connection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }

            Application.Run(new LoginForm());
        }
    }
}
