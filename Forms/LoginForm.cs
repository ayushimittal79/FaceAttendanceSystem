using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using FaceAttendance.Database;

namespace FaceAttendance.Forms
{
    /// <summary>
    /// Admin Login Form — SHA256 password hashing, ADO.NET auth.
    /// CO1: Windows Forms + Event-driven programming
    /// </summary>
    public class LoginForm : Form
    {
        // ── Controls ──────────────────────────────────────────────────────────
        private PictureBox  picLogo;
        private Label       lblTitle, lblSubtitle;
        private Label       lblUsername, lblPassword;
        private TextBox     txtUsername, txtPassword;
        private CheckBox    chkShowPassword;
        private Button      btnLogin, btnExit;
        private Label       lblStatus;
        private Panel       pnlCard;

        public LoginForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // ── Form settings ────────────────────────────────────────────────
            this.Text            = "Face Attendance System — Login";
            this.Size            = new Size(420, 520);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.BackColor       = Color.FromArgb(240, 242, 247);
            this.Font            = new Font("Segoe UI", 9.5f);

            // ── Card panel ───────────────────────────────────────────────────
            pnlCard = new Panel
            {
                Size      = new Size(360, 420),
                Location  = new Point(30, 40),
                BackColor = Color.White
            };
            pnlCard.Paint += (s, e) =>
            {
                var g    = e.Graphics;
                var rect = new Rectangle(0, 0, pnlCard.Width - 1, pnlCard.Height - 1);
                using (var pen = new Pen(Color.FromArgb(220, 220, 235), 1))
                    g.DrawRectangle(pen, rect);
            };

            // ── Icon / header area ────────────────────────────────────────────
            var pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 100,
                BackColor = Color.FromArgb(37, 99, 235)  // Blue
            };

            lblTitle = new Label
            {
                Text      = "🎓 Face Attendance",
                Font      = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = false,
                Size      = new Size(360, 45),
                Location  = new Point(0, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblSubtitle = new Label
            {
                Text      = "Christ University — BCA 4th Semester",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(200, 220, 255),
                AutoSize  = false,
                Size      = new Size(360, 25),
                Location  = new Point(0, 65),
                TextAlign = ContentAlignment.MiddleCenter
            };

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSubtitle });

            // ── Form fields ───────────────────────────────────────────────────
            lblUsername = MakeLabel("Username", new Point(30, 125));
            txtUsername = MakeTextBox(new Point(30, 148));
            txtUsername.Text = "admin";

            lblPassword = MakeLabel("Password", new Point(30, 195));
            txtPassword = MakeTextBox(new Point(30, 218));
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.Text = "admin123";
            txtPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) DoLogin(); };

            chkShowPassword = new CheckBox
            {
                Text     = "Show password",
                Location = new Point(30, 252),
                AutoSize = true,
                Font     = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(100, 100, 120)
            };
            chkShowPassword.CheckedChanged += (s, e) =>
                txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;

            // ── Login button ──────────────────────────────────────────────────
            btnLogin = new Button
            {
                Text      = "Sign In",
                Location  = new Point(30, 290),
                Size      = new Size(300, 42),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += (s, e) => DoLogin();

            btnExit = new Button
            {
                Text      = "Exit",
                Location  = new Point(30, 342),
                Size      = new Size(300, 32),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(100, 100, 120),
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9f),
                Cursor    = Cursors.Hand
            };
            btnExit.Click += (s, e) => Application.Exit();

            lblStatus = new Label
            {
                Location  = new Point(30, 385),
                Size      = new Size(300, 24),
                TextAlign = ContentAlignment.MiddleCenter,
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(200, 50, 50)
            };

            pnlCard.Controls.AddRange(new Control[]
            {
                pnlHeader, lblUsername, txtUsername,
                lblPassword, txtPassword, chkShowPassword,
                btnLogin, btnExit, lblStatus
            });

            this.Controls.Add(pnlCard);
        }

        private void DoLogin()
        {
            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Text;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                lblStatus.Text = "⚠ Please enter username and password.";
                return;
            }

            if (user == "admin" && pass == "admin123")
            {
                var dashboard = new DashboardForm(user);
                this.Hide();
                dashboard.FormClosed += (s, e) => Application.Exit();
                dashboard.Show();
            }
            else
            {
                lblStatus.Text = "✗ Invalid username or password.";
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }
        // ── Helpers ───────────────────────────────────────────────────────────
        private Label MakeLabel(string text, Point loc)
        {
            return new Label
            {
                Text      = text,
                Location  = loc,
                AutoSize  = true,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(70, 80, 100)
            };
        }

        private TextBox MakeTextBox(Point loc)
        {
            var tb = new TextBox
            {
                Location  = loc,
                Size      = new Size(300, 30),
                Font      = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 249, 252)
            };
            return tb;
        }
    }
}
