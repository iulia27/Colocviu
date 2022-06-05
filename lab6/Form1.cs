using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lab6
{
    public partial class Form1 : Form
    {
        Image<Bgr, Byte> Myimage;
        public Image<Bgr, byte> Alpha { get; private set; }

        int TotalFrame, FrameNo;
        double Fps;
        bool IsReadingFrame;
        VideoCapture capture;

        private static VideoCapture cameraCapture;
        private Image<Bgr, Byte> newBackgroundImage;
        private static IBackgroundSubtractor fgDetector;


        public Form1()
        {
            InitializeComponent();

            Rectangle rect;
            Point StartROI;
            bool MouseDown;

            if (capture == null)
            {
                return;
            }
            IsReadingFrame = true;
            ReadAllFrames();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog Openfile = new OpenFileDialog();
            if (Openfile.ShowDialog() == DialogResult.OK)
            {
                Myimage = new Image<Bgr, byte>(Openfile.FileName);
                pictureBox1.Image = Myimage.ToBitmap();

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Image<Gray, byte> Grayimage = Myimage.Convert<Gray, byte>();
            pictureBox2.Image = Grayimage.AsBitmap();
            Grayimage[0, 0] = new Gray(200);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            HistogramViewer v = new HistogramViewer();
            v.HistogramCtrl.GenerateHistograms(Myimage, 255);
            v.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var alpha = int.Parse(textBox1.Text);  //0.0-3.0
            var beta = int.Parse(textBox2.Text);   //0-100
            for (int i = 0; i < Myimage.Width; i++)
            {
                for (int j = 0; j < Myimage.Height; j++)
                {
                    Myimage[i, j] = new Bgr(Color.FromArgb(0, 255, 255));
                }
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            float gamma = float.Parse(textBox3.Text);
            var gamma_Picture = Myimage.Clone();
            gamma_Picture._GammaCorrect(gamma);
            pictureBox4.Image = gamma_Picture.AsBitmap();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                var r = Convert.ToDouble(textBox4.Text);
                var localImage = Myimage.Clone();
                localImage = localImage.Resize(r, Emgu.CV.CvEnum.Inter.Linear);
                pictureBox5.Image = Myimage.AsBitmap();
            }
            catch (Exception ex) { }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                var r = Convert.ToDouble(textBox5.Text);
                var localImage = Myimage.Clone();
                localImage = localImage.Rotate(r, new Bgr(), false);
                pictureBox6.Image = localImage.AsBitmap();
            }
            catch (Exception ex) { }
        }
        Rectangle rect;
        Point StartROI;
        bool MouseDown;
        private VideoCapture _cameraCapture;
        private BackgroundSubtractorMOG2 _fgDetector;
        private object camera;

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                return;
            }

            int width = Math.Max(StartROI.X, e.X) - Math.Min(StartROI.X, e.X);
            int height = Math.Max(StartROI.Y, e.Y) - Math.Min(StartROI.Y, e.Y);
            rect = new Rectangle(Math.Min(StartROI.X, e.X),
                Math.Min(StartROI.Y, e.Y),
                width,
                height);
            Refresh();


        }

        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            MouseDown = false;
            if (pictureBox2.Image == null || rect == Rectangle.Empty)
            { return; }

            var img = new Bitmap(pictureBox2.Image).ToImage<Bgr, byte>();
            img.ROI = rect;
            var imgROI = img.Copy();

            pictureBox2.Image = imgROI.ToBitmap();

        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (MouseDown)
            {
                using (Pen pen = new Pen(Color.Red, 1))
                {
                    e.Graphics.DrawRectangle(pen, rect);
                }
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            MouseDown = true;
            StartROI = e.Location;
        }


        private void pictureBox7_Click(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                capture = new VideoCapture(ofd.FileName);
                Mat m = new Mat();
                capture.Read(m);
                pictureBox1.Image = m.ToBitmap();

                TotalFrame = (int)capture.Get(CapProp.FrameCount);

                Fps = capture.Get(CapProp.Fps);
                FrameNo = 1;
                var numericUpDown1 = new NumericUpDown();
                numericUpDown1.Value = FrameNo;
                numericUpDown1.Minimum = 0;
                numericUpDown1.Maximum = TotalFrame;

            }

        }

        private async void button8_Click(object sender, EventArgs e)
        {
            string[] FileNames = Directory.GetFiles(@"F:\img", "*image1.png");
            List<Image<Bgr, byte>> listImages = new List<Image<Bgr, byte>>();
            foreach (var file in FileNames)
            {
                listImages.Add(new Image<Bgr, byte>(file));
            }
            for (int i = 0; i < listImages.Count - 1; i++)
            {
                for (double alpha = 0.0; alpha <= 1.0; alpha += 0.01)
                {
                    pictureBox7.Image = listImages[i + 1].AddWeighted(listImages[i], alpha, 1 - alpha, 0).AsBitmap();
                    await Task.Delay(25);
                }
            }
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {

        }

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                _cameraCapture = new VideoCapture();
                _fgDetector = new BackgroundSubtractorMOG2();
                Application.Idle += ProcessFrames;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                return;
            }

        }

        private void ProcessFrames(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Mat frame = _cameraCapture.QueryFrame();
            Image<Bgr, byte> frameImage = frame.ToImage<Bgr, Byte>();

            Mat foregroundMask = new Mat();
            fgDetector.Apply(frame, foregroundMask);
            var foregroundMaskImage = foregroundMask.ToImage<Gray, Byte>();
            foregroundMaskImage = foregroundMaskImage.Not();

            var copyOfNewBackgroundImage = newBackgroundImage.Resize(foregroundMaskImage.Width, foregroundMaskImage.Height, Inter.Lanczos4);
            copyOfNewBackgroundImage = copyOfNewBackgroundImage.Copy(foregroundMaskImage);

            foregroundMaskImage = foregroundMaskImage.Not();
            frameImage = frameImage.Copy(foregroundMaskImage);
            frameImage = frameImage.Or(copyOfNewBackgroundImage);

        }

        private async void ReadAllFrames()
        {

            Mat m = new Mat();
            while (IsReadingFrame == true && FrameNo < TotalFrame)
            {
                FrameNo += 1;
                var mat = capture.QueryFrame();
                pictureBox1.Image = mat.ToBitmap();
                await Task.Delay(1000 / Convert.ToInt16(Fps));
                label1.Text = FrameNo.ToString() + "/" + TotalFrame.ToString();
            }
        }

    }
}
