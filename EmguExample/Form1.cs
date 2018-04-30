using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.IO.Ports;

namespace EmguExample
{
    public partial class Form1 : Form
    {
        private VideoCapture _capture;
        private Thread _captureThread;
        SerialPort _serialPort = new SerialPort("COM6", 2400);   //***IMPORTANT***



        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _capture = new VideoCapture();
            _captureThread = new Thread(DisplayWebcam);
            _captureThread.Start();

            _serialPort.DataBits = 8;            //***IMPORTANT***
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.Two;
            _serialPort.Open();
        }

        private int dakka = 165;
        private int red1 = 150;
        private int red2 = 110;
        private int redmode = 0;
        private int redtime = 20;

        const byte STOP = 0x7F;
        const byte FLOAT = 0x0F;
        const byte FORWARD = 0x6f;
        const byte BACKWARD = 0x5F;

        private void DisplayWebcam()
        {
            while (_capture.IsOpened)
            {
                Mat frame = _capture.QueryFrame();
                CvInvoke.Resize(frame, frame, pictureBox1.Size);
                CvInvoke.Flip(frame,frame,FlipType.Horizontal);
                //CvInvoke.BitwiseNot(frame, frame); //XXXXXXXXX

                Image<Gray, Byte> img = frame.ToImage<Gray, Byte>();
                Image<Bgr, Byte> img2 = frame.ToImage<Bgr, Byte>();



                byte left = FORWARD;    //***IMPORTANT***
                byte right = FORWARD;



                //Image<Gray, Byte> blues = imgBgr[0];

                int redCount = 0;
                for (int x = 0; x < img2.Width; x++)
                {
                    for (int y = 0; y < img2.Height; y++)
                    {
                        if (img2.Data[y, x, 2] >= red1 && img2.Data[y, x, 0] <= red2 && img2.Data[y, x, 1] <= red2)
                        {
                            redCount++;
                            img2.Data[y, x, 2] = 255;
                        }
                        else
                        {
                            img2.Data[y, x, 2] = 0;
                        }
                        img2.Data[y, x, 0] = 0;
                        img2.Data[y, x, 1] = 0;
                    }
                }
                int redbox = 800;
                //int redmode = 0;
                if(redCount >= redbox)
                {
                    redmode = redtime;
                    this.Invoke(new Action(() => label5.Text = "RED"));
                    left = FORWARD;
                    right = STOP;           //***IMPORTANT***
                    move(left, right);
                }
                if (redmode >= 1)
                {
                    redmode--;
                    this.Invoke(new Action(() => label5.Text = "RED"));
                    left = FORWARD;
                    right = STOP;              //***IMPORTANT***
                    move(left, right);
                }
                else
                {
                    this.Invoke(new Action(() => label5.Text = "NULL"));
                }

                pictureBox2.Image = img2.Bitmap;

                img = img.ThresholdBinary(new Gray(dakka), new Gray(255));

                /*
                img = img.SmoothGaussian(5);
                img = (img.Sobel(1, 0, 5).AbsDiff(new Gray(0))
                    + img.Sobel(0, 1, 5).AbsDiff(new Gray(0))).Convert<Gray, Byte>();
                img = img.Canny(100, 200);
                */

                //img = img.Erode(1); //************Had Previously************

                int white = img.CountNonzero()[0], tot = img.Width * img.Height;



                int limit1 = (img.Width / 3) * 1;
                int limit2 = (img.Width / 3) * 2;
                //int limit15 = (img.Width / 5);
                //int limit45 = (img.Width / 5) * 4;

                this.Invoke(new Action(() => label2.Text = $"{dakka}"));
                this.Invoke(new Action(() => label3.Text = $"{red1}"));
                this.Invoke(new Action(() => label4.Text = $"{red2}"));
                this.Invoke(new Action(() => label6.Text = $"{redmode}"));

                int leftWhiteCount = 0;
                for (int x = 0; x < limit1; x++)
                {
                    for (int y = 0; y < img.Height; y++)
                    {
                        if (img.Data[y, x, 0] == 255)
                            leftWhiteCount++;
                    }
                }

                int rightWhiteCount = 0;
                for (int x = limit2; x < img.Width; x++)
                {
                    for (int y = 0; y < img.Height; y++)
                    {
                        if (img.Data[y, x, 0] == 255)
                            rightWhiteCount++;
                    }
                }

                int middleWhiteCount = 0;
                for (int x = limit1; x < limit2; x++)
                {
                    for (int y = 0; y < img.Height; y++)
                    {
                        if (img.Data[y, x, 0] == 255)
                            middleWhiteCount++;
                    }
                }

                if(redmode < 1)
                {
                // > >
                if(leftWhiteCount < rightWhiteCount && leftWhiteCount < middleWhiteCount)          //***IMPORTANT***
                {
                    this.Invoke(new Action(() => label1.Text = "Left"));
                    left = STOP;
                    right = FORWARD;              //***IMPORTANT***
                    move(left, right);
                }
                // < >
                else if(leftWhiteCount > rightWhiteCount && rightWhiteCount < middleWhiteCount)
                {
                    this.Invoke(new Action(() => label1.Text = "right"));
                    left = FORWARD;
                    right = STOP;           //***IMPORTANT***
                    move(left, right);
                }
                else
                {
                    this.Invoke(new Action(() => label1.Text = "forward"));
                    left = FORWARD;
                    right = FORWARD;           //***IMPORTANT***
                    move(left, right);
                }
                }
                else
                {
                    this.Invoke(new Action(() => label1.Text = "RED"));
                }



                /*
                if(white > (tot/2))
                {
                    BackColor = Color.FromArgb(255, 0, 0);
                }
                else
                {
                    BackColor = Color.FromArgb(0, 255, 0);
                }
                */

                pictureBox1.Image = img.Bitmap;


                //Image<Gray, Byte> img2 = img.Convert<Gray, Byte>();
                //img = img.ThresholdBinary(new Gray(100), new Gray(255));
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _captureThread.Abort();
        }
        
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void slider_Scroll(object sender, EventArgs e)
        {
            dakka = slider.Value;
        }

        private void slider2_Scroll(object sender, EventArgs e)
        {
            red1 = slider2.Value;
        }

        private void slider3_Scroll(object sender, EventArgs e)
        {
            red2 = slider3.Value;
        }

        private void slider3_Scroll_1(object sender, EventArgs e)
        {
            red2 = slider3.Value;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            int value = (int)numericUpDown1.Value;
            redtime = value;
        }



        
        private void move(byte left, byte right)       //***IMPORTANT***
        {
            byte[] buffer = { 0x01, left, right };
            _serialPort.Write(buffer, 0, 3);
        }
        
    }
}
