using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.Windows.Threading;
using System.IO;

namespace MotionRecognition
{
    /// <summary>
    /// IdentifyAction.xaml 的互動邏輯
    /// </summary>
    public partial class IdentifyAction : Window
    {
        private KinectSensor sensor;
        private WriteableBitmap colorBitmap;
        private Body[] bodies;
        private MultiSourceFrameReader msfr;
        private Boolean startRecord = false;
        private static string FilePath = System.IO.Directory.GetCurrentDirectory() + "\\";
        private DispatcherTimer CountdownTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(1000) };
        private FrameDescription frameDescription;
        private int timeLeft = 5; //倒數五秒
        private FrameDescription colorFrameDescription;
        private MotionFeature TestBody = new MotionFeature();
        private MotionClass[] motionclass;

        struct MotionFeature
        {
            public List<CameraSpacePoint> Limb_0 { get; set; }
            public List<CameraSpacePoint> Limb_1 { get; set; }
            public List<CameraSpacePoint> Limb_2 { get; set; }
            public List<CameraSpacePoint> Limb_3 { get; set; }
            public List<CameraSpacePoint> Limb_4 { get; set; }
            public List<CameraSpacePoint> Limb_5 { get; set; }
            public List<CameraSpacePoint> Limb_6 { get; set; }
            public List<CameraSpacePoint> Limb_7 { get; set; }
            public string MotionName { get; set; }
        }
        struct MotionClass
        {
            public MotionFeature[] motionFeature { get; set; }
            public string MotionName { get; set; }
            public double[] LimbEntropy { get; set; }
        }

        public IdentifyAction()
        {
            LoadMotion();
            
            TestBody.Limb_0 = new List<CameraSpacePoint>();
            TestBody.Limb_1 = new List<CameraSpacePoint>();
            TestBody.Limb_2 = new List<CameraSpacePoint>();
            TestBody.Limb_3 = new List<CameraSpacePoint>();
            TestBody.Limb_4 = new List<CameraSpacePoint>();
            TestBody.Limb_5 = new List<CameraSpacePoint>();
            TestBody.Limb_6 = new List<CameraSpacePoint>();
            TestBody.Limb_7 = new List<CameraSpacePoint>();
            TestBody.MotionName = "測試動作";

            InitializeComponent();
            RecodingIcon.Visibility = Visibility.Hidden;

            sensor = KinectSensor.GetDefault();
            frameDescription = this.sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            colorBitmap = new WriteableBitmap(frameDescription.Width, frameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
            image.Source = colorBitmap;
            bodies = new Body[6];
            sensor.Open();
            colorFrameDescription = sensor.ColorFrameSource.FrameDescription;

            msfr = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Body | FrameSourceTypes.Color);
            msfr.MultiSourceFrameArrived += msfr_MultiSourceFrameArrived;
            CountdownTimer.Tick += CountdownTimer_Tick;
            if (sensor.IsAvailable == true)
            {
                MessageBox.Show("請接上kinect sensor");
            }

            

        }

        void CountdownTimer_Tick(object sender, EventArgs e)
        {
            if (timeLeft > 0)
            {
                timeLeft = timeLeft - 1;
                TimeTB.Text = timeLeft + "s";
            }
            else
            {
                TimeTB.Text = "start！";
                TestBody.Limb_0.Clear();
                TestBody.Limb_1.Clear();
                TestBody.Limb_2.Clear();
                TestBody.Limb_3.Clear();
                TestBody.Limb_4.Clear();
                TestBody.Limb_5.Clear();
                TestBody.Limb_6.Clear();
                TestBody.Limb_7.Clear();
                RecodingIcon.Visibility = Visibility.Visible;

                CountdownTimer.Stop();
                startRecord = true;
            }
        }

        void msfr_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            MultiSourceFrame msf = e.FrameReference.AcquireFrame();
            if (msf != null)
            {
                using (BodyFrame bodyframe = msf.BodyFrameReference.AcquireFrame())
                {
                    using (ColorFrame colorframe = msf.ColorFrameReference.AcquireFrame())
                    {

                        if (bodyframe != null && colorframe != null)
                        {

                            // 畫即時的彩色影像在colorBitmap上
                            this.colorBitmap.Lock();

                            if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
                            {
                                colorframe.CopyConvertedFrameDataToIntPtr(
                                    this.colorBitmap.BackBuffer,
                                    (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                    ColorImageFormat.Bgra);

                                this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                            }
                            this.colorBitmap.Unlock();
                            colorframe.Dispose();

                            //獲得骨架資料
                            bodyframe.GetAndRefreshBodyData(bodies);
                            bodyCanvas.Children.Clear();
                            int bodyNum = 0;

                            foreach (Body body in bodies)
                            {
                                if (body.IsTracked)
                                {
                                    if (bodyNum == 0)
                                    {
                                        if (startRecord )
                                        {
                                            CameraSpacePoint HipLR_Vector = new CameraSpacePoint { X = body.Joints[JointType.HipRight].Position.X - body.Joints[JointType.HipLeft].Position.X, Y = body.Joints[JointType.HipRight].Position.Y - body.Joints[JointType.HipLeft].Position.Y, Z = body.Joints[JointType.HipRight].Position.Z - body.Joints[JointType.HipLeft].Position.Z };
                                            double PlaneAngle = Math.Atan2(HipLR_Vector.Z, HipLR_Vector.X);
                                            CameraSpacePoint newpoint1 = getNewXZPlanePoint(body.Joints[JointType.ElbowRight].Position, PlaneAngle);
                                            CameraSpacePoint newpoint2 = getNewXZPlanePoint(body.Joints[JointType.ShoulderRight].Position, PlaneAngle);
                                            TestBody.Limb_0.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                            newpoint1 = getNewXZPlanePoint(body.Joints[JointType.ElbowLeft].Position, PlaneAngle);
                                            newpoint2 = getNewXZPlanePoint(body.Joints[JointType.ShoulderLeft].Position, PlaneAngle);
                                            TestBody.Limb_1.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                            newpoint1 = getNewXZPlanePoint(body.Joints[JointType.KneeRight].Position, PlaneAngle);
                                            newpoint2 = getNewXZPlanePoint(body.Joints[JointType.HipRight].Position, PlaneAngle);
                                            TestBody.Limb_2.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                            newpoint1 = getNewXZPlanePoint(body.Joints[JointType.KneeLeft].Position, PlaneAngle);
                                            newpoint2 = getNewXZPlanePoint(body.Joints[JointType.HipLeft].Position, PlaneAngle);
                                            TestBody.Limb_3.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                            newpoint1 = getNewXZPlanePoint(body.Joints[JointType.WristRight].Position, PlaneAngle);
                                            newpoint2 = getNewXZPlanePoint(body.Joints[JointType.ElbowRight].Position, PlaneAngle);
                                            TestBody.Limb_4.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                            newpoint1 = getNewXZPlanePoint(body.Joints[JointType.WristLeft].Position, PlaneAngle);
                                            newpoint2 = getNewXZPlanePoint(body.Joints[JointType.ElbowLeft].Position, PlaneAngle);
                                            TestBody.Limb_5.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                            newpoint1 = getNewXZPlanePoint(body.Joints[JointType.AnkleRight].Position, PlaneAngle);
                                            newpoint2 = getNewXZPlanePoint(body.Joints[JointType.KneeRight].Position, PlaneAngle);
                                            TestBody.Limb_6.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                            newpoint1 = getNewXZPlanePoint(body.Joints[JointType.AnkleLeft].Position, PlaneAngle);
                                            newpoint2 = getNewXZPlanePoint(body.Joints[JointType.KneeLeft].Position, PlaneAngle);
                                            TestBody.Limb_7.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });
                                            
                                        }
                                        //畫Joint
                                        for (int i = 0; i < 25; i++)
                                        {
                                            CanvasAddJoint(body, (JointType)i);
                                        }
                                    }
                                    bodyNum++;
                                }
                            }
                        }
                    }
                }
            }
        }
        private void CanvasAddJoint(Body body, JointType JT)
        {
            Joint joint = body.Joints[JT];
            if (joint.TrackingState == TrackingState.Tracked)
            {
                ColorSpacePoint csp = sensor.CoordinateMapper.MapCameraPointToColorSpace(joint.Position);
                double scaleX = (double)1920.0 / bodyCanvas.Width;
                double scaleY = (double)1080.0 / bodyCanvas.Height;

                System.Windows.Shapes.Ellipse jointCircle = new System.Windows.Shapes.Ellipse { Width = 18, Height = 18, Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0)) };
                bodyCanvas.Children.Add(jointCircle);
                Canvas.SetLeft(jointCircle, csp.X / scaleX - 9);
                Canvas.SetTop(jointCircle, csp.Y / scaleY - 9);
            }
        }
        private string MotionMatching(MotionFeature RealTimeMotion, MotionClass[] DataMotion)
        {
            List<double> Distance = new List<double>();
            List<string> motionname = new List<string>();
            string BestMatching = "";
            for (int ClassNum = 0; ClassNum < DataMotion.Length; ClassNum++)
            {
                for (int MotionNum = 0; MotionNum < DataMotion[ClassNum].motionFeature.Length; MotionNum++)
                {
                    double cost = 0;
                    double[,] DTWTable = new double[RealTimeMotion.Limb_0.Count, DataMotion[ClassNum].motionFeature[MotionNum].Limb_0.Count];
                    //設周圍兩排為正無窮大
                    for (int i = 1; i < RealTimeMotion.Limb_0.Count; i++)
                    {
                        DTWTable[i, 0] = Double.PositiveInfinity;
                    }
                    for (int i = 1; i < motionclass[ClassNum].motionFeature[MotionNum].Limb_0.Count; i++)
                    {
                        DTWTable[0, i] = Double.PositiveInfinity;
                    }
                    //設起點成本為0
                    DTWTable[0, 0] = 0;
                    //跑動作的每個Frame的分數
                    for (int i = 1; i < RealTimeMotion.Limb_0.Count; i++)
                    {
                        for (int j = 1; j < motionclass[ClassNum].motionFeature[MotionNum].Limb_0.Count; j++)
                        {
                            //cost = 100 - MotionScore(RealTimeMotion, motionclass[ClassNum].motionFeature[MotionNum], i, j);
                            cost = MotionDistance(RealTimeMotion, motionclass[ClassNum].motionFeature[MotionNum], i, j, ClassNum);
                            double min = Math.Min(DTWTable[i - 1, j], DTWTable[i, j - 1]);
                            min = Math.Min(min, DTWTable[i - 1, j - 1]);
                            DTWTable[i, j] = cost + min;
                        }
                    }
                    int step = DTWSteps(DTWTable, DTWTable.GetLength(0) - 1, DTWTable.GetLength(1) - 1, 0);
                    double dis = DTWTable[RealTimeMotion.Limb_0.Count - 1, motionclass[ClassNum].motionFeature[MotionNum].Limb_0.Count - 1] / step;

                    Distance.Add(dis);
                    motionname.Add(motionclass[ClassNum].MotionName);
                }
            }
            //最小距離的動作
            double MinDistance = Distance.Min();
            //假如距離小於RealtimeBodyLength*45差距，每Frame最多差距45，就給出動作類別
            BestMatching = motionname[Distance.IndexOf(MinDistance)];
            
            return BestMatching;
        }

        private int DTWSteps(double[,] dtwTable, int i, int j, int steps)
        {
            steps++;
            if (i > 0 && j > 0)
            {
                double light = dtwTable[i - 1, j];
                double down = dtwTable[i, j - 1];
                double min = dtwTable[i - 1, j - 1];
                if (light <= down)
                {
                    if (min <= light)
                    {
                        steps = DTWSteps(dtwTable, i - 1, j - 1, steps);
                    }
                    else
                    {
                        steps = DTWSteps(dtwTable, i - 1, j, steps);
                    }
                }
                else
                {
                    if (min <= down)
                    {
                        steps = DTWSteps(dtwTable, i - 1, j - 1, steps);
                    }
                    else
                    {
                        steps = DTWSteps(dtwTable, i, j - 1, steps);
                    }
                }
            }
            return steps;
        }
        private double MotionDistance(MotionFeature A_motion, MotionFeature B_motion, int A_count, int B_count, int classnum)
        {
            double score = 0;

            double[] angle = new double[] {
                cos_theta(A_motion.Limb_0[A_count],B_motion.Limb_0[B_count]),
                cos_theta(A_motion.Limb_1[A_count],B_motion.Limb_1[B_count]),
                cos_theta(A_motion.Limb_2[A_count],B_motion.Limb_2[B_count]),
                cos_theta(A_motion.Limb_3[A_count],B_motion.Limb_3[B_count]),
                cos_theta(A_motion.Limb_4[A_count],B_motion.Limb_4[B_count]),
                cos_theta(A_motion.Limb_5[A_count],B_motion.Limb_5[B_count]),
                cos_theta(A_motion.Limb_6[A_count],B_motion.Limb_6[B_count]),
                cos_theta(A_motion.Limb_7[A_count],B_motion.Limb_7[B_count]),
            };
            for (int i = 0; i < angle.Length; i++)
            {
                score += angle[i];
            }
            return score;
        }
        


        private double cos_theta(CameraSpacePoint A_Limb, CameraSpacePoint B_Limb)
        {
            double cos;
            double angle;

            double tmp;
            tmp = Math.Sqrt(A_Limb.X * A_Limb.X + A_Limb.Y * A_Limb.Y + A_Limb.Z * A_Limb.Z) * Math.Sqrt(B_Limb.X * B_Limb.X + B_Limb.Y * B_Limb.Y + B_Limb.Z * B_Limb.Z);
            if (Math.Abs(tmp) < 0.0001)
            {
                cos = 0;
            }
            else
            {
                cos = (A_Limb.X * B_Limb.X + A_Limb.Y * B_Limb.Y + A_Limb.Z * B_Limb.Z) / tmp;
            }
            if (cos > 1)
            {
                cos = 1;
            }
            angle = Math.Acos(cos) * 180 / Math.PI;

            return angle;
        }

        private void RecordingBT_Click(object sender, RoutedEventArgs e)
        {
            if (startRecord)
            {//停止錄影
                startRecord = false;
                RecordingBT.Content = "錄影";
                CountdownTimer.Stop();
                ResultTB.Text = MotionMatching(TestBody,motionclass);
                RecodingIcon.Visibility = Visibility.Hidden;
            }
            else
            {//開始錄影
                timeLeft = 5;
                CountdownTimer.Start();
                RecordingBT.Content = "結束錄影";
            }
        }
        
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.msfr != null)
            {
                this.msfr.Dispose();
                this.msfr = null;
            }
            if (this.sensor != null)
            {
                this.sensor.Close();
                this.sensor = null;
            }
            MainWindow mainWindow = new MainWindow();
            this.Close();
            mainWindow.Show();
        }

        private CameraSpacePoint getNewXZPlanePoint(CameraSpacePoint point, double angle)//旋轉X-Z平面上坐標系得到點的新座標
        {
            CameraSpacePoint newpoint = new CameraSpacePoint { X = (float)(Math.Cos(angle) * point.X + Math.Sin(angle) * point.Z), Y = point.Y, Z = (float)(-Math.Sin(angle) * point.X + Math.Cos(angle) * point.Z) };
            return newpoint;
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
                int motionAmount = Dir.GetFiles("*.Limb3D").Length;
                MotionFeature[] MFs = new MotionFeature[motionAmount];

                for (int m = 1; m <= motionAmount; m++)//類別內的動作數量
                {
                    string text = LoadFile(FilePath + "動作類別\\" + fileNames[i].ToString() + "\\" + m.ToString() + ".Limb3D");
                    string[] A_Frame = text.Split(new Char[] { '@' });
                    MotionFeature MF = new MotionFeature
                    {
                        Limb_0 = new List<CameraSpacePoint>(),
                        Limb_1 = new List<CameraSpacePoint>(),
                        Limb_2 = new List<CameraSpacePoint>(),
                        Limb_3 = new List<CameraSpacePoint>(),
                        Limb_4 = new List<CameraSpacePoint>(),
                        Limb_5 = new List<CameraSpacePoint>(),
                        Limb_6 = new List<CameraSpacePoint>(),
                        Limb_7 = new List<CameraSpacePoint>(),
                        MotionName = fileNames[i].ToString()
                    };
                    for (int j = 0; j < A_Frame.Length; j++)//frame的數量
                    {
                        string[] Limb = A_Frame[j].Split(new Char[] { '#' });
                        for (int k = 0; k < Limb.Length; k++)//肢體的數量
                        {
                            string[] xyz = Limb[k].Split(new Char[] { ',' });
                            switch (k)
                            {
                                case 0:
                                    MF.Limb_0.Add(new CameraSpacePoint { X = (float)Convert.ToDouble(xyz[0]), Y = (float)Convert.ToDouble(xyz[1]), Z = (float)Convert.ToDouble(xyz[2]) });
                                    break;
                                case 1:
                                    MF.Limb_1.Add(new CameraSpacePoint { X = (float)Convert.ToDouble(xyz[0]), Y = (float)Convert.ToDouble(xyz[1]), Z = (float)Convert.ToDouble(xyz[2]) });
                                    break;
                                case 2:
                                    MF.Limb_2.Add(new CameraSpacePoint { X = (float)Convert.ToDouble(xyz[0]), Y = (float)Convert.ToDouble(xyz[1]), Z = (float)Convert.ToDouble(xyz[2]) });
                                    break;
                                case 3:
                                    MF.Limb_3.Add(new CameraSpacePoint { X = (float)Convert.ToDouble(xyz[0]), Y = (float)Convert.ToDouble(xyz[1]), Z = (float)Convert.ToDouble(xyz[2]) });
                                    break;
                                case 4:
                                    MF.Limb_4.Add(new CameraSpacePoint { X = (float)Convert.ToDouble(xyz[0]), Y = (float)Convert.ToDouble(xyz[1]), Z = (float)Convert.ToDouble(xyz[2]) });
                                    break;
                                case 5:
                                    MF.Limb_5.Add(new CameraSpacePoint { X = (float)Convert.ToDouble(xyz[0]), Y = (float)Convert.ToDouble(xyz[1]), Z = (float)Convert.ToDouble(xyz[2]) });
                                    break;
                                case 6:
                                    MF.Limb_6.Add(new CameraSpacePoint { X = (float)Convert.ToDouble(xyz[0]), Y = (float)Convert.ToDouble(xyz[1]), Z = (float)Convert.ToDouble(xyz[2]) });
                                    break;
                                case 7:
                                    MF.Limb_7.Add(new CameraSpacePoint { X = (float)Convert.ToDouble(xyz[0]), Y = (float)Convert.ToDouble(xyz[1]), Z = (float)Convert.ToDouble(xyz[2]) });
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


            for (int i = 0; i < motionclass.Length; i++)//類別總數
            {
                double[] thetaSum = new double[8];
                for (int j = 0; j < motionclass[i].motionFeature.Length; j++)//類別內的動作樣本數量
                {
                    for (int k = 1; k < motionclass[i].motionFeature[j].Limb_0.Count; k++)//Frame的數量
                    {
                        thetaSum[0] += cos_theta(motionclass[i].motionFeature[j].Limb_0[k], motionclass[i].motionFeature[j].Limb_0[k - 1]);
                        thetaSum[1] += cos_theta(motionclass[i].motionFeature[j].Limb_1[k], motionclass[i].motionFeature[j].Limb_1[k - 1]);
                        thetaSum[2] += cos_theta(motionclass[i].motionFeature[j].Limb_2[k], motionclass[i].motionFeature[j].Limb_2[k - 1]);
                        thetaSum[3] += cos_theta(motionclass[i].motionFeature[j].Limb_3[k], motionclass[i].motionFeature[j].Limb_3[k - 1]);
                        thetaSum[4] += cos_theta(motionclass[i].motionFeature[j].Limb_4[k], motionclass[i].motionFeature[j].Limb_4[k - 1]);
                        thetaSum[5] += cos_theta(motionclass[i].motionFeature[j].Limb_5[k], motionclass[i].motionFeature[j].Limb_5[k - 1]);
                        thetaSum[6] += cos_theta(motionclass[i].motionFeature[j].Limb_6[k], motionclass[i].motionFeature[j].Limb_6[k - 1]);
                        thetaSum[7] += cos_theta(motionclass[i].motionFeature[j].Limb_7[k], motionclass[i].motionFeature[j].Limb_7[k - 1]);
                    }
                }
                double Totaltheta = 0;
                for (int j = 0; j < thetaSum.Length; j++)
                {
                    Totaltheta += thetaSum[j];
                }
                for (int j = 0; j < thetaSum.Length; j++)
                {
                    thetaSum[j] = thetaSum[j] / Totaltheta;
                }
                motionclass[i].LimbEntropy = thetaSum;
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
    }
}
