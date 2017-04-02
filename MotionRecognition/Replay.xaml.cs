using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MotionRecognition
{
    /// <summary>
    /// Replay.xaml 的互動邏輯
    /// </summary>
    public partial class Replay : Window
    {
        struct MotionFeature
        {
            public List<Point> Joint_0 { get; set; }
            public List<Point> Joint_1 { get; set; }
            public List<Point> Joint_2 { get; set; }
            public List<Point> Joint_3 { get; set; }
            public List<Point> Joint_4 { get; set; }
            public List<Point> Joint_5 { get; set; }
            public List<Point> Joint_6 { get; set; }
            public List<Point> Joint_7 { get; set; }
            public List<Point> Joint_8 { get; set; }
            public List<Point> Joint_9 { get; set; }
            public List<Point> Joint_10 { get; set; }
            public List<Point> Joint_11 { get; set; }
            public List<Point> Joint_12 { get; set; }
            public List<Point> Joint_13 { get; set; }
            public List<Point> Joint_14 { get; set; }
            public List<Point> Joint_15 { get; set; }
            public List<Point> Joint_16 { get; set; }
            public List<Point> Joint_17 { get; set; }
            public List<Point> Joint_18 { get; set; }
            public List<Point> Joint_19 { get; set; }
            public List<Point> Joint_20 { get; set; }
            public List<Point> Joint_21 { get; set; }
            public List<Point> Joint_22 { get; set; }
            public List<Point> Joint_23 { get; set; }
            public List<Point> Joint_24 { get; set; }
            public string MotionName { get; set; }
        }
        struct MotionClass
        {
            public MotionFeature[] motionFeature { get; set; }
            public string MotionName { get; set; }
        }

        private static string FilePath = System.IO.Directory.GetCurrentDirectory() + "\\";
        private MotionClass[] motionclass;
        DispatcherTimer DrawTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(30) };
        private int NowMotionNum = 0;
        private int NowClassNum = 0;
        private int TimerCount = 0;

        double scaleX;
        double scaleY;

        public Replay()
        {
            InitializeComponent();

            scaleX = (double)512 / BodyCanvas.Width;
            scaleY = (double)484 / BodyCanvas.Height;

            LoadMotion();
            for (int i = 0; i < motionclass.Length; i++)
            {
                ClassListBox.Items.Add(motionclass[i].MotionName);
            }
            ClassListBox.SelectionChanged += ClassListBox_SelectionChanged;

            DrawTimer.Tick += DrawTimer_Tick;

        }

        void DrawTimer_Tick(object sender, EventArgs e)
        {
            if (TimerCount < motionclass[NowClassNum].motionFeature[NowMotionNum].Joint_0.Count)
            {
                MotionCanvas(motionclass[NowClassNum].motionFeature[NowMotionNum], BodyCanvas, TimerCount);

                TimerCount++;
            }
            else
            {
                DrawTimer.Stop();
            }
            
        }

        void ClassListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectNum = (sender as ListBox).SelectedIndex;
            if (selectNum != -1)//ListBox沒有反白的選項時
            {
                NumMotionListBox.Items.Clear();
                int count = 0;
                for (int i = 0; i < motionclass[selectNum].motionFeature.Length; i++)
                {
                    NumMotionListBox.Items.Add(motionclass[selectNum].MotionName + count.ToString());
                    count++;
                }
                NumMotionListBox.SelectionChanged += NumMotionListBox_SelectionChanged;
                NowClassNum = selectNum;
            }
        }

        void NumMotionListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectNum = (sender as ListBox).SelectedIndex;
            if (selectNum != -1)
            {
                NowMotionNum = selectNum;
                //saveImg();
                DrawTimer.Start();
                TimerCount = 0;
            }
            else
            {
                DrawTimer.Stop();
            }
            
        }
        private void MotionCanvas(MotionFeature motion, Canvas bodycanvas, int count)
        {
            bodycanvas.Children.Clear();

            if (count < motion.Joint_0.Count)
            {
                CanvasAddBone(bodycanvas, motion.Joint_3[count], motion.Joint_2[count],8, Brushes.Gray);
                CanvasAddBone(bodycanvas, motion.Joint_2[count], motion.Joint_20[count],8, Brushes.Gray);
                CanvasAddBone(bodycanvas, motion.Joint_20[count], motion.Joint_1[count],30, Brushes.Gray);
                CanvasAddBone(bodycanvas, motion.Joint_1[count], motion.Joint_0[count],30, Brushes.Gray);
                CanvasAddBone(bodycanvas, motion.Joint_20[count], motion.Joint_8[count],6, Brushes.Gray);
                CanvasAddBone(bodycanvas, motion.Joint_20[count], motion.Joint_4[count], 6, Brushes.Gray);
                CanvasAddBone(bodycanvas, motion.Joint_0[count], motion.Joint_16[count], 6, Brushes.Gray);
                CanvasAddBone(bodycanvas, motion.Joint_0[count], motion.Joint_12[count], 6, Brushes.Gray);

                CanvasAddBone(bodycanvas, motion.Joint_8[count], motion.Joint_9[count], 8, Brushes.CadetBlue);
                CanvasAddBone(bodycanvas, motion.Joint_9[count], motion.Joint_10[count], 8, Brushes.CadetBlue);
                CanvasAddBone(bodycanvas, motion.Joint_10[count], motion.Joint_11[count], 8, Brushes.CadetBlue);
                CanvasAddBone(bodycanvas, motion.Joint_11[count], motion.Joint_23[count], 3, Brushes.CadetBlue);
                CanvasAddBone(bodycanvas, motion.Joint_10[count], motion.Joint_24[count], 3, Brushes.CadetBlue);

                CanvasAddBone(bodycanvas, motion.Joint_4[count], motion.Joint_5[count], 8, Brushes.IndianRed);
                CanvasAddBone(bodycanvas, motion.Joint_5[count], motion.Joint_6[count], 8, Brushes.IndianRed);
                CanvasAddBone(bodycanvas, motion.Joint_6[count], motion.Joint_7[count], 8, Brushes.IndianRed);
                CanvasAddBone(bodycanvas, motion.Joint_7[count], motion.Joint_21[count], 3, Brushes.IndianRed);
                CanvasAddBone(bodycanvas, motion.Joint_6[count], motion.Joint_22[count], 3, Brushes.IndianRed);

                CanvasAddBone(bodycanvas, motion.Joint_16[count], motion.Joint_17[count], 10, Brushes.DarkGreen);
                CanvasAddBone(bodycanvas, motion.Joint_17[count], motion.Joint_18[count], 10, Brushes.DarkGreen);
                CanvasAddBone(bodycanvas, motion.Joint_18[count], motion.Joint_19[count], 10, Brushes.DarkGreen);

                CanvasAddBone(bodycanvas, motion.Joint_13[count], motion.Joint_12[count], 10, Brushes.DarkViolet);
                CanvasAddBone(bodycanvas, motion.Joint_13[count], motion.Joint_14[count], 10, Brushes.DarkViolet);
                CanvasAddBone(bodycanvas, motion.Joint_14[count], motion.Joint_15[count], 10, Brushes.DarkViolet);

                System.Windows.Shapes.Ellipse Head = new System.Windows.Shapes.Ellipse { Width = 32, Height = 32, Fill = new SolidColorBrush(Color.FromArgb(255, 50, 50, 50)) };
                bodycanvas.Children.Add(Head);
                Canvas.SetLeft(Head, motion.Joint_3[count].X / scaleX - 13);
                Canvas.SetTop(Head, motion.Joint_3[count].Y / scaleY - 13);
            }
                
        }
        private void CanvasAddBone(Canvas bodycanvas, Point dsp1, Point dsp2, double thickness, SolidColorBrush brush)
        {
            System.Windows.Shapes.Line bone = new System.Windows.Shapes.Line { X1 = dsp1.X / scaleX, X2 = dsp2.X / scaleX, Y1 = dsp1.Y / scaleY, Y2 = dsp2.Y / scaleY, StrokeThickness = thickness, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Stroke = brush };
            bodycanvas.Children.Add(bone);
        }
        private void LoadMotion()
        {
            //資料夾路徑
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(FilePath + "動作類別");
            //取得資料夾內該層的所有資料夾
            System.IO.DirectoryInfo[] fileNames = di.GetDirectories("*.*", System.IO.SearchOption.TopDirectoryOnly);
            motionclass = new MotionClass[fileNames.Length];

            for (int i = 0; i < fileNames.Length; i++)//類別的數量
            {
                DirectoryInfo Dir = new DirectoryInfo(FilePath + "動作類別\\" + fileNames[i].ToString() + "\\");
                int motionAmount = Dir.GetFiles("*.position2D").Length;
                MotionFeature[] MFs = new MotionFeature[motionAmount];

                for (int m = 1; m <= motionAmount; m++)//類別內的動作數量
                {
                    string text = LoadFile(FilePath + "動作類別\\" + fileNames[i].ToString() + "\\" + m.ToString() + ".position2D");
                    string[] All_Frame = text.Split(new Char[] { '#' });
                    MotionFeature MF = new MotionFeature
                    {
                        Joint_0 = new List<Point>(),
                        Joint_1 = new List<Point>(),
                        Joint_2 = new List<Point>(),
                        Joint_3 = new List<Point>(),
                        Joint_4 = new List<Point>(),
                        Joint_5 = new List<Point>(),
                        Joint_6 = new List<Point>(),
                        Joint_7 = new List<Point>(),
                        Joint_8 = new List<Point>(),
                        Joint_9 = new List<Point>(),
                        Joint_10 = new List<Point>(),
                        Joint_11 = new List<Point>(),
                        Joint_12 = new List<Point>(),
                        Joint_13 = new List<Point>(),
                        Joint_14 = new List<Point>(),
                        Joint_15 = new List<Point>(),
                        Joint_16 = new List<Point>(),
                        Joint_17 = new List<Point>(),
                        Joint_18 = new List<Point>(),
                        Joint_19 = new List<Point>(),
                        Joint_20 = new List<Point>(),
                        Joint_21 = new List<Point>(),
                        Joint_22 = new List<Point>(),
                        Joint_23 = new List<Point>(),
                        Joint_24 = new List<Point>(),
                        MotionName = fileNames[i].ToString()
                    };
                    for (int j = 0; j < All_Frame.Length; j++)//Joint的數量
                    {
                        string[] A_Frame = All_Frame[j].Split(new Char[] { '@' });
                        for (int k = 0; k < A_Frame.Length; k++)//Frame的數量
                        {
                            string[] xy = A_Frame[k].Split(new Char[] { ',' });
                            switch (j)
                            {
                                case 0:
                                    MF.Joint_0.Add(new Point { X = (float)Convert.ToDouble(xy[0]), Y = (float)Convert.ToDouble(xy[1])});
                                    break;
                                case 1:
                                    MF.Joint_1.Add(new Point { X = (float)Convert.ToDouble(xy[0]), Y = (float)Convert.ToDouble(xy[1]) });
                                    break;
                                case 2:
                                    MF.Joint_2.Add(new Point { X = (float)Convert.ToDouble(xy[0]), Y = (float)Convert.ToDouble(xy[1]) });
                                    break;
                                case 3:
                                    MF.Joint_3.Add(new Point { X = (float)Convert.ToDouble(xy[0]), Y = (float)Convert.ToDouble(xy[1]) });
                                    break;
                                case 4:
                                    MF.Joint_4.Add(new Point { X = (float)Convert.ToDouble(xy[0]), Y = (float)Convert.ToDouble(xy[1]) });
                                    break;
                                case 5:
                                    MF.Joint_5.Add(new Point { X = (float)Convert.ToDouble(xy[0]), Y = (float)Convert.ToDouble(xy[1]) });
                                    break;
                                case 6:
                                    MF.Joint_6.Add(new Point { X = (float)Convert.ToDouble(xy[0]), Y = (float)Convert.ToDouble(xy[1]) });
                                    break;
                                case 7:
                                    MF.Joint_7.Add(new Point { X = (float)Convert.ToDouble(xy[0]), Y = (float)Convert.ToDouble(xy[1]) });
                                    break;
                                case 8:
                                    MF.Joint_8.Add(new Point { X = (float)Convert.ToDouble(xy[0]), Y = (float)Convert.ToDouble(xy[1]) });
                                    break;
                                case 9:
                                    MF.Joint_9.Add(new Point { X = (float)Convert.ToDouble(xy[0]), Y = (float)Convert.ToDouble(xy[1]) });
                                    break;
                                case 10:
                                    MF.Joint_10.Add(new Point { X = (float)Convert.ToDouble(xy[0]), Y = (float)Convert.ToDouble(xy[1]) });
                                    break;
                                case 11:
                                    MF.Joint_11.Add(new Point { X = (float)Convert.ToDouble(xy[0]), Y = (float)Convert.ToDouble(xy[1]) });
                                    break;
                                case 12:
                                    MF.Joint_12.Add(new Point { X = (float)Convert.ToDouble(xy[0]), Y = (float)Convert.ToDouble(xy[1]) });
                                    break;
                                case 13:
                                    MF.Joint_13.Add(new Point { X = (float)Convert.ToDouble(xy[0]), Y = (float)Convert.ToDouble(xy[1]) });
                                    break;
                                case 14:
                                    MF.Joint_14.Add(new Point { X = (float)Convert.ToDouble(xy[0]), Y = (float)Convert.ToDouble(xy[1]) });
                                    break;
                                case 15:
                                    MF.Joint_15.Add(new Point { X = (float)Convert.ToDouble(xy[0]), Y = (float)Convert.ToDouble(xy[1]) });
                                    break;
                                case 16:
                                    MF.Joint_16.Add(new Point { X = (float)Convert.ToDouble(xy[0]), Y = (float)Convert.ToDouble(xy[1]) });
                                    break;
                                case 17:
                                    MF.Joint_17.Add(new Point { X = (float)Convert.ToDouble(xy[0]), Y = (float)Convert.ToDouble(xy[1]) });
                                    break;
                                case 18:
                                    MF.Joint_18.Add(new Point { X = (float)Convert.ToDouble(xy[0]), Y = (float)Convert.ToDouble(xy[1]) });
                                    break;
                                case 19:
                                    MF.Joint_19.Add(new Point { X = (float)Convert.ToDouble(xy[0]), Y = (float)Convert.ToDouble(xy[1]) });
                                    break;
                                case 20:
                                    MF.Joint_20.Add(new Point { X = (float)Convert.ToDouble(xy[0]), Y = (float)Convert.ToDouble(xy[1]) });
                                    break;
                                case 21:
                                    MF.Joint_21.Add(new Point { X = (float)Convert.ToDouble(xy[0]), Y = (float)Convert.ToDouble(xy[1]) });
                                    break;
                                case 22:
                                    MF.Joint_22.Add(new Point { X = (float)Convert.ToDouble(xy[0]), Y = (float)Convert.ToDouble(xy[1]) });
                                    break;
                                case 23:
                                    MF.Joint_23.Add(new Point { X = (float)Convert.ToDouble(xy[0]), Y = (float)Convert.ToDouble(xy[1]) });
                                    break;
                                case 24:
                                    MF.Joint_24.Add(new Point { X = (float)Convert.ToDouble(xy[0]), Y = (float)Convert.ToDouble(xy[1]) });
                                    break;
                                default:
                                    MessageBox.Show("讀檔錯誤");
                                    break;
                            }

                        }
                    }
                    MFs[m - 1] = MF;
                }

                motionclass[i].motionFeature = MFs;
                motionclass[i].MotionName = fileNames[i].ToString();
            }
        }
        private string LoadFile(string FileName)
        {
            using (StreamReader sr = new StreamReader(FileName))
            {
                string line = sr.ReadLine();
                return line;
            }
        }
        private void saveImg()
        {
            /*
            for (int i = 0; i < motionclass[NowClassNum].motionFeature[NowMotionNum].Joint_0.Count; i++)
            {
                MotionCanvas(motionclass[NowClassNum].motionFeature[NowMotionNum], BodyCanvas, i);
                CanvasToJPG(BodyCanvas, @"E:\Image\" + motionclass[NowClassNum].motionFeature[NowMotionNum].MotionName + i + ".jpg");
            }*/

            
            MotionCanvas(motionclass[NowClassNum].motionFeature[NowMotionNum], BodyCanvas,0);
            CanvasToJPG(BodyCanvas, @"E:\Image\" + motionclass[NowClassNum].motionFeature[NowMotionNum].MotionName+"0.jpg");
            MotionCanvas(motionclass[NowClassNum].motionFeature[NowMotionNum], BodyCanvas, (motionclass[NowClassNum].motionFeature[NowMotionNum].Joint_0.Count - 1) / 6);
            CanvasToJPG(BodyCanvas, @"E:\Image\" + motionclass[NowClassNum].motionFeature[NowMotionNum].MotionName + "1.jpg");
            MotionCanvas(motionclass[NowClassNum].motionFeature[NowMotionNum], BodyCanvas, 2* (motionclass[NowClassNum].motionFeature[NowMotionNum].Joint_0.Count - 1) / 6);
            CanvasToJPG(BodyCanvas, @"E:\Image\" + motionclass[NowClassNum].motionFeature[NowMotionNum].MotionName + "2.jpg");
            MotionCanvas(motionclass[NowClassNum].motionFeature[NowMotionNum], BodyCanvas, 3* (motionclass[NowClassNum].motionFeature[NowMotionNum].Joint_0.Count - 1) / 6);
            CanvasToJPG(BodyCanvas, @"E:\Image\" + motionclass[NowClassNum].motionFeature[NowMotionNum].MotionName + "3.jpg");
            MotionCanvas(motionclass[NowClassNum].motionFeature[NowMotionNum], BodyCanvas, 4* (motionclass[NowClassNum].motionFeature[NowMotionNum].Joint_0.Count - 1) / 6);
            CanvasToJPG(BodyCanvas, @"E:\Image\" + motionclass[NowClassNum].motionFeature[NowMotionNum].MotionName + "4.jpg");
            MotionCanvas(motionclass[NowClassNum].motionFeature[NowMotionNum], BodyCanvas, 5*(motionclass[NowClassNum].motionFeature[NowMotionNum].Joint_0.Count-1)/6);
            CanvasToJPG(BodyCanvas, @"E:\Image\" + motionclass[NowClassNum].motionFeature[NowMotionNum].MotionName + "5.jpg");
            MotionCanvas(motionclass[NowClassNum].motionFeature[NowMotionNum], BodyCanvas, (motionclass[NowClassNum].motionFeature[NowMotionNum].Joint_0.Count - 1));
            CanvasToJPG(BodyCanvas, @"E:\Image\" + motionclass[NowClassNum].motionFeature[NowMotionNum].MotionName + "6.jpg");

        }
        private void CanvasToJPG(Canvas canvas, string filename)
        {
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
            (int)canvas.Width, (int)canvas.Height, 96d, 96d, PixelFormats.Pbgra32);
            // needed otherwise the image output is black
            canvas.Measure(new Size((int)canvas.Width, (int)canvas.Height));
            canvas.Arrange(new Rect(new Size((int)canvas.Width, (int)canvas.Height)));

            renderBitmap.Render(canvas);

            //JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using (FileStream file = File.Create(filename))
            {
                encoder.Save(file);
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow MW = new MainWindow();
            this.Close();
            MW.Show();
        }
    }
}
