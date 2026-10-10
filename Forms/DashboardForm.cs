using System;
using System.Drawing;
using System.Windows.Forms;

namespace FaceAttendance.Forms
{
    /// <summary>
    /// Main Dashboard — navigation hub after login.
    /// CO2: Windows Forms, custom controls, event-driven.
    /// </summary>
    public class DashboardForm : Form
    {
        private string _adminName;
        private Panel  pnlSidebar, pnlContent, pnlHeader;
        private Label  lblWelcome, lblDate;

        public DashboardForm(string adminName)
        {
            _adminName = adminName;
            InitializeComponents();
            ShowWelcomePanel();
        }

        private void InitializeComponents()
        {
            this.Text            = "Face Attendance System — Dashboard";
            this.Size            = new Size(1100, 680);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.MinimumSize     = new Size(900, 600);
            this.BackColor       = Color.FromArgb(240, 242, 247);
            this.Font            = new Font("Segoe UI", 9.5f);

            // ── Header bar ────────────────────────────────────────────────────
            pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 60,
                BackColor = Color.FromArgb(37, 99, 235)
            };

            var lblApp = new Label
            {
                Text      = "🎓 Face Attendance System",
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(20, 16)
            };

            lblWelcome = new Label
            {
                Text      = $"Welcome, {_adminName}",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(200, 220, 255),
                AutoSize  = true,
                Location  = new Point(850, 10)
            };

            lblDate = new Label
            {
                Text      = DateTime.Now.ToString("dddd, MMMM dd yyyy"),
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(180, 200, 240),
                AutoSize  = true,
                Location  = new Point(850, 32)
            };

            pnlHeader.Controls.AddRange(new Control[] { lblApp, lblWelcome, lblDate });

            // ── Sidebar ───────────────────────────────────────────────────────
            pnlSidebar = new Panel
            {
                Dock      = DockStyle.Left,
                Width     = 220,
                BackColor = Color.FromArgb(30, 41, 59)
            };

            string[] menuItems = {
                "🏠  Dashboard",
                "📋  Register Student",
                "📷  Mark Attendance",
                "📊  Reports",
                "⚙️  Settings",
                "",
                "🚪  Logout"
            };

            int y = 20;
            foreach (var item in menuItems)
            {
                if (item == "") { y += 10; continue; }

                bool isLogout = item.Contains("Logout");
                var btn = new Button
                {
                    Text      = item,
                    Location  = new Point(10, y),
                    Size      = new Size(200, 44),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Transparent,
                    ForeColor = isLogout ? Color.FromArgb(255, 150, 150) : Color.FromArgb(200, 215, 240),
                    Font      = new Font("Segoe UI", 9.5f),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding   = new Padding(10, 0, 0, 0),
                    Cursor    = Cursors.Hand,
                    Tag       = item
                };
                btn.FlatAppearance.BorderSize  = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(51, 65, 85);
                btn.Click += SidebarButton_Click;
                pnlSidebar.Controls.Add(btn);
                y += 50;
            }

            // ── Content area ─────────────────────────────────────────────────
            pnlContent = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 242, 247),
                Padding   = new Padding(20)
            };

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlSidebar);
            this.Controls.Add(pnlHeader);
        }

        private void SidebarButton_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            string tag = btn.Tag?.ToString() ?? "";

            if (tag.Contains("Register"))     ShowForm(new RegisterStudentForm());
            else if (tag.Contains("Mark"))    ShowForm(new MarkAttendanceForm());
            else if (tag.Contains("Reports")) ShowForm(new ReportsForm());
            else if (tag.Contains("Logout"))  { this.Close(); new LoginForm().Show(); }
            else ShowWelcomePanel();
        }

        private void ShowForm(Form form)
        {
            pnlContent.Controls.Clear();
            form.TopLevel       = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock           = DockStyle.Fill;
            pnlContent.Controls.Add(form);
            form.Show();
        }

        private void ShowWelcomePanel()
        {
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(BuildWelcomePanel());
        }

        private Panel BuildWelcomePanel()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            var title = new Label
            {
                Text      = "Welcome to Face Attendance System",
                Font      = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize  = true,
                Location  = new Point(40, 40)
            };

            var sub = new Label
            {
                Text      = "Use the sidebar to register students, mark attendance, or view reports.",
                Font      = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoSize  = true,
                Location  = new Point(40, 80)
            };

            // Quick-action cards
            string[] cards = {
                "📋\nRegister\nStudent",
                "📷\nMark\nAttendance",
                "📊\nView\nReports"
            };
            Color[] cardColors = {
                Color.FromArgb(37, 99, 235),
                Color.FromArgb(16, 185, 129),
                Color.FromArgb(245, 158, 11)
            };
            int cx = 40;
            for (int i = 0; i < cards.Length; i++)
            {
                int idx = i;
                var card = new Button
                {
                    Text      = cards[i],
                    Location  = new Point(cx, 130),
                    Size      = new Size(150, 130),
                    BackColor = cardColors[i],
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                    Cursor    = Cursors.Hand
                };
                card.FlatAppearance.BorderSize = 0;
                card.Click += (s, e) =>
                {
                    if (idx == 0)      ShowForm(new RegisterStudentForm());
                    else if (idx == 1) ShowForm(new MarkAttendanceForm());
                    else               ShowForm(new ReportsForm());
                };
                pnl.Controls.Add(card);
                cx += 170;
            }

            pnl.Controls.AddRange(new Control[] { title, sub });
            return pnl;
        }
    }
}
