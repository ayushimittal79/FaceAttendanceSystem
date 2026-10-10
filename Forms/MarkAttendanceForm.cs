using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using FaceAttendance.Database;
using FaceAttendance.FaceRecognition;
using FaceAttendance.Models;

namespace FaceAttendance.Forms
{
    /// <summary>
    /// Mark Attendance via live webcam face recognition.
    /// Detects faces in real-time, matches against registered students,
    /// marks attendance in DB with confidence score.
    /// CO5: Real-world face recognition application
    /// </summary>
    public class MarkAttendanceForm : Form
    {
        private ComboBox   cmbSubject;
        private PictureBox picCamera;
        private DataGridView dgvToday;
        private Button     btnStart, btnStop;
        private Label      lblStatus, lblCount, lblSubjectHint;
        private Timer      timerCamera;
        private ProgressBar pbConfidence;
        private Label      lblConfLabel;

        private VideoCapture        _camera;
        private FaceEngine          _faceEngine;
        private bool                _running;
        private List<Subject>       _subjects;
        private HashSet<int>        _markedToday = new HashSet<int>(); // StudentIDs marked this session
        private int                 _presentCount;

        private readonly AttendanceRepository _attendRepo  = new AttendanceRepository();
        private readonly StudentRepository    _studentRepo = new StudentRepository();

        public MarkAttendanceForm()
        {
            _faceEngine = new FaceEngine();
            InitializeComponents();
            LoadSubjects();
            TrainRecognizer();
        }

        private void InitializeComponents()
        {
            this.Text       = "Mark Attendance";
            this.BackColor  = Color.FromArgb(240, 242, 247);
            this.Font       = new Font("Segoe UI", 9.5f);
            this.AutoScroll = true;

            var lblTitle = new Label
            {
                Text      = "Mark Attendance — Face Recognition",
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize  = true,
                Location  = new Point(20, 15)
            };

            // ── Subject selector ──────────────────────────────────────────────
            lblSubjectHint = new Label
            {
                Text      = "Select Subject:",
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(70, 80, 100),
                AutoSize  = true,
                Location  = new Point(20, 58)
            };

            cmbSubject = new ComboBox
            {
                Location      = new Point(130, 54),
                Size          = new Size(300, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = new Font("Segoe UI", 9.5f)
            };

            btnStart = MakeButton("▶  Start Camera", new Point(450, 52), new Size(155, 34), Color.FromArgb(37, 99, 235));
            btnStop  = MakeButton("⏹  Stop",         new Point(615, 52), new Size(100, 34), Color.FromArgb(239, 68, 68));
            btnStop.Enabled = false;
            btnStart.Click += BtnStart_Click;
            btnStop.Click  += BtnStop_Click;

            // ── Camera feed ───────────────────────────────────────────────────
            picCamera = new PictureBox
            {
                Location    = new Point(20, 98),
                Size        = new Size(520, 390),
                BackColor   = Color.FromArgb(15, 23, 42),
                SizeMode    = PictureBoxSizeMode.Zoom
            };

            // ── Confidence bar ────────────────────────────────────────────────
            lblConfLabel = new Label
            {
                Text      = "Match confidence:",
                Location  = new Point(20, 498),
                AutoSize  = true,
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(100, 116, 139)
            };

            pbConfidence = new ProgressBar
            {
                Location = new Point(135, 496),
                Size     = new Size(200, 18),
                Minimum  = 0,
                Maximum  = 100,
                Style    = ProgressBarStyle.Continuous
            };

            // ── Status bar ────────────────────────────────────────────────────
            lblStatus = new Label
            {
                Location  = new Point(20, 522),
                Size      = new Size(510, 28),
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(100, 116, 139)
            };

            // ── Today's attendance table ──────────────────────────────────────
            lblCount = new Label
            {
                Text      = "Today's Attendance",
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize  = true,
                Location  = new Point(555, 98)
            };

            dgvToday = new DataGridView
            {
                Location          = new Point(555, 125),
                Size              = new Size(400, 363),
                ReadOnly          = true,
                AllowUserToAddRows= false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor   = Color.White,
                BorderStyle       = BorderStyle.None,
                RowHeadersVisible = false,
                Font              = new Font("Segoe UI", 8.5f),
                GridColor         = Color.FromArgb(230, 232, 240),
                SelectionMode     = DataGridViewSelectionMode.FullRowSelect
            };
            dgvToday.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvToday.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvToday.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgvToday.EnableHeadersVisualStyles              = false;
            dgvToday.Columns.Add("Roll",   "Roll No.");
            dgvToday.Columns.Add("Name",   "Student Name");
            dgvToday.Columns.Add("Time",   "Time");
            dgvToday.Columns.Add("Conf",   "Confidence");

            // Timer
            timerCamera = new Timer { Interval = 33 };
            timerCamera.Tick += TimerCamera_Tick;

            this.Controls.AddRange(new Control[]
            {
                lblTitle, lblSubjectHint, cmbSubject,
                btnStart, btnStop, picCamera,
                lblConfLabel, pbConfidence, lblStatus,
                lblCount, dgvToday
            });
        }

        private void LoadSubjects()
        {
            _subjects = _attendRepo.GetAllSubjects();
            foreach (var s in _subjects)
                cmbSubject.Items.Add(s);
            if (cmbSubject.Items.Count > 0)
                cmbSubject.SelectedIndex = 0;
        }

        private void TrainRecognizer()
        {
            try
            {
                var students = _studentRepo.GetStudentsWithFace();
                MessageBox.Show($"Found {students.Count} students with faces. First path: {(students.Count > 0 ? students[0].FaceImagePath : "none")}");
                _faceEngine.Train(students);
                lblStatus.Text = $"✅ Recognizer trained on {students.Count} students.";
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"⚠ Training warning: {ex.Message}";
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (cmbSubject.SelectedItem == null)
            { MessageBox.Show("Please select a subject first.", "Select Subject"); return; }

            if (!_faceEngine.IsTrained)
            { MessageBox.Show("No students registered. Please register students first."); return; }

            try
            {
                _camera = new VideoCapture(0);
                if (!_camera.IsOpened) throw new Exception("Webcam not accessible.");
                timerCamera.Start();
                _running        = true;
                btnStart.Enabled = false;
                btnStop.Enabled  = true;
                lblStatus.Text   = "🎥 Camera active — system will auto-mark attendance.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Camera error: {ex.Message}");
            }
        }

        private void BtnStop_Click(object sender, EventArgs e) => StopCamera();

        private void TimerCamera_Tick(object sender, EventArgs e)
        {
            var frame = new Mat();
            _camera.Read(frame);
            if (frame.IsEmpty) return;

            var faces = _faceEngine.DetectFaces(frame);
            string   overlayName = "";
            double   bestConf    = 0;
            int      bestStudent = -1;

            foreach (var face in faces)
            {
                var (studentId, confidence) = _faceEngine.Recognize(frame, face);

                if (studentId > 0 && confidence > bestConf)
                {
                    bestConf    = confidence;
                    bestStudent = studentId;
                }
            }

            if (bestStudent > 0)
            {
                var student = _studentRepo.GetByID(bestStudent);
                overlayName = student?.FullName ?? "Unknown";

                // Auto-mark if not already done this session
                if (!_markedToday.Contains(bestStudent))
                {
                    _markedToday.Add(bestStudent);
                    MarkStudentPresent(bestStudent, bestConf, student);
                }
            }

            // Draw overlay
            _faceEngine.DrawOverlay(frame, faces, overlayName, bestConf);

            // Update UI
            pbConfidence.Value = (int)Math.Min(bestConf, 100);
            picCamera.Image?.Dispose();
            picCamera.Image = _faceEngine.MatToBitmap(frame);
        }

        private void MarkStudentPresent(int studentId, double confidence, Student student)
        {
            if (cmbSubject.SelectedItem is not Subject subject) return;

            try
            {
                _attendRepo.MarkAttendance(studentId, subject.SubjectID, "Present", confidence);
                _presentCount++;
                lblStatus.ForeColor = Color.FromArgb(16, 185, 129);
                lblStatus.Text      = $"✅ Marked Present: {student.FullName} ({confidence:0}% match)";

                // Add to grid
                dgvToday.Rows.Add(
                    student.RollNumber,
                    student.FullName,
                    DateTime.Now.ToString("hh:mm tt"),
                    $"{confidence:0}%"
                );

                // Highlight row green
                int lastRow = dgvToday.Rows.Count - 1;
                dgvToday.Rows[lastRow].DefaultCellStyle.BackColor = Color.FromArgb(220, 252, 231);
                lblCount.Text = $"Today's Attendance  ({_presentCount} present)";
            }
            catch (Exception ex)
            {
                lblStatus.ForeColor = Color.FromArgb(239, 68, 68);
                lblStatus.Text      = $"DB error: {ex.Message}";
            }
        }

        private void StopCamera()
        {
            timerCamera.Stop();
            _camera?.Dispose();
            _running          = false;
            btnStart.Enabled  = true;
            btnStop.Enabled   = false;
            picCamera.Image   = null;
            lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
            lblStatus.Text    = "Camera stopped.";
        }

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

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            StopCamera();
            _faceEngine?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
