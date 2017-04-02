using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.IO;

namespace MotionRecognition
{
    /// <summary>
    /// IdentifyAction.xaml 的互動邏輯
    /// </summary>
    public partial class RealTimeIdentifyAction : Window
    {
        private string filePath = System.IO.Directory.GetCurrentDirectory() + "\\";
        private KinectSensor kinectSensor = null;
        private MultiSourceFrameReader multiFrameReader = null;
        private WriteableBitmap colorBitmap = null;
        private MotionClass[] motionclass;
        private MotionFeature RealTimeBody = new MotionFeature();
        private Body[] bodies = null;
        int RealtimeBodyLength = 60;

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
            public double T3_Threshold { get; set; }
            public MotionFeature[] motionFeature { get; set; }
            public string MotionName { get; set; }
            public double[] LimbEntropy { get; set; }
        }

        string nowMotion = "";
        string preMotion = "";  
        public RealTimeIdentifyAction()
        {
            this.kinectSensor = KinectSensor.GetDefault();
            this.multiFrameReader = this.kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Body | FrameSourceTypes.Color);
            multiFrameReader.MultiSourceFrameArrived += multiFrameReader_MultiSourceFrameArrived;
            FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
            this.kinectSensor.Open();
            this.DataContext = this;

            RealTimeBody.Limb_0 = new List<CameraSpacePoint>();
            RealTimeBody.Limb_1 = new List<CameraSpacePoint>();
            RealTimeBody.Limb_2 = new List<CameraSpacePoint>();
            RealTimeBody.Limb_3 = new List<CameraSpacePoint>();
            RealTimeBody.Limb_4 = new List<CameraSpacePoint>();
            RealTimeBody.Limb_5 = new List<CameraSpacePoint>();
            RealTimeBody.Limb_6 = new List<CameraSpacePoint>();
            RealTimeBody.Limb_7 = new List<CameraSpacePoint>();
            RealTimeBody.MotionName = "即時動作";
            
            LoadMotion();
            
            //DTW();

            this.InitializeComponent();
        }
        CameraSpacePoint[] prebody = new CameraSpacePoint[21];
        int framecount = 0;
        int frameCount = 0;
        ulong? trackingID = null;
        bool StartMotion = false;
        void multiFrameReader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
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
                            //以下為ColorFrame部分
                            FrameDescription colorFrameDescription = colorframe.FrameDescription;

                            using (KinectBuffer colorBuffer = colorframe.LockRawImageBuffer())
                            {
                                this.colorBitmap.Lock();

                                // verify data and write the new color frame data to the display bitmap
                                if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
                                {
                                    colorframe.CopyConvertedFrameDataToIntPtr(
                                        this.colorBitmap.BackBuffer,
                                        (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                        ColorImageFormat.Bgra);

                                    this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                                }

                                this.colorBitmap.Unlock();
                            }
                            //以下為BodyFrame部分
                            bool dataReceived = false;

                            if (this.bodies == null)
                            {
                                this.bodies = new Body[bodyframe.BodyCount];
                            }

                            // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                            // As long as those body objects are not disposed and not set to null in the array,
                            // those body objects will be re-used.
                            bodyframe.GetAndRefreshBodyData(this.bodies);
                            dataReceived = true;
                            if (dataReceived)
                            {
                                
                                foreach (Body body in this.bodies)
                                {
                                    if (body.IsTracked)
                                    {
                                        if (trackingID == null)
                                        {
                                            trackingID = body.TrackingId;
                                        }
                                        else
                                        {
                                            if (trackingID == body.TrackingId)
                                            {
                                                Bodycanvas.Children.Clear();
                                                CanvasAddJoint(body, JointType.Head);

                                                double dis = 0;
                                                CameraSpacePoint[] nowbody = new CameraSpacePoint[21];
                                                for (int i = 0; i < nowbody.Length; i++)
                                                {
                                                    nowbody[i].X = body.Joints[(JointType)i].Position.X - body.Joints[0].Position.X;
                                                    nowbody[i].Y = body.Joints[(JointType)i].Position.Y - body.Joints[0].Position.Y;
                                                    nowbody[i].Z = body.Joints[(JointType)i].Position.Z - body.Joints[0].Position.Z;
                                                }
                                                for (int i = 0; i < nowbody.Length; i++)
                                                {
                                                    dis += o4dis(prebody[i], nowbody[i]);
                                                }
                                                if (dis > 0.1)
                                                {
                                                    CameraSpacePoint HipLR_Vector = new CameraSpacePoint { X = body.Joints[JointType.HipRight].Position.X - body.Joints[JointType.HipLeft].Position.X, Y = body.Joints[JointType.HipRight].Position.Y - body.Joints[JointType.HipLeft].Position.Y, Z = body.Joints[JointType.HipRight].Position.Z - body.Joints[JointType.HipLeft].Position.Z };
                                                    double PlaneAngle = Math.Atan2(HipLR_Vector.Z, HipLR_Vector.X);

                                                    if (StartMotion == false)
                                                    {
                                                        StartMotion = true;
                                                        LB3.Content = "●";
                                                        frameCount = 1;
                                                        
                                                        RealTimeBody.Limb_0.Clear();
                                                        RealTimeBody.Limb_1.Clear();
                                                        RealTimeBody.Limb_2.Clear();
                                                        RealTimeBody.Limb_3.Clear();
                                                        RealTimeBody.Limb_4.Clear();
                                                        RealTimeBody.Limb_5.Clear();
                                                        RealTimeBody.Limb_6.Clear();
                                                        RealTimeBody.Limb_7.Clear();

                                                        CameraSpacePoint newpoint1 = getNewXZPlanePoint(body.Joints[JointType.ElbowRight].Position, PlaneAngle);
                                                        CameraSpacePoint newpoint2 = getNewXZPlanePoint(body.Joints[JointType.ShoulderRight].Position, PlaneAngle);
                                                        RealTimeBody.Limb_0.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                        newpoint1 = getNewXZPlanePoint(body.Joints[JointType.ElbowLeft].Position, PlaneAngle);
                                                        newpoint2 = getNewXZPlanePoint(body.Joints[JointType.ShoulderLeft].Position, PlaneAngle);
                                                        RealTimeBody.Limb_1.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                        newpoint1 = getNewXZPlanePoint(body.Joints[JointType.KneeRight].Position, PlaneAngle);
                                                        newpoint2 = getNewXZPlanePoint(body.Joints[JointType.HipRight].Position, PlaneAngle);
                                                        RealTimeBody.Limb_2.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                        newpoint1 = getNewXZPlanePoint(body.Joints[JointType.KneeLeft].Position, PlaneAngle);
                                                        newpoint2 = getNewXZPlanePoint(body.Joints[JointType.HipLeft].Position, PlaneAngle);
                                                        RealTimeBody.Limb_3.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                        newpoint1 = getNewXZPlanePoint(body.Joints[JointType.WristRight].Position, PlaneAngle);
                                                        newpoint2 = getNewXZPlanePoint(body.Joints[JointType.ElbowRight].Position, PlaneAngle);
                                                        RealTimeBody.Limb_4.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                        newpoint1 = getNewXZPlanePoint(body.Joints[JointType.WristLeft].Position, PlaneAngle);
                                                        newpoint2 = getNewXZPlanePoint(body.Joints[JointType.ElbowLeft].Position, PlaneAngle);
                                                        RealTimeBody.Limb_5.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                        newpoint1 = getNewXZPlanePoint(body.Joints[JointType.AnkleRight].Position, PlaneAngle);
                                                        newpoint2 = getNewXZPlanePoint(body.Joints[JointType.KneeRight].Position, PlaneAngle);
                                                        RealTimeBody.Limb_6.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                        newpoint1 = getNewXZPlanePoint(body.Joints[JointType.AnkleLeft].Position, PlaneAngle);
                                                        newpoint2 = getNewXZPlanePoint(body.Joints[JointType.KneeLeft].Position, PlaneAngle);
                                                        RealTimeBody.Limb_7.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                    }
                                                    else
                                                    {
                                                        frameCount++;

                                                        CameraSpacePoint newpoint1 = getNewXZPlanePoint(body.Joints[JointType.ElbowRight].Position, PlaneAngle);
                                                        CameraSpacePoint newpoint2 = getNewXZPlanePoint(body.Joints[JointType.ShoulderRight].Position, PlaneAngle);
                                                        RealTimeBody.Limb_0.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                        newpoint1 = getNewXZPlanePoint(body.Joints[JointType.ElbowLeft].Position, PlaneAngle);
                                                        newpoint2 = getNewXZPlanePoint(body.Joints[JointType.ShoulderLeft].Position, PlaneAngle);
                                                        RealTimeBody.Limb_1.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                        newpoint1 = getNewXZPlanePoint(body.Joints[JointType.KneeRight].Position, PlaneAngle);
                                                        newpoint2 = getNewXZPlanePoint(body.Joints[JointType.HipRight].Position, PlaneAngle);
                                                        RealTimeBody.Limb_2.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                        newpoint1 = getNewXZPlanePoint(body.Joints[JointType.KneeLeft].Position, PlaneAngle);
                                                        newpoint2 = getNewXZPlanePoint(body.Joints[JointType.HipLeft].Position, PlaneAngle);
                                                        RealTimeBody.Limb_3.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                        newpoint1 = getNewXZPlanePoint(body.Joints[JointType.WristRight].Position, PlaneAngle);
                                                        newpoint2 = getNewXZPlanePoint(body.Joints[JointType.ElbowRight].Position, PlaneAngle);
                                                        RealTimeBody.Limb_4.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                        newpoint1 = getNewXZPlanePoint(body.Joints[JointType.WristLeft].Position, PlaneAngle);
                                                        newpoint2 = getNewXZPlanePoint(body.Joints[JointType.ElbowLeft].Position, PlaneAngle);
                                                        RealTimeBody.Limb_5.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                        newpoint1 = getNewXZPlanePoint(body.Joints[JointType.AnkleRight].Position, PlaneAngle);
                                                        newpoint2 = getNewXZPlanePoint(body.Joints[JointType.KneeRight].Position, PlaneAngle);
                                                        RealTimeBody.Limb_6.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                        newpoint1 = getNewXZPlanePoint(body.Joints[JointType.AnkleLeft].Position, PlaneAngle);
                                                        newpoint2 = getNewXZPlanePoint(body.Joints[JointType.KneeLeft].Position, PlaneAngle);
                                                        RealTimeBody.Limb_7.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });
                                                        
                                                    }
                                                }
                                                else
                                                {
                                                    if (StartMotion == true)
                                                    {
                                                        StartMotion = false;
                                                        LB3.Content = "";
                                                        if (frameCount > 20)
                                                        {
                                                            
                                                            nowMotion = MotionMatching(RealTimeBody, motionclass);
                                                            LB.Content += nowMotion + "\n";
                                                            /*
                                                            if (preMotion != nowMotion)
                                                            {
                                                                LB.Content += nowMotion + "\n";
                                                                preMotion = nowMotion;
                                                            }*/
                                                            //給出匹配結果
                                                        }
                                                    }
                                                    else
                                                    {
                                                        StartMotion = false;
                                                    }
                                                }

                                                for (int i = 0; i < nowbody.Length; i++)
                                                {
                                                    prebody[i].X = nowbody[i].X;
                                                    prebody[i].Y = nowbody[i].Y;
                                                    prebody[i].Z = nowbody[i].Z;
                                                }



                                                /*
                                                Bodycanvas.Children.Clear();
                                                CanvasAddJoint(body, JointType.Head);
                                                CameraSpacePoint HipLR_Vector = new CameraSpacePoint { X = body.Joints[JointType.HipRight].Position.X - body.Joints[JointType.HipLeft].Position.X, Y = body.Joints[JointType.HipRight].Position.Y - body.Joints[JointType.HipLeft].Position.Y, Z = body.Joints[JointType.HipRight].Position.Z - body.Joints[JointType.HipLeft].Position.Z };
                                                double PlaneAngle = Math.Atan2(HipLR_Vector.Z, HipLR_Vector.X);
                                                if (framecount < RealtimeBodyLength)
                                                {
                                                    CameraSpacePoint newpoint1 = getNewXZPlanePoint(body.Joints[JointType.ElbowRight].Position, PlaneAngle);
                                                    CameraSpacePoint newpoint2 = getNewXZPlanePoint(body.Joints[JointType.ShoulderRight].Position, PlaneAngle);
                                                    RealTimeBody.Limb_0.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                    newpoint1 = getNewXZPlanePoint(body.Joints[JointType.ElbowLeft].Position, PlaneAngle);
                                                    newpoint2 = getNewXZPlanePoint(body.Joints[JointType.ShoulderLeft].Position, PlaneAngle);
                                                    RealTimeBody.Limb_1.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                    newpoint1 = getNewXZPlanePoint(body.Joints[JointType.KneeRight].Position, PlaneAngle);
                                                    newpoint2 = getNewXZPlanePoint(body.Joints[JointType.HipRight].Position, PlaneAngle);
                                                    RealTimeBody.Limb_2.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                    newpoint1 = getNewXZPlanePoint(body.Joints[JointType.KneeLeft].Position, PlaneAngle);
                                                    newpoint2 = getNewXZPlanePoint(body.Joints[JointType.HipLeft].Position, PlaneAngle);
                                                    RealTimeBody.Limb_3.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                    newpoint1 = getNewXZPlanePoint(body.Joints[JointType.WristRight].Position, PlaneAngle);
                                                    newpoint2 = getNewXZPlanePoint(body.Joints[JointType.ElbowRight].Position, PlaneAngle);
                                                    RealTimeBody.Limb_4.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                    newpoint1 = getNewXZPlanePoint(body.Joints[JointType.WristLeft].Position, PlaneAngle);
                                                    newpoint2 = getNewXZPlanePoint(body.Joints[JointType.ElbowLeft].Position, PlaneAngle);
                                                    RealTimeBody.Limb_5.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                    newpoint1 = getNewXZPlanePoint(body.Joints[JointType.AnkleRight].Position, PlaneAngle);
                                                    newpoint2 = getNewXZPlanePoint(body.Joints[JointType.KneeRight].Position, PlaneAngle);
                                                    RealTimeBody.Limb_6.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                    newpoint1 = getNewXZPlanePoint(body.Joints[JointType.AnkleLeft].Position, PlaneAngle);
                                                    newpoint2 = getNewXZPlanePoint(body.Joints[JointType.KneeLeft].Position, PlaneAngle);
                                                    RealTimeBody.Limb_7.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });
                                                    framecount++;
                                                }
                                                else
                                                {
                                                    RealTimeBody.Limb_0.RemoveAt(0);
                                                    RealTimeBody.Limb_1.RemoveAt(0);
                                                    RealTimeBody.Limb_2.RemoveAt(0);
                                                    RealTimeBody.Limb_3.RemoveAt(0);
                                                    RealTimeBody.Limb_4.RemoveAt(0);
                                                    RealTimeBody.Limb_5.RemoveAt(0);
                                                    RealTimeBody.Limb_6.RemoveAt(0);
                                                    RealTimeBody.Limb_7.RemoveAt(0);
                                                    CameraSpacePoint newpoint1 = getNewXZPlanePoint(body.Joints[JointType.ElbowRight].Position, PlaneAngle);
                                                    CameraSpacePoint newpoint2 = getNewXZPlanePoint(body.Joints[JointType.ShoulderRight].Position, PlaneAngle);
                                                    RealTimeBody.Limb_0.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                    newpoint1 = getNewXZPlanePoint(body.Joints[JointType.ElbowLeft].Position, PlaneAngle);
                                                    newpoint2 = getNewXZPlanePoint(body.Joints[JointType.ShoulderLeft].Position, PlaneAngle);
                                                    RealTimeBody.Limb_1.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                    newpoint1 = getNewXZPlanePoint(body.Joints[JointType.KneeRight].Position, PlaneAngle);
                                                    newpoint2 = getNewXZPlanePoint(body.Joints[JointType.HipRight].Position, PlaneAngle);
                                                    RealTimeBody.Limb_2.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                    newpoint1 = getNewXZPlanePoint(body.Joints[JointType.KneeLeft].Position, PlaneAngle);
                                                    newpoint2 = getNewXZPlanePoint(body.Joints[JointType.HipLeft].Position, PlaneAngle);
                                                    RealTimeBody.Limb_3.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                    newpoint1 = getNewXZPlanePoint(body.Joints[JointType.WristRight].Position, PlaneAngle);
                                                    newpoint2 = getNewXZPlanePoint(body.Joints[JointType.ElbowRight].Position, PlaneAngle);
                                                    RealTimeBody.Limb_4.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                    newpoint1 = getNewXZPlanePoint(body.Joints[JointType.WristLeft].Position, PlaneAngle);
                                                    newpoint2 = getNewXZPlanePoint(body.Joints[JointType.ElbowLeft].Position, PlaneAngle);
                                                    RealTimeBody.Limb_5.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                    newpoint1 = getNewXZPlanePoint(body.Joints[JointType.AnkleRight].Position, PlaneAngle);
                                                    newpoint2 = getNewXZPlanePoint(body.Joints[JointType.KneeRight].Position, PlaneAngle);
                                                    RealTimeBody.Limb_6.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });

                                                    newpoint1 = getNewXZPlanePoint(body.Joints[JointType.AnkleLeft].Position, PlaneAngle);
                                                    newpoint2 = getNewXZPlanePoint(body.Joints[JointType.KneeLeft].Position, PlaneAngle);
                                                    RealTimeBody.Limb_7.Add(new CameraSpacePoint { X = newpoint1.X - newpoint2.X, Y = newpoint1.Y - newpoint2.Y, Z = newpoint1.Z - newpoint2.Z });
                                                    if (framecount % 5 == 0)
                                                    {
                                                        nowMotion = MotionMatching(RealTimeBody, motionclass);
                                                        if (preMotion != nowMotion)
                                                        {
                                                            LB.Content += nowMotion + "\n";
                                                            preMotion = nowMotion;
                                                        }

                                                    }
                                                    if (framecount >= RealtimeBodyLength + 15)
                                                    {
                                                        framecount = RealtimeBodyLength;
                                                    }
                                                    framecount++;

                                                }*/
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }

        }
        private double o4dis(CameraSpacePoint c1, CameraSpacePoint c2)
        {
            return Math.Sqrt((c1.X - c2.X) * (c1.X - c2.X) + (c1.Y - c2.Y) * (c1.Y - c2.Y) + (c1.Z - c2.Z) * (c1.Z - c2.Z));
        }
        public ImageSource ImageSource
        {
            get
            {
                return this.colorBitmap;
            }
        }
        private string MotionMatching(MotionFeature RealTimeMotion, MotionClass[] DataMotion)
        {
            List<double> Distance=new List<double>();
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
                    for (int i = 1; i < DataMotion[ClassNum].motionFeature[MotionNum].Limb_0.Count; i++)
                    {
                        DTWTable[0, i] = Double.PositiveInfinity;
                    }
                    //設起點成本為0
                    DTWTable[0, 0] = 0;
                    //跑動作的每個Frame的分數
                    for (int i = 1; i < RealTimeMotion.Limb_0.Count; i++)
                    {
                        for (int j = 1; j < DataMotion[ClassNum].motionFeature[MotionNum].Limb_0.Count; j++)
                        {
                            //cost = 100 - MotionScore(RealTimeMotion, motionclass[ClassNum].motionFeature[MotionNum], i, j);
                            cost = MotionDistance(RealTimeMotion, DataMotion[ClassNum].motionFeature[MotionNum], i, j, ClassNum);
                            double min = Math.Min(DTWTable[i - 1, j], DTWTable[i, j - 1]);
                            min = Math.Min(min, DTWTable[i - 1, j - 1]);
                            DTWTable[i, j] = cost + min;
                        }
                    }
                    int step = DTWSteps(DTWTable, DTWTable.GetLength(0) - 1, DTWTable.GetLength(1) - 1, 0);
                    double dis = DTWTable[RealTimeMotion.Limb_0.Count - 1, DataMotion[ClassNum].motionFeature[MotionNum].Limb_0.Count - 1] / step;

                    Distance.Add(dis- DataMotion[ClassNum].T3_Threshold);
                    motionname.Add(DataMotion[ClassNum].MotionName);
                }
            }
            //最小距離的動作
            double MinDistance = Distance.Min();
            
            LB2.Content = MinDistance;
            //每Frame最多差距50，就給出動作類別
            //if (MinDistance < 100)
            //{
                BestMatching = motionname[Distance.IndexOf(MinDistance)];
            //}
            
            return BestMatching;
        }
        private double MotionScore(MotionFeature A_motion, MotionFeature B_motion, int A_count,int B_count)
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

            double LVU = Math.Abs(angle[0]) + Math.Abs(angle[1]) + Math.Abs(angle[4]) + Math.Abs(angle[5]);//上肢
            double LVL = Math.Abs(angle[2]) + Math.Abs(angle[3]) + Math.Abs(angle[6]) + Math.Abs(angle[7]);//下肢
            double LV1 = Math.Abs(angle[0]) + Math.Abs(angle[1]) + Math.Abs(angle[2]) + Math.Abs(angle[3]);//一級
            double LV2 = Math.Abs(angle[4]) + Math.Abs(angle[5]) + Math.Abs(angle[6]) + Math.Abs(angle[7]);//二級
            double average = (Math.Abs(angle[0]) + Math.Abs(angle[1]) + Math.Abs(angle[2]) + Math.Abs(angle[3]) + Math.Abs(angle[4]) + Math.Abs(angle[5]) + Math.Abs(angle[6]) + Math.Abs(angle[7])) / 8;
            double Standard_deviation = Math.Sqrt(((angle[0] - average) * (angle[0] - average) + (angle[1] - average) * (angle[1] - average) + (angle[2] - average) * (angle[2] - average) + (angle[3] - average) * (angle[3] - average) + (angle[4] - average) * (angle[4] - average) + (angle[5] - average) * (angle[5] - average) + (angle[6] - average) * (angle[6] - average) + (angle[7] - average) * (angle[7] - average)) / 8);
            double metric = Standard_deviation + (LVU * 0.12 + LVL * 0.38 + LV1 * 0.24 + LV2 * 0.26) / 4;
            score = Math.Abs(Math.Round((35 - metric) * (100 - 70) / 35 + 70, 3));

            return score;
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

        private double cos_theta(CameraSpacePoint A_Limb,CameraSpacePoint B_Limb)
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
        /*private void LoadMotion2()
        {
            //資料夾路徑
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(filePath + "動作類別");
            //取得資料夾內該層的所有資料夾
            System.IO.DirectoryInfo[] fileNames = di.GetDirectories("*.*", System.IO.SearchOption.TopDirectoryOnly);
            motionclass = new MotionClass[fileNames.Length];

            for (int i = 0; i < fileNames.Length; i++)//類別的數量
            {
                DirectoryInfo Dir = new DirectoryInfo(filePath + "動作類別\\" + fileNames[i].ToString() + "\\");
                int motionAmount = Dir.GetFiles("*.Limb3D").Length;
                MotionFeature[] MFs = new MotionFeature[motionAmount];

                for (int m = 1; m <= motionAmount; m++)//類別內的動作數量
                {
                    string text = LoadFile(filePath + "動作類別\\" + fileNames[i].ToString() + "\\" + m.ToString() + ".Limb3D");
                    string[] A_Frame = text.Split(new Char[] { '@' });
                    MotionFeature MF = new MotionFeature
                    {
                        Limb_0 = new CameraSpacePoint[A_Frame.Length],
                        Limb_1 = new CameraSpacePoint[A_Frame.Length],
                        Limb_2 = new CameraSpacePoint[A_Frame.Length],
                        Limb_3 = new CameraSpacePoint[A_Frame.Length],
                        Limb_4 = new CameraSpacePoint[A_Frame.Length],
                        Limb_5 = new CameraSpacePoint[A_Frame.Length],
                        Limb_6 = new CameraSpacePoint[A_Frame.Length],
                        Limb_7 = new CameraSpacePoint[A_Frame.Length],
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
                                    MF.Limb_0[j] = new CameraSpacePoint { X = (float)Convert.ToDouble(xyz[0]), Y = (float)Convert.ToDouble(xyz[1]), Z = (float)Convert.ToDouble(xyz[2]) };
                                    break;
                                case 1:
                                    MF.Limb_1[j] = new CameraSpacePoint { X = (float)Convert.ToDouble(xyz[0]), Y = (float)Convert.ToDouble(xyz[1]), Z = (float)Convert.ToDouble(xyz[2]) };
                                    break;
                                case 2:
                                    MF.Limb_2[j] = new CameraSpacePoint { X = (float)Convert.ToDouble(xyz[0]), Y = (float)Convert.ToDouble(xyz[1]), Z = (float)Convert.ToDouble(xyz[2]) };
                                    break;
                                case 3:
                                    MF.Limb_3[j] = new CameraSpacePoint { X = (float)Convert.ToDouble(xyz[0]), Y = (float)Convert.ToDouble(xyz[1]), Z = (float)Convert.ToDouble(xyz[2]) };
                                    break;
                                case 4:
                                    MF.Limb_4[j] = new CameraSpacePoint { X = (float)Convert.ToDouble(xyz[0]), Y = (float)Convert.ToDouble(xyz[1]), Z = (float)Convert.ToDouble(xyz[2]) };
                                    break;
                                case 5:
                                    MF.Limb_5[j] = new CameraSpacePoint { X = (float)Convert.ToDouble(xyz[0]), Y = (float)Convert.ToDouble(xyz[1]), Z = (float)Convert.ToDouble(xyz[2]) };
                                    break;
                                case 6:
                                    MF.Limb_6[j] = new CameraSpacePoint { X = (float)Convert.ToDouble(xyz[0]), Y = (float)Convert.ToDouble(xyz[1]), Z = (float)Convert.ToDouble(xyz[2]) };
                                    break;
                                case 7:
                                    MF.Limb_7[j] = new CameraSpacePoint { X = (float)Convert.ToDouble(xyz[0]), Y = (float)Convert.ToDouble(xyz[1]), Z = (float)Convert.ToDouble(xyz[2]) };
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
        }*/
        private void LoadMotion()
        {
            //資料夾路徑
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(filePath + "動作類別");
            //取得資料夾內該層的所有資料夾
            System.IO.DirectoryInfo[] fileNames = di.GetDirectories("*.*", System.IO.SearchOption.TopDirectoryOnly);
            motionclass = new MotionClass[fileNames.Length];

            for(int i =0;i<fileNames.Length;i++)//類別的數量
            {
                DirectoryInfo Dir = new DirectoryInfo(filePath + "動作類別\\" + fileNames[i].ToString() + "\\");
                int motionAmount = Dir.GetFiles("*.Limb3D").Length;
                MotionFeature[] MFs = new MotionFeature[motionAmount];
                string T3=LoadFile(filePath + "動作類別\\" + fileNames[i].ToString() + "\\T3.txt") ;

                for (int m = 1; m <= motionAmount; m++)//類別內的動作數量
                {
                    string text = LoadFile(filePath + "動作類別\\" + fileNames[i].ToString() + "\\" + m.ToString() + ".Limb3D");
                    string[] A_Frame = text.Split(new Char[] { '@' });
                    MotionFeature MF = new MotionFeature {
                        Limb_0 = new List<CameraSpacePoint>() ,
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
                                    MF.Limb_0.Add( new CameraSpacePoint {X=(float)Convert.ToDouble(xyz[0]),Y=(float)Convert.ToDouble(xyz[1]),Z=(float)Convert.ToDouble(xyz[2]) });
                                    break;
                                case 1:
                                    MF.Limb_1.Add( new CameraSpacePoint { X = (float)Convert.ToDouble(xyz[0]), Y = (float)Convert.ToDouble(xyz[1]), Z = (float)Convert.ToDouble(xyz[2]) });
                                    break;
                                case 2:
                                    MF.Limb_2.Add( new CameraSpacePoint { X = (float)Convert.ToDouble(xyz[0]), Y = (float)Convert.ToDouble(xyz[1]), Z = (float)Convert.ToDouble(xyz[2]) });
                                    break;
                                case 3:
                                    MF.Limb_3.Add( new CameraSpacePoint { X = (float)Convert.ToDouble(xyz[0]), Y = (float)Convert.ToDouble(xyz[1]), Z = (float)Convert.ToDouble(xyz[2]) });
                                    break;
                                case 4:
                                    MF.Limb_4.Add( new CameraSpacePoint { X = (float)Convert.ToDouble(xyz[0]), Y = (float)Convert.ToDouble(xyz[1]), Z = (float)Convert.ToDouble(xyz[2]) });
                                    break;
                                case 5:
                                    MF.Limb_5.Add( new CameraSpacePoint { X = (float)Convert.ToDouble(xyz[0]), Y = (float)Convert.ToDouble(xyz[1]), Z = (float)Convert.ToDouble(xyz[2]) });
                                    break;
                                case 6:
                                    MF.Limb_6.Add( new CameraSpacePoint { X = (float)Convert.ToDouble(xyz[0]), Y = (float)Convert.ToDouble(xyz[1]), Z = (float)Convert.ToDouble(xyz[2]) });
                                    break;
                                case 7:
                                    MF.Limb_7.Add( new CameraSpacePoint { X = (float)Convert.ToDouble(xyz[0]), Y = (float)Convert.ToDouble(xyz[1]), Z = (float)Convert.ToDouble(xyz[2]) });
                                    break;
                                default:
                                    MessageBox.Show("讀檔錯誤");
                                    break;
                            }
                            
                        }
                    }
                    MFs[m - 1] = MF;
                }
                motionclass[i].T3_Threshold = Convert.ToDouble(T3);

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
        private CameraSpacePoint getNewXZPlanePoint(CameraSpacePoint point, double angle)//旋轉X-Z平面上坐標系得到點的新座標
        {
            CameraSpacePoint newpoint = new CameraSpacePoint { X = (float)(Math.Cos(angle) * point.X + Math.Sin(angle) * point.Z), Y = point.Y, Z = (float)(-Math.Sin(angle) * point.X + Math.Cos(angle) * point.Z) };
            return newpoint;
        }
        private void CanvasAddJoint(Body body, JointType JT)
        {
            Joint joint = body.Joints[JT];
            if (joint.TrackingState == TrackingState.Tracked)
            {
                ColorSpacePoint dsp = kinectSensor.CoordinateMapper.MapCameraPointToColorSpace(joint.Position);
                double scaleX = (double)1920.0 / Bodycanvas.Width;
                double scaleY = (double)1080.0 / Bodycanvas.Height;

                System.Windows.Shapes.Ellipse jointCircle = new System.Windows.Shapes.Ellipse { Width = 18, Height = 18, Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0)) };
                Bodycanvas.Children.Add(jointCircle);
                Canvas.SetLeft(jointCircle, dsp.X / scaleX - 9);
                Canvas.SetTop(jointCircle, dsp.Y / scaleY - 9);
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.multiFrameReader != null)
            {
                // ColorFrameReder is IDisposable
                this.multiFrameReader.Dispose();
                this.multiFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }
    }
}
