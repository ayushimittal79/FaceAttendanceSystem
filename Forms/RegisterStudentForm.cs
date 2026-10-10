using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using FaceAttendance.Database;
using FaceAttendance.FaceRecognition;
using FaceAttendance.Models;

namespace FaceAttendance.Forms
{
    /// <summary>
    /// Register a new student with live webcam face capture.
    /// CO2: Windows Forms, Timer, event-driven
    /// CO3: ADO.NET INSERT
    /// CO5: Face capture via Emgu CV
    /// </summary>
    public class RegisterStudentForm : Form
    {
        // Controls
        private TextBox  txtRoll, txtName, txtEmail, txtBranch;
        private ComboBox cmbSemester;
        private PictureBox picPreview, picCapture;
        private Button   btnStartCamera, btnCapture, btnRegister, btnClear;
        private Label    lblStatus;
        private Timer    timerCamera;

        // State
        private VideoCapture _camera;
        private Mat          _lastFrame;
        private FaceEngine   _faceEngine;
        private bool         _cameraRunning;
        private string       _capturedImagePath;

        private readonly StudentRepository _studentRepo = new StudentRepository();

        public RegisterStudentForm()
        {
            _faceEngine = new FaceEngine();
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text      = "Register Student";
            this.BackColor = Color.FromArgb(240, 242, 247);
            this.Font      = new Font("Segoe UI", 9.5f);
            this.AutoScroll = true;

            // ── Title ─────────────────────────────────────────────────────────
            var lblTitle = new Label
            {
                Text      = "Register New Student",
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize  = true,
                Location  = new Point(20, 15)
            };

            // ── Left panel: form fields ────────────────────────────────────────
            var pnlForm = new Panel { Location = new Point(20, 55), Size = new Size(380, 400), BackColor = Color.White };
            pnlForm.Paint += PaintCardBorder;

            int y = 20;
            AddFormField(pnlForm, "Roll Number *",  ref y, out txtRoll);
            AddFormField(pnlForm, "Full Name *",    ref y, out txtName);
            AddFormField(pnlForm, "Email",          ref y, out txtEmail);
            AddFormField(pnlForm, "Branch",         ref y, out txtBranch);

            var lblSem = MakeLabel("Semester *", new Point(20, y));
            pnlForm.Controls.Add(lblSem);
            y += 22;
            cmbSemester = new ComboBox
            {
                Location = new Point(20, y),
                Size     = new Size(340, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            for (int i = 1; i <= 8; i++) cmbSemester.Items.Add($"Semester {i}");
            cmbSemester.SelectedIndex = 3; // Default: 4th sem
            pnlForm.Controls.Add(cmbSemester);
            y += 45;

            btnRegister = MakeButton("✅  Register Student", new Point(20, y), new Size(340, 42),
                Color.FromArgb(16, 185, 129));
            btnRegister.Click += BtnRegister_Click;
            pnlForm.Controls.Add(btnRegister);

            btnClear = MakeButton("🔄  Clear Form", new Point(20, y + 50), new Size(340, 36),
                Color.FromArgb(100, 116, 139));
            btnClear.Click += (s, e) => ClearForm();
            pnlForm.Controls.Add(btnClear);

            // ── Right panel: webcam ────────────────────────────────────────────
            var pnlCam = new Panel { Location = new Point(420, 55), Size = new Size(440, 400), BackColor = Color.White };
            pnlCam.Paint += PaintCardBorder;

            var lblCam = MakeLabel("Face Capture", new Point(15, 15));
            lblCam.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            pnlCam.Controls.Add(lblCam);

            picPreview = new PictureBox
            {
                Location    = new Point(15, 40),
                Size        = new Size(260, 195),
                BackColor   = Color.FromArgb(15, 23, 42),
                SizeMode    = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.None
            };
            var lblCamHint = new Label
            {
                Text      = "Camera preview",
                ForeColor = Color.Gray,
                AutoSize  = true,
                Location  = new Point(70, 100)
            };
            picPreview.Controls.Add(lblCamHint);
            pnlCam.Controls.Add(picPreview);

            picCapture = new PictureBox
            {
                Location    = new Point(285, 40),
                Size        = new Size(140, 105),
                BackColor   = Color.FromArgb(240, 242, 247),
                SizeMode    = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle
            };
            var lblCapHint = new Label
            {
                Text      = "Captured\nface",
                ForeColor = Color.Gray,
                AutoSize  = true,
                Location  = new Point(30, 35)
            };
            picCapture.Controls.Add(lblCapHint);
            pnlCam.Controls.Add(picCapture);

            btnStartCamera = MakeButton("📷  Start Camera", new Point(15, 248), new Size(195, 40),
                Color.FromArgb(37, 99, 235));
            btnStartCamera.Click += BtnStartCamera_Click;
            pnlCam.Controls.Add(btnStartCamera);

            btnCapture = MakeButton("✂  Capture Face", new Point(220, 248), new Size(195, 40),
                Color.FromArgb(245, 158, 11));
            btnCapture.Enabled = false;
            btnCapture.Click   += BtnCapture_Click;
            pnlCam.Controls.Add(btnCapture);

            lblStatus = new Label
            {
                Location  = new Point(15, 300),
                Size      = new Size(400, 30),
                Font      = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(100, 116, 139)
            };
            pnlCam.Controls.Add(lblStatus);

            // ── Camera timer ──────────────────────────────────────────────────
            timerCamera = new Timer { Interval = 33 }; // ~30 FPS
            timerCamera.Tick += TimerCamera_Tick;

            this.Controls.AddRange(new Control[] { lblTitle, pnlForm, pnlCam });
        }

        // ── Camera logic ──────────────────────────────────────────────────────
        private void BtnStartCamera_Click(object sender, EventArgs e)
        {
            if (_cameraRunning)
            {
                StopCamera();
                btnStartCamera.Text = "📷  Start Camera";
                btnCapture.Enabled  = false;
            }
            else
            {
                try
                {
                    _camera = new VideoCapture(0);
                    if (!_camera.IsOpened) throw new Exception("Webcam not found.");
                    timerCamera.Start();
                    _cameraRunning      = true;
                    btnStartCamera.Text = "⏹  Stop Camera";
                    btnCapture.Enabled  = true;
                    lblStatus.Text      = "Camera running. Position face in frame.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Camera error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void TimerCamera_Tick(object sender, EventArgs e)
        {
            _lastFrame = new Mat();
            _camera.Read(_lastFrame);
            if (_lastFrame.IsEmpty) return;

            // Detect faces and draw overlay
            var faces = _faceEngine.DetectFaces(_lastFrame);
            foreach (var face in faces)
                CvInvoke.Rectangle(_lastFrame, face, new MCvScalar(0, 200, 100), 2);

            picPreview.Image?.Dispose();
            picPreview.Image = _faceEngine.MatToBitmap(_lastFrame);
        }

        private void BtnCapture_Click(object sender, EventArgs e)
        {
            if (_lastFrame == null || _lastFrame.IsEmpty) return;

            string roll = txtRoll.Text.Trim();
            if (string.IsNullOrEmpty(roll)) { lblStatus.Text = "⚠ Enter roll number first."; return; }

            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FaceData");
            string path = Path.Combine(dir, $"{roll}.jpg");

            _capturedImagePath = _faceEngine.CaptureFaceImage(_lastFrame, path);

            if (_capturedImagePath != null)
            {
                picCapture.Image?.Dispose();
                picCapture.Image = Image.FromFile(_capturedImagePath);
                lblStatus.ForeColor = Color.FromArgb(16, 185, 129);
                lblStatus.Text      = "✅ Face captured successfully!";
            }
            else
            {
                lblStatus.ForeColor = Color.FromArgb(239, 68, 68);
                lblStatus.Text      = "✗ No face detected. Adjust position and try again.";
            }
        }

        // ── Register student ──────────────────────────────────────────────────
        private void BtnRegister_Click(object sender, EventArgs e)
        {
            if (!ValidateForm()) return;

            if (_capturedImagePath == null)
            {
                MessageBox.Show("Please capture a face photo before registering.",
                    "Face Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_studentRepo.RollNumberExists(txtRoll.Text.Trim()))
            {
                MessageBox.Show("A student with this roll number already exists.",
                    "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Read face image as bytes for DB storage
                byte[] faceBytes = File.ReadAllBytes(_capturedImagePath);

                var student = new Student
                {
                    RollNumber    = txtRoll.Text.Trim().ToUpper(),
                    FullName      = txtName.Text.Trim(),
                    Email         = txtEmail.Text.Trim(),
                    Branch        = txtBranch.Text.Trim(),
                    Semester      = cmbSemester.SelectedIndex + 1,
                    FaceImagePath = _capturedImagePath,
                    FaceEncoding  = faceBytes
                };

                int newId = _studentRepo.AddStudent(student);
                StopCamera();

                MessageBox.Show($"✅ Student registered successfully!\nID: {newId}",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Registration failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtRoll.Text)) { ShowError("Roll Number is required."); return false; }
            if (string.IsNullOrWhiteSpace(txtName.Text)) { ShowError("Full Name is required."); return false; }
            return true;
        }

        private void ShowError(string msg) =>
            MessageBox.Show(msg, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        private void ClearForm()
        {
            txtRoll.Clear(); txtName.Clear(); txtEmail.Clear(); txtBranch.Clear();
            cmbSemester.SelectedIndex = 3;
            picCapture.Image = null;
            _capturedImagePath = null;
            lblStatus.Text = "";
        }

        private void StopCamera()
        {
            timerCamera.Stop();
            _camera?.Dispose();
            _cameraRunning = false;
            picPreview.Image = null;
        }

        private void AddFormField(Panel parent, string labelText, ref int y, out TextBox tb)
        {
            parent.Controls.Add(MakeLabel(labelText, new Point(20, y)));
            y += 22;
            tb = new TextBox
            {
                Location    = new Point(20, y),
                Size        = new Size(340, 28),
                Font        = new Font("Segoe UI", 9.5f),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor   = Color.FromArgb(248, 249, 252)
            };
            parent.Controls.Add(tb);
            y += 42;
        }

        private Label MakeLabel(string text, Point loc) => new Label
        {
            Text = text, Location = loc, AutoSize = true,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(70, 80, 100)
        };

        private Button MakeButton(string text, Point loc, Size size, Color color)
        {
            var btn = new Button
            {
                Text      = text,
                Location  = loc,
                Size      = size,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void PaintCardBorder(object sender, PaintEventArgs e)
        {
            var p = (Panel)sender;
            using (var pen = new Pen(Color.FromArgb(220, 220, 235), 1))
                e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, p.Width - 1, p.Height - 1));
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            StopCamera();
            _faceEngine?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
