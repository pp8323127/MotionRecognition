using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Microsoft.Kinect;
using System.Windows.Threading;
using System.Diagnostics;
using System.IO;


namespace MotionRecognition
{
    /// <summary>
    /// RecordingWindows.xaml 的互動邏輯
    /// </summary>
    public partial class RecordingWindows : Window
    {
        public string FilePath = "";
        DispatcherTimer CountdownTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(1000) };
        Boolean StartRecording = false;
        int timeLeft = 3; //倒數3秒
        Stopwatch RecordingTime = new Stopwatch(); //影片計時
        List<CameraSpacePoint>[] JointsPosition=new List<CameraSpacePoint>[25];

        public RecordingWindows()
        {
            this.kinectSensor = KinectSensor.GetDefault();

            this.coordinateMapper = this.kinectSensor.CoordinateMapper;

            FrameDescription frameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;

            this.displayWidth = frameDescription.Width;
            this.displayHeight = frameDescription.Height;

            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();
            this.bones = new List<Tuple<JointType, JointType>>();
            // Torso
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));

            // Right Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight));

            // Left Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft));

            // Right Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipRight, JointType.KneeRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.FootRight));

            // Left Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));

            this.kinectSensor.Open();
            this.drawingGroup = new DrawingGroup();
            this.imageSource = new DrawingImage(this.drawingGroup);
            this.DataContext = this;

            CountdownTimer.Tick += CountdownTimer_Tick;

            this.InitializeComponent();

            RecodingIcon.Visibility = Visibility.Hidden;
        }

        void CountdownTimer_Tick(object sender, EventArgs e)
        {
            if (timeLeft > 0)
            {
                timeLeft = timeLeft - 1;
                LB.Content = timeLeft + " seconds";
            }
            else
            {
                RecodingIcon.Visibility = Visibility.Visible;
                RecordingTime.Reset();
                CountdownTimer.Stop();
                StartRecording = true;
                RecordingTime.Start();
            }
        }

        private const double HandSize = 30;
        private const double JointThickness = 3;
        private const double ClipBoundsThickness = 10;
        private const float InferredZPositionClamp = 0.1f;
        private readonly Brush handClosedBrush = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));
        private readonly Brush handOpenBrush = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0));
        private readonly Brush handLassoBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255));
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private readonly Brush inferredJointBrush = Brushes.Yellow;
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);
        private DrawingGroup drawingGroup;
        private DrawingImage imageSource;
        private KinectSensor kinectSensor = null;
        private CoordinateMapper coordinateMapper = null;
        private BodyFrameReader bodyFrameReader = null;
        private Body[] bodies = null;
        private List<Tuple<JointType, JointType>> bones;
        private int displayWidth;
        private int displayHeight;
        Pen drawPen = new Pen(Brushes.Red, 6);

        public ImageSource ImageSource
        {
            get
            {
                return this.imageSource;
            }
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
            }
        }
        private void MainWindow_Closing(object sender,  System.ComponentModel.CancelEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }

        private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                using (DrawingContext dc = this.drawingGroup.Open())
                {
                    dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                    /*
                    LB.Content =
                        this.bodies[0].IsTracked.ToString() + "\n" +
                        this.bodies[1].IsTracked.ToString() + "\n" +
                        this.bodies[2].IsTracked.ToString() + "\n" +
                        this.bodies[3].IsTracked.ToString() + "\n" +
                        this.bodies[4].IsTracked.ToString() + "\n" +
                        this.bodies[5].IsTracked.ToString() + "\n";
                    */
                    int bodyIndex = 0;
                    foreach (Body body in this.bodies)
                    {
                        if (body.IsTracked)
                        {
                            if (bodyIndex == 0)
                            {
                                this.DrawClippedEdges(body, dc);

                                IReadOnlyDictionary<JointType, Joint> joints = body.Joints;
                                Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                                foreach (JointType jointType in joints.Keys)
                                {
                                    // sometimes the depth(Z) of an inferred joint may show as negative
                                    // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                                    CameraSpacePoint position = joints[jointType].Position;
                                    if (position.Z < 0)
                                    {
                                        position.Z = InferredZPositionClamp;
                                    }

                                    DepthSpacePoint depthSpacePoint = this.coordinateMapper.MapCameraPointToDepthSpace(position);
                                    jointPoints[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);
                                }

                                this.DrawBody(joints, jointPoints, dc, drawPen);

                                this.DrawHand(body.HandLeftState, jointPoints[JointType.HandLeft], dc);
                                this.DrawHand(body.HandRightState, jointPoints[JointType.HandRight], dc);

                                if (StartRecording)
                                {
                                    LB.Content = Math.Round(((double)RecordingTime.Elapsed.TotalSeconds),1).ToString();
                                    for (int i = 0; i < 25; i++)//紀錄25個關節
                                    {
                                        JointsPosition[i].Add(body.Joints[(JointType)i].Position);
                                    }
                                }
                            }
                            bodyIndex++;
                        }
                    }
                    // prevent drawing outside of our render area
                    this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                }
            }
        }

        private void DrawBody(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext, Pen drawingPen)
        {
            // Draw the bones
            foreach (var bone in this.bones)
            {
                this.DrawBone(joints, jointPoints, bone.Item1, bone.Item2, drawingContext, drawingPen);
            }

            // Draw the joints
            foreach (JointType jointType in joints.Keys)
            {
                Brush drawBrush = null;

                TrackingState trackingState = joints[jointType].TrackingState;

                if (trackingState == TrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (trackingState == TrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, jointPoints[jointType], JointThickness, JointThickness);
                }
            }
        }

        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, JointType jointType0, JointType jointType1, DrawingContext drawingContext, Pen drawingPen)
        {
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == TrackingState.NotTracked ||
                joint1.TrackingState == TrackingState.NotTracked)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if ((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked))
            {
                drawPen = drawingPen;
            }

            drawingContext.DrawLine(drawPen, jointPoints[jointType0], jointPoints[jointType1]);
        }

        private void DrawHand(HandState handState, Point handPosition, DrawingContext drawingContext)
        {
            switch (handState)
            {
                case HandState.Closed:
                    drawingContext.DrawEllipse(this.handClosedBrush, null, handPosition, HandSize, HandSize);
                    break;

                case HandState.Open:
                    drawingContext.DrawEllipse(this.handOpenBrush, null, handPosition, HandSize, HandSize);
                    break;

                case HandState.Lasso:
                    drawingContext.DrawEllipse(this.handLassoBrush, null, handPosition, HandSize, HandSize);
                    break;
            }
        }

        private void DrawClippedEdges(Body body, DrawingContext drawingContext)
        {
            FrameEdges clippedEdges = body.ClippedEdges;

            if (clippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, this.displayHeight - ClipBoundsThickness, this.displayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, this.displayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, this.displayHeight));
            }

            if (clippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(this.displayWidth - ClipBoundsThickness, 0, ClipBoundsThickness, this.displayHeight));
            }
        }

        private void RecodingButton_Click(object sender, RoutedEventArgs e)
        {
            if (StartRecording)//停止錄
            {
                StartRecording=false;
                RecodingButton.Content = "開始錄製";
                CountdownTimer.Stop();
                RecordingTime.Stop();
                //以下儲存3D關節資訊
                string Joint3DText = "";
                for (int i = 0; i < 25; i++)
                {
                    if (i < 24)
                    {
                        for (int j = 0; j < JointsPosition[i].Count; j++)
                        {
                            if (j < JointsPosition[i].Count - 1)
                            {
                                CameraSpacePoint JP = JointsPosition[i][j];

                                Joint3DText += JP.X + "," + JP.Y + "," + JP.Z + "@";
                                continue;
                            }
                            CameraSpacePoint JP2 = JointsPosition[i][j];
                            Joint3DText += JP2.X + "," + JP2.Y + "," + JP2.Z + "#";
                        }
                        continue;
                    }
                    for (int j = 0; j < JointsPosition[i].Count; j++)
                    {
                        if (j < JointsPosition[i].Count - 1)
                        {
                            CameraSpacePoint JP = JointsPosition[i][j];

                            Joint3DText += JP.X + "," + JP.Y + "," + JP.Z + "@";
                            continue;
                        }
                        CameraSpacePoint JP2 = JointsPosition[i][j];
                        Joint3DText += JP2.X + "," + JP2.Y + "," + JP2.Z ;
                    }
                }
                //以下儲存2D顯示資訊
                string text = "";
                for (int i = 0; i < 25; i++)
                {
                    if (i < 24)
                    {
                        for (int j = 0; j < JointsPosition[i].Count - 1; j++)
                        {
                            CameraSpacePoint JP = JointsPosition[i][j];
                            if (JP.Z < 0)
                            {
                                JP.Z = 0.1f;
                            }
                            DepthSpacePoint dsp = this.coordinateMapper.MapCameraPointToDepthSpace(JP);
                            text += dsp.X + "," + dsp.Y + "@";
                        }
                        CameraSpacePoint JP2 = JointsPosition[i][JointsPosition[i].Count - 1];
                        if (JP2.Z < 0)
                        {
                            JP2.Z = 0.1f;
                        }
                        DepthSpacePoint dsp2 = this.coordinateMapper.MapCameraPointToDepthSpace(JP2);
                        text += dsp2.X + "," + dsp2.Y + "#";
                        continue;
                    }
                    for (int j = 0; j < JointsPosition[i].Count - 1; j++)
                    {
                        CameraSpacePoint JP = JointsPosition[i][j];
                        if (JP.Z < 0)
                        {
                            JP.Z = 0.1f;
                        }
                        DepthSpacePoint dsp = this.coordinateMapper.MapCameraPointToDepthSpace(JP);
                        text += dsp.X + "," + dsp.Y + "@";
                    }
                    CameraSpacePoint JP3 = JointsPosition[i][JointsPosition[i].Count - 1];
                    if (JP3.Z < 0)
                    {
                        JP3.Z = 0.1f;
                    }
                    DepthSpacePoint dsp3 = this.coordinateMapper.MapCameraPointToDepthSpace(JP3);
                    text += dsp3.X + "," + dsp3.Y ;
                }
                //以下儲存X-Z座標軸旋轉後8個肢體向量資訊
                string LimbText = "";
                for (int i = 0; i < JointsPosition[0].Count-1; i++)
                {
                    Vector4 HipLR_Vector = new Vector4 { X = (float)JointsPosition[16][i].X - (float)JointsPosition[12][i].X, Y = (float)JointsPosition[16][i].Y - (float)JointsPosition[12][i].Y, Z = (float)JointsPosition[16][i].Z - (float)JointsPosition[12][i].Z };
                    double angle = Math.Atan2(HipLR_Vector.Z, HipLR_Vector.X);
                    CameraSpacePoint[] Limb = new CameraSpacePoint[8];
                    Limb[0] = NewLimb(JointsPosition[9][i], JointsPosition[8][i],angle);
                    Limb[1] = NewLimb(JointsPosition[5][i], JointsPosition[4][i], angle);
                    Limb[2] = NewLimb(JointsPosition[17][i], JointsPosition[16][i], angle);
                    Limb[3] = NewLimb(JointsPosition[13][i], JointsPosition[12][i], angle);
                    Limb[4] = NewLimb(JointsPosition[10][i], JointsPosition[9][i], angle);
                    Limb[5] = NewLimb(JointsPosition[6][i], JointsPosition[5][i], angle);
                    Limb[6] = NewLimb(JointsPosition[18][i], JointsPosition[17][i], angle);
                    Limb[7] = NewLimb(JointsPosition[14][i], JointsPosition[13][i], angle);
                    LimbText +=
                        Limb[0].X + "," + Limb[0].Y + "," + Limb[0].Z + "#" +
                        Limb[1].X + "," + Limb[1].Y + "," + Limb[1].Z + "#" +
                        Limb[2].X + "," + Limb[2].Y + "," + Limb[2].Z + "#" +
                        Limb[3].X + "," + Limb[3].Y + "," + Limb[3].Z + "#" +
                        Limb[4].X + "," + Limb[4].Y + "," + Limb[4].Z + "#" +
                        Limb[5].X + "," + Limb[5].Y + "," + Limb[5].Z + "#" +
                        Limb[6].X + "," + Limb[6].Y + "," + Limb[6].Z + "#" +
                        Limb[7].X + "," + Limb[7].Y + "," + Limb[7].Z + "@";
                }
                int LastCount = JointsPosition[0].Count - 1;
                Vector4 HipLR_Vector2 = new Vector4 { X = (float)JointsPosition[16][LastCount].X - (float)JointsPosition[12][LastCount].X, Y = (float)JointsPosition[16][LastCount].Y - (float)JointsPosition[12][LastCount].Y, Z = (float)JointsPosition[16][LastCount].Z - (float)JointsPosition[12][LastCount].Z };
                double angle2 = Math.Atan2(HipLR_Vector2.Z, HipLR_Vector2.X);
                CameraSpacePoint[] Limb2 = new CameraSpacePoint[8];
                Limb2[0] = NewLimb(JointsPosition[9][LastCount], JointsPosition[8][LastCount], angle2);
                Limb2[1] = NewLimb(JointsPosition[5][LastCount], JointsPosition[4][LastCount], angle2);
                Limb2[2] = NewLimb(JointsPosition[17][LastCount], JointsPosition[16][LastCount], angle2);
                Limb2[3] = NewLimb(JointsPosition[13][LastCount], JointsPosition[12][LastCount], angle2);
                Limb2[4] = NewLimb(JointsPosition[10][LastCount], JointsPosition[9][LastCount], angle2);
                Limb2[5] = NewLimb(JointsPosition[6][LastCount], JointsPosition[5][LastCount], angle2);
                Limb2[6] = NewLimb(JointsPosition[18][LastCount], JointsPosition[17][LastCount], angle2);
                Limb2[7] = NewLimb(JointsPosition[14][LastCount], JointsPosition[13][LastCount], angle2);
                LimbText +=
                    Limb2[0].X + "," + Limb2[0].Y + "," + Limb2[0].Z + "#" +
                    Limb2[1].X + "," + Limb2[1].Y + "," + Limb2[1].Z + "#" +
                    Limb2[2].X + "," + Limb2[2].Y + "," + Limb2[2].Z + "#" +
                    Limb2[3].X + "," + Limb2[3].Y + "," + Limb2[3].Z + "#" +
                    Limb2[4].X + "," + Limb2[4].Y + "," + Limb2[4].Z + "#" +
                    Limb2[5].X + "," + Limb2[5].Y + "," + Limb2[5].Z + "#" +
                    Limb2[6].X + "," + Limb2[6].Y + "," + Limb2[6].Z + "#" +
                    Limb2[7].X + "," + Limb2[7].Y + "," + Limb2[7].Z ;


                //查詢路徑下範本數量
                DirectoryInfo Dir = new DirectoryInfo(FilePath);
                int motionAmount = Dir.GetFiles("*.position2D").Length;
                //創建新範本
                CreateFile((motionAmount+1).ToString()+".position2D",text);
                CreateFile((motionAmount + 1).ToString() + ".Limb3D", LimbText);
                CreateFile((motionAmount + 1).ToString() + ".Joint3D", Joint3DText);

                RecodingIcon.Visibility = Visibility.Hidden;

            }
            else//開始錄
            {
                timeLeft = 3;
                RecodingButton.Content = "停止錄製";
                for (int i = 0; i < 25; i++)
                {
                    JointsPosition[i] = new List<CameraSpacePoint>();
                }
                CountdownTimer.Start();

            }
        }

        private CameraSpacePoint NewLimb(CameraSpacePoint jointNumBefore, CameraSpacePoint jointNumAfter, double angle)
        {
            CameraSpacePoint before = new CameraSpacePoint { X = jointNumBefore.X, Y = jointNumBefore.Y, Z = jointNumBefore.Z };
            CameraSpacePoint after = new CameraSpacePoint { X = jointNumAfter.X, Y = jointNumAfter.Y, Z = jointNumAfter.Z };
            before = getNewXZPlanePoint(before, angle);
            after = getNewXZPlanePoint(after, angle);
            CameraSpacePoint limb = new CameraSpacePoint { X = before.X - after.X, Y = before.Y - after.Y, Z = before.Z - after.Z };
            return limb;
        }
        private void CreateFile(string FileName, string contain)
        {
            FileInfo fh = new FileInfo(FilePath+"\\"+FileName);
            StreamWriter sw = fh.CreateText();
            sw.WriteLine(contain);
            sw.Flush();
            sw.Close();
        }

        private CameraSpacePoint getNewXZPlanePoint(CameraSpacePoint point, double angle)//旋轉X-Z平面上坐標系得到點的新座標
        {
            CameraSpacePoint newpoint = new CameraSpacePoint { X = (float)(Math.Cos(angle) * point.X + Math.Sin(angle) * point.Z), Y = point.Y, Z = (float)(-Math.Sin(angle) * point.X + Math.Cos(angle) * point.Z) };
            return newpoint;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }
            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
            MainWindow MW = new MainWindow();
            this.Close();
            MW.Show();
        }

    }
}
