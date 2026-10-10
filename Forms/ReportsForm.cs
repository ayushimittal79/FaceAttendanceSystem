using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using FaceAttendance.Database;
using FaceAttendance.Models;

namespace FaceAttendance.Forms
{
    /// <summary>
    /// Attendance Reports — filter by subject/date, highlight low attendance,
    /// export to CSV. CO3: Disconnected ADO.NET, DataGridView, DataTable.
    /// </summary>
    public class ReportsForm : Form
    {
        private ComboBox    cmbSubject, cmbReportType;
        private DateTimePicker dtpFrom, dtpTo;
        private Button      btnLoad, btnExportCSV, btnPrint;
        private DataGridView dgvReport;
        private Label       lblInfo, lblTitle;
        private Panel       pnlFilters;

        private readonly AttendanceRepository _attendRepo = new AttendanceRepository();

        public ReportsForm()
        {
            InitializeComponents();
            LoadSubjects();
        }

        private void InitializeComponents()
        {
            this.Text       = "Attendance Reports";
            this.BackColor  = Color.FromArgb(240, 242, 247);
            this.Font       = new Font("Segoe UI", 9.5f);
            this.AutoScroll = true;

            lblTitle = new Label
            {
                Text      = "📊 Attendance Reports",
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize  = true,
                Location  = new Point(20, 15)
            };

            // ── Filter panel ──────────────────────────────────────────────────
            pnlFilters = new Panel
            {
                Location  = new Point(20, 50),
                Size      = new Size(920, 90),
                BackColor = Color.White,
                Padding   = new Padding(15)
            };
            pnlFilters.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 220, 235)))
                    e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, pnlFilters.Width-1, pnlFilters.Height-1));
            };

            int x = 15;

            // Report type
            pnlFilters.Controls.Add(MakeSmallLabel("Report Type", new Point(x, 12)));
            cmbReportType = new ComboBox
            {
                Location = new Point(x, 32), Size = new Size(160, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbReportType.Items.AddRange(new object[] { "Summary (All Time)", "Date Range" });
            cmbReportType.SelectedIndex = 0;
            cmbReportType.SelectedIndexChanged += (s, e) => ToggleDateRange();
            pnlFilters.Controls.Add(cmbReportType);
            x += 175;

            // Subject
            pnlFilters.Controls.Add(MakeSmallLabel("Subject", new Point(x, 12)));
            cmbSubject = new ComboBox
            {
                Location = new Point(x, 32), Size = new Size(220, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            pnlFilters.Controls.Add(cmbSubject);
            x += 235;

            // Date range
            pnlFilters.Controls.Add(MakeSmallLabel("From", new Point(x, 12)));
            dtpFrom = new DateTimePicker
            {
                Location = new Point(x, 32), Size = new Size(140, 28),
                Format   = DateTimePickerFormat.Short,
                Value    = DateTime.Today.AddDays(-30),
                Enabled  = false
            };
            pnlFilters.Controls.Add(dtpFrom);
            x += 155;

            pnlFilters.Controls.Add(MakeSmallLabel("To", new Point(x, 12)));
            dtpTo = new DateTimePicker
            {
                Location = new Point(x, 32), Size = new Size(140, 28),
                Format   = DateTimePickerFormat.Short,
                Value    = DateTime.Today,
                Enabled  = false
            };
            pnlFilters.Controls.Add(dtpTo);
            x += 155;

            // Buttons
            btnLoad = MakeButton("🔍  Load Report", new Point(x, 28), new Size(140, 34),
                Color.FromArgb(37, 99, 235));
            btnLoad.Click += BtnLoad_Click;
            pnlFilters.Controls.Add(btnLoad);

            // ── Action buttons ────────────────────────────────────────────────
            btnExportCSV = MakeButton("📥  Export CSV", new Point(20, 155), new Size(145, 34),
                Color.FromArgb(16, 185, 129));
            btnExportCSV.Click += BtnExportCSV_Click;

            btnPrint = MakeButton("🖨  Print", new Point(175, 155), new Size(110, 34),
                Color.FromArgb(100, 116, 139));
            btnPrint.Click += (s, e) => MessageBox.Show("Use Export CSV to save the report.", "Print");

            lblInfo = new Label
            {
                Location  = new Point(300, 162),
                Size      = new Size(560, 22),
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(100, 116, 139)
            };

            // ── Legend ────────────────────────────────────────────────────────
            var pnlLegend = new Panel { Location = new Point(750, 155), Size = new Size(200, 34), BackColor = Color.Transparent };
            pnlLegend.Controls.Add(new Label { Text = "🔴 < 75%  🟢 ≥ 75%", AutoSize = true, Location = new Point(0, 8),
                Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(80, 90, 110) });

            // ── DataGridView ──────────────────────────────────────────────────
            dgvReport = new DataGridView
            {
                Location              = new Point(20, 196),
                Size                  = new Size(930, 430),
                ReadOnly              = true,
                AllowUserToAddRows    = false,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor       = Color.White,
                BorderStyle           = BorderStyle.None,
                RowHeadersVisible     = false,
                Font                  = new Font("Segoe UI", 9f),
                GridColor             = Color.FromArgb(230, 232, 240),
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(248, 249, 252) }
            };
            dgvReport.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            dgvReport.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvReport.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgvReport.EnableHeadersVisualStyles               = false;
            dgvReport.RowPrePaint += DgvReport_RowPrePaint;

            this.Controls.AddRange(new Control[]
            {
                lblTitle, pnlFilters,
                btnExportCSV, btnPrint, lblInfo, pnlLegend,
                dgvReport
            });
        }

        private void LoadSubjects()
        {
            var subjects = _attendRepo.GetAllSubjects();
            cmbSubject.Items.Add(new Subject { SubjectID = -1, SubjectCode = "ALL", SubjectName = "All Subjects" });
            foreach (var s in subjects) cmbSubject.Items.Add(s);
            cmbSubject.SelectedIndex = 0;
        }

        private void ToggleDateRange()
        {
            bool isRange = cmbReportType.SelectedIndex == 1;
            dtpFrom.Enabled = isRange;
            dtpTo.Enabled   = isRange;
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt;
                Subject selected = cmbSubject.SelectedItem as Subject;
                int? subjectId = (selected?.SubjectID == -1) ? null : selected?.SubjectID;

                if (cmbReportType.SelectedIndex == 0)
                {
                    // Summary view
                    dt = _attendRepo.GetAttendanceSummary(subjectId);
                    lblInfo.Text = $"Showing attendance summary — {dt.Rows.Count} student records.";
                }
                else
                {
                    // Date range
                    if (subjectId == null)
                    {
                        MessageBox.Show("Please select a specific subject for date range report.");
                        return;
                    }
                    dt = _attendRepo.GetAttendanceByDateRange(dtpFrom.Value, dtpTo.Value, subjectId.Value);
                    lblInfo.Text = $"Records from {dtpFrom.Value:dd MMM} to {dtpTo.Value:dd MMM} — {dt.Rows.Count} rows.";
                }

                dgvReport.DataSource = dt;
                StyleColumns();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading report: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Color rows: red if below 75% attendance
        private void DgvReport_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (dgvReport.DataSource == null) return;
            var row = dgvReport.Rows[e.RowIndex];
            if (row.Cells["AttendancePercentage"] != null && row.Cells["AttendancePercentage"].Value != DBNull.Value)
            {
                double pct = Convert.ToDouble(row.Cells["AttendancePercentage"].Value);
                row.DefaultCellStyle.BackColor = pct < 75
                    ? Color.FromArgb(255, 235, 235)
                    : Color.FromArgb(235, 253, 245);
            }
        }

        private void StyleColumns()
        {
            // Rename columns for readability
            var renames = new System.Collections.Generic.Dictionary<string, string>
            {
                { "RollNumber",           "Roll No." },
                { "FullName",             "Student Name" },
                { "SubjectCode",          "Subject" },
                { "SubjectName",          "Subject Name" },
                { "PresentCount",         "Present" },
                { "AbsentCount",          "Absent" },
                { "TotalClasses",         "Total" },
                { "AttendancePercentage", "Attendance %" },
                { "AttendanceDate",       "Date" },
                { "AttendanceTime",       "Time" },
                { "Status",               "Status" },
                { "Confidence",           "Confidence" }
            };

            foreach (DataGridViewColumn col in dgvReport.Columns)
            {
                if (renames.TryGetValue(col.Name, out string friendly))
                    col.HeaderText = friendly;
            }
        }

        // ── CSV Export ────────────────────────────────────────────────────────
        private void BtnExportCSV_Click(object sender, EventArgs e)
        {
            if (dgvReport.Rows.Count == 0) { MessageBox.Show("No data to export."); return; }

            using (var dlg = new SaveFileDialog
            {
                Filter   = "CSV File|*.csv",
                FileName = $"Attendance_Report_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;

                var sb = new StringBuilder();

                // Header row
                foreach (DataGridViewColumn col in dgvReport.Columns)
                    sb.Append($"\"{col.HeaderText}\",");
                sb.AppendLine();

                // Data rows
                foreach (DataGridViewRow row in dgvReport.Rows)
                {
                    if (row.IsNewRow) continue;
                    foreach (DataGridViewCell cell in row.Cells)
                        sb.Append($"\"{cell.Value}\",");
                    sb.AppendLine();
                }

                File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show($"✅ Exported to:\n{dlg.FileName}", "Export Successful",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private Label MakeSmallLabel(string text, Point loc) => new Label
        {
            Text = text, Location = loc, AutoSize = true,
            Font = new Font("Segoe UI", 8f, FontStyle.Bold),
            ForeColor = Color.FromArgb(90, 100, 120)
        };

        private Button MakeButton(string text, Point loc, Size size, Color color)
        {
            var btn = new Button
            {
                Text = text, Location = loc, Size = size,
                BackColor = color, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}
