using System;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Emgu.CV.CvEnum;

namespace Computer_Vision
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
  
        private Image<Bgr, byte> sourceImage;
        private Image<Bgr, byte> sourceImage2;

        private void inspectionFruit(string imagePath)
        {
            // อ่านรูปผลไม้
            sourceImage = new Image<Bgr, byte>(imagePath);
            sourceImage2 = new Image<Bgr, byte>(imagePath);

            // ทำการประมวลผลเพื่อแยกส่วนที่ดีและเน่าของผลไม้
            Mat src = sourceImage.Mat;
            Mat thr = new Mat();
            CvInvoke.CvtColor(src, thr, ColorConversion.Bgr2Gray);
            CvInvoke.Threshold(thr, thr, 0, 255, ThresholdType.Otsu);

            // Convert BGR to HSV
            Mat HSV = new Mat();
            CvInvoke.CvtColor(src, HSV, ColorConversion.Bgr2Hsv);

            // Thresholding in HSV color space
            Mat hsv_thr = new Mat();
            CvInvoke.InRange(HSV, new ScalarArray(new MCvScalar(20, 10, 10)), 
                new ScalarArray(new MCvScalar(90, 255, 255)), hsv_thr);

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hierarchy = new Mat();
            int largest_contour_index = 0;
            double largest_area = 0;
            Mat mask = new Mat(src.Rows, src.Cols, DepthType.Cv8U, 1);
            CvInvoke.FindContours(thr.Clone(), contours, hierarchy, 
                RetrType.External, ChainApproxMethod.ChainApproxSimple);

            for (int i = 0; i < contours.Size; i++)
            {
                double a = CvInvoke.ContourArea(contours[i], false);
                if (a > largest_area)
                {
                    largest_area = a;
                    largest_contour_index = i;
                }
            }

            CvInvoke.DrawContours(mask, contours, largest_contour_index, new MCvScalar(255, 255, 255), -1, LineType.EightConnected, hierarchy);
            int dilation_size = 2;
            Mat element = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(2 * dilation_size + 1, 2 * dilation_size + 1), new Point(dilation_size, dilation_size));
            CvInvoke.Erode(mask, mask, element, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));

            // Apply bitwise_not
            Mat dst = new Mat();
            CvInvoke.BitwiseNot(hsv_thr, dst, mask);

            // Find contours in the inverted image
            VectorOfVectorOfPoint contours_dst = new VectorOfVectorOfPoint();
            Mat hierarchy_dst = new Mat();
            CvInvoke.FindContours(dst.Clone(), contours_dst, hierarchy_dst, 
                RetrType.External, ChainApproxMethod.ChainApproxSimple);

            // Draw contours on the original image
            for (int i = 0; i < contours_dst.Size; i++)
            {
                CvInvoke.DrawContours(src, contours_dst, i, 
                    new MCvScalar(0, 0, 255), 1, 
                    LineType.EightConnected, hierarchy_dst);
            }

            // ปรับขนาดรูปให้พอดีกับ PictureBox
            pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox5.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox6.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox7.SizeMode = PictureBoxSizeMode.StretchImage;

            // แสดงรูปภาพผลลัพธ์
            pictureBox4.Image = sourceImage2.ToBitmap(); //แสดงรูปแอปเปิ้ล
            pictureBox5.Image = dst.ToImage<Bgr, byte>().ToBitmap(); // แสดงส่วนที่ดีของแอปเปิ้ล
            pictureBox6.Image = hsv_thr.ToImage<Bgr, byte>().ToBitmap(); // แสดงส่วนที่เน่าของแอปเปิ้ล
            pictureBox7.Image = sourceImage.ToBitmap(); // แสดงส่วนที่เน่าของแอปเปิ้ล RGB
        }


        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Title = "Select Image File";
            openFileDialog1.Filter = "Image Files (*.jpg; *.jpeg; *.png; *.gif)|*.jpg; *.jpeg; *.png; *.gif";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string ImagePath = openFileDialog1.FileName;
                inspectionFruit(ImagePath);
            }
        }

    }
}