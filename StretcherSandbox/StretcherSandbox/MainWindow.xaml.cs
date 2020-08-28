using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Threading;

namespace StretcherSandbox
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort serial = new SerialPort();
        delegate void SetTextCallBack(String text);
        int RecID = 0;
        StretchTactor tactorTimeLine;

        public MainWindow()
        {
            InitializeComponent();

            InitSerialPort();
            CmdTextBox.KeyDown += new KeyEventHandler(EnterKeyDownHandler);
            canvas.MouseLeftButtonDown += new MouseButtonEventHandler(canvas_MouseLeftButtonDown);
            tactorTimeLine = new StretchTactor(serial);
        }

        private void InitSerialPort()
        {
            serial.BaudRate = 38400;
            serial.DataReceived += new SerialDataReceivedEventHandler(serial_DataReceived);
        }

        private void serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string line = serial.ReadExisting();
                if (line != string.Empty)
                {
                    SetLog(line);
                }
            }
            catch (TimeoutException)
            {
            }
        }

        private void SetLog(string text)
        {
            if(LogBox.Dispatcher.CheckAccess())
            {
                LogBox.Text += text;
                svLogBox.ScrollToEnd();
            }
            else
            {
                SetTextCallBack d = new SetTextCallBack(SetLog);
                LogBox.Dispatcher.Invoke(d, new object[] { text });
            }
        }

        private void EnterKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SendSerialCmd();
            }
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                SerialComboBox.Items.Add(port);
            }
        }

        private void ConnectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (serial.IsOpen)
            {
                serial.Close();
            }

            ConnectBtn.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#ffdddddd"));
            serial.PortName = SerialComboBox.SelectedItem.ToString();
            serial.BaudRate = 38400;

            try
            {
                serial.Open();
                string line = serial.ReadExisting();
                LogBox.Text += line + "\n";
                ConnectBtn.Background = Brushes.Orange;
            }
            catch (Exception ex)
            {
                LogBox.Text += "Serial port open failed\n";
            }
        }

        private void SendSerialCmd()
        {
            if (serial.IsOpen)
            {
                serial.WriteLine(CmdTextBox.Text);
                CmdTextBox.Text = String.Empty;
            }
        }

        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            SendSerialCmd();
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (canvas.IsMouseDirectlyOver)
            {
                Point clickPoint = e.GetPosition(canvas);
                Rectangle newRec = AddRectangleToCanvas(clickPoint);
                TimePosition newTP = tpFromPoint(clickPoint, newRec);

                tactorTimeLine.addTP(newTP);
                Console.WriteLine(tactorTimeLine.ToString());
            }
        }

        private Rectangle AddRectangleToCanvas(Point point)
        {
            Rectangle tmpRec = new Rectangle();
            tmpRec.Stroke = Brushes.Red;
            tmpRec.Fill = Brushes.Red;
            tmpRec.Margin = new Thickness(point.X - 5, point.Y - 5, point.X + 5, point.Y + 5);
            tmpRec.Width = 10;
            tmpRec.Height = 10;
            tmpRec.MouseLeftButtonDown += new MouseButtonEventHandler(rec_MouseLeftButtonDown);
            tmpRec.Name = "tpRec" + (RecID++).ToString();

            canvas.Children.Add(tmpRec);
            return tmpRec;
        }

        private TimePosition tpFromPoint(Point point, Rectangle rect)
        {
            double tpTime = point.X / canvas.Width * 5000.0;
            double tpDegree = (canvas.Height - point.Y) / canvas.Height * 180.0;
            return new TimePosition(tpTime, tpDegree, rect);
        }

        private void rec_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            tactorTimeLine.removeTP(((Rectangle)sender).Name);
            canvas.Children.Remove((UIElement)sender);
            Console.WriteLine(tactorTimeLine.ToString());
        }

        private void PlayBtn_Click_1(object sender, RoutedEventArgs e)
        {
            Thread playThread = new Thread(tactorTimeLine.playPattern);
            playThread.Start();
        }
    }
}
