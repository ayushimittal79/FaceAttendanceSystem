using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using FaceAttendance.Models;

namespace FaceAttendance.FaceRecognition
{
    public class FaceEngine : IDisposable
    {
        private CascadeClassifier    _faceCascade;
        private LBPHFaceRecognizer   _recognizer;
        private Dictionary<int, int> _labelToStudentID;

        private const double CONFIDENCE_THRESHOLD = 40.0;
        private const string CASCADE_PATH = @"Assets\haarcascade_frontalface_default.xml";

        public bool IsTrained => _recognizer != null && _labelToStudentID?.Count > 0;

        public FaceEngine()
        {
            _faceCascade      = new CascadeClassifier(CASCADE_PATH);
            _labelToStudentID = new Dictionary<int, int>();
        }

        public void Train(List<Student> students)
        {
            var images = new List<Mat>();
            var labels = new List<int>();
            _labelToStudentID.Clear();

            int label = 0;
            foreach (var student in students)
            {
                if (student.FaceImagePath == null || !File.Exists(student.FaceImagePath))
                    continue;

                var img = new Mat();
                img = CvInvoke.Imread(student.FaceImagePath, ImreadModes.Grayscale);
                CvInvoke.Resize(img, img, new Size(100, 100));
                images.Add(img);
                labels.Add(label);
                _labelToStudentID[label] = student.StudentID;
                label++;
            }

            if (images.Count == 0) return;

            _recognizer = new LBPHFaceRecognizer(1, 8, 8, 8, 100);
            _recognizer.Train(images.ToArray(), labels.ToArray());
        }

        public Rectangle[] DetectFaces(Mat frame)
        {
            var gray = new Mat();
            CvInvoke.CvtColor(frame, gray, ColorConversion.Bgr2Gray);
            CvInvoke.EqualizeHist(gray, gray);
            return _faceCascade.DetectMultiScale(gray, 1.1, 5, new Size(80, 80));
        }

        public (int StudentID, double Confidence) Recognize(Mat frame, Rectangle faceRect)
        {
            if (!IsTrained) return (-1, 0);

            var gray = new Mat();
            CvInvoke.CvtColor(frame, gray, ColorConversion.Bgr2Gray);
            var faceRegion = new Mat(gray, faceRect);
            CvInvoke.Resize(faceRegion, faceRegion, new Size(100, 100));

            var result = _recognizer.Predict(faceRegion);
            double confidence = Math.Max(0, 100.0 - result.Distance);

            if (confidence < CONFIDENCE_THRESHOLD) return (-1, confidence);
            if (!_labelToStudentID.TryGetValue(result.Label, out int studentId)) return (-1, 0);
            return (studentId, confidence);
        }

        public string CaptureFaceImage(Mat frame, string savePath)
        {
            var faces = DetectFaces(frame);
            if (faces.Length == 0) return null;

            Rectangle best = faces[0];
            foreach (var f in faces)
                if (f.Width * f.Height > best.Width * best.Height) best = f;

            var faceRegion = new Mat(frame, best);
            CvInvoke.Resize(faceRegion, faceRegion, new Size(200, 200));
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            faceRegion.Save(savePath);
            return savePath;
        }

        public void DrawOverlay(Mat frame, Rectangle[] faces, string label, double confidence)
        {
            foreach (var face in faces)
            {
                var color = confidence >= CONFIDENCE_THRESHOLD
                    ? new MCvScalar(0, 255, 0) : new MCvScalar(0, 0, 255);
                CvInvoke.Rectangle(frame, face, color, 2);
                string text = confidence >= CONFIDENCE_THRESHOLD
                    ? $"{label} ({confidence:0}%)" : "Unknown";
                CvInvoke.PutText(frame, text, new Point(face.X, face.Y - 10),
                    FontFace.HersheySimplex, 0.7, color, 2);
            }
        }

        public Bitmap MatToBitmap(Mat frame)
        {
            var bitmap = new Bitmap(frame.Width, frame.Height, frame.Step, System.Drawing.Imaging.PixelFormat.Format24bppRgb, frame.DataPointer);
            return new Bitmap(bitmap);
        }

        public void Dispose()
        {
            _faceCascade?.Dispose();
            _recognizer?.Dispose();
        }
    }
}
