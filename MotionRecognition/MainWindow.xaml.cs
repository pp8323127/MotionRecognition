using System.Windows;

namespace MotionRecognition
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            

        }

        private void RecodeButton_Click(object sender, RoutedEventArgs e)
        {
            RecordingClass RW = new RecordingClass();
            this.Close();
            RW.Show();
        }

        private void RealTimeRecognitionButton_Click(object sender, RoutedEventArgs e)
        {
            RealTimeIdentifyAction RTIA = new RealTimeIdentifyAction();
            this.Close();
            RTIA.Show();
        }
        
        private void RecognitionButton_Click(object sender, RoutedEventArgs e)
        {
            IdentifyAction IA = new IdentifyAction();
            this.Close();
            IA.Show();
        }
        

        private void DataBaseButton_Click(object sender, RoutedEventArgs e)
        {
            Replay DB = new Replay();
            this.Close();
            DB.Show();
        }
    }
}
