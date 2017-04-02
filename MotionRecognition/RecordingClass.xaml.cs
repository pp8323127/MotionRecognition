using System;
using System.IO;
using System.Windows;

namespace MotionRecognition
{
    /// <summary>
    /// RecodeWindows.xaml 的互動邏輯
    /// </summary>
    public partial class RecordingClass : Window
    {
        private string filePath = System.IO.Directory.GetCurrentDirectory() + "\\";

        public RecordingClass()
        {
            InitializeComponent();
            //創建資料夾，如果已存在CreateDirectory不回執行任何動作，也不會擲回例外狀況
            System.IO.Directory.CreateDirectory(filePath+"動作類別");
            //資料夾路徑
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(filePath + "動作類別");
            //取得資料夾內該層的所有資料夾
            System.IO.DirectoryInfo[] fileNames = di.GetDirectories("*.*", System.IO.SearchOption.TopDirectoryOnly);
            //將所有動作類別新增到ComboBox的Items內
            foreach (System.IO.DirectoryInfo DI in fileNames)
            {
                CB.Items.Add(DI);
            }
            
        }

        private void Determine_Click(object sender, RoutedEventArgs e)
        {
            if (newRB.IsChecked.Value)
            {
                if (TB.Text != "")
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(filePath + "動作類別\\" + TB.Text);
                        RecordingWindows RW = new RecordingWindows();
                        RW.FilePath = filePath + "動作類別\\" + TB.Text;

                        CreateFile(filePath + "動作類別\\" + TB.Text+"\\T3.txt","0");

                        RW.Show();
                        this.Close();
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("類別名稱含有非法字元");
                    }
                }
                else
                {
                    MessageBox.Show("請輸入類別名稱");
                }
            }
            else if(selectRB.IsChecked.Value)
            {
                if (CB.SelectedItem == null)
                {
                    MessageBox.Show("請選擇類別");
                }
                else
                {
                    RecordingWindows RW = new RecordingWindows();
                    RW.FilePath = filePath + "動作類別\\" + CB.SelectedItem.ToString();
                    RW.Show();
                    this.Close();
                }
                
            }
            else
            {
                MessageBox.Show("請選取「創建類別」或「選擇類別」");
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow MW = new MainWindow();
            MW.Show();
            this.Close();
        }
        private void CreateFile(string FileName, string contain)
        {
            FileInfo fh = new FileInfo(FileName);
            StreamWriter sw = fh.CreateText();
            sw.WriteLine(contain);
            sw.Flush();
            sw.Close();
        }

    }
}
