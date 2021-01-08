using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace PDFResizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
        }

        private bool StartProcess(string runFilePath, string args)
        {
            Process process = new Process();   
            ProcessStartInfo startInfo = new ProcessStartInfo(runFilePath, args);
            process.StartInfo = startInfo;
            startInfo.RedirectStandardInput = true;      //接受来自调用程序的输入     
            startInfo.RedirectStandardOutput = true;     //由调用程序获取输出信息
            startInfo.CreateNoWindow = true;             //不显示调用程序的窗口 
            process.Start();
            return true;
        }

        private static long GetFileSize(string sFullName)
        {
            long lSize = 0;
            if (File.Exists(sFullName))
                lSize = new FileInfo(sFullName).Length;
            return lSize;
        }

        private void DataHandler(string args)
        {
            string QualitySettings = " -dPDFSETTINGS=" + args;
            string OutFilename = " -sOutputFile=" + FilePathTextbox.Text + "_compressed.pdf ";
            string gsargs = "-sDEVICE=pdfwrite -dNOPAUSE -dBATCH" +
                QualitySettings + OutFilename + FilePathTextbox.Text;
            long FileSize = GetFileSize(FilePathTextbox.Text);
            int t = 10;
            if (FileSize < 1000000)
            {
                t = 10;
            }
            else
            {
                if (FileSize < 10000000)
                {
                    t = 80;
                }
                else
                {
                    t = 400;
                }
            }
            Task.Run(() =>
            {
                StartProcess(@"gs\gswin64c.exe", gsargs);
                for (int i = 0; i < 100; i++)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Progress.Value = i;
                    }), System.Windows.Threading.DispatcherPriority.Normal);
                    System.Threading.Thread.Sleep(t);
                }
            });
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            switch ((QualitySlider.Value).ToString())
            {
                case "0":
                    QualityLabel.Content = "最小（质量最好）";
                    break;
                case "10":
                    QualityLabel.Content = "较小（质量较好）";
                    break;
                case "20":
                    QualityLabel.Content = "较大（质量较差）";
                    break;
                case "30":
                    QualityLabel.Content = "最大（质量最差）";
                    break;
            }
        }

        private void CompressButton_Click(object sender, RoutedEventArgs e)
        {
            if (FilePathTextbox.Text.Length < 5)
            {
                MessageBox.Show("请选择文件！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string quality = "screen";
            switch ((QualitySlider.Value).ToString())
            {
                case "0":
                    quality = "/prepress";
                    break;
                case "10":
                    quality = "/printer";
                    break;
                case "20":
                    quality = "/ebook";
                    break;
                case "30":
                    quality = "/screen";
                    break;
            }
            DataHandler(quality);
            ButtonLabel.Content = "处理中";
            CompressButton.IsEnabled = false;
        }

        private void Progress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Progress.Value.ToString() == "99")
            {
                ButtonLabel.Content = "开始压缩";
                CompressButton.IsEnabled = true;
                Progress.Value = 0;
                MessageBoxButton MSGBtn = MessageBoxButton.OK;
                MessageBoxImage MSGImg = MessageBoxImage.Information;
                MessageBox.Show("压缩完成，请在原文件目录查看压缩后的文件.", "消息", MSGBtn, MSGImg);
            }
        }

        private void FileSelectButton_Click(object sender, RoutedEventArgs e)
        {
            var OpenFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "PDF文件|*.pdf"
            };
            var Result = OpenFileDialog.ShowDialog();
            if (Result == true)
            {
                FilePathTextbox.Text = OpenFileDialog.FileName;
            }
        }
    }
}
