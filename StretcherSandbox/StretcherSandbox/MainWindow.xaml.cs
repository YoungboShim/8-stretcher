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
        StretchGraph[,] stretchGraphs = new StretchGraph[2, 4];

        public MainWindow()
        {
            InitializeComponent();

            InitSerialPort();
            CmdTextBox.KeyDown += new KeyEventHandler(EnterKeyDownHandler);
            tactorTimeLine = new StretchTactor(serial);

            Loaded += delegate
            {
                for (int i = 0;i < 2;i++)
                {
                    for (int j = 0;j < 4; j++)
                    {
                        stretchGraphs[i, j] = new StretchGraph(serial);
                        MainGrid.Children.Add(stretchGraphs[i, j]);
                        stretchGraphs[i, j].initSretchGraph(MainGrid.ColumnDefinitions[0].ActualWidth - 10, MainGrid.RowDefinitions[0].ActualHeight - 10);
                        Grid.SetColumn(stretchGraphs[i, j], j);
                        Grid.SetRow(stretchGraphs[i, j], i);
                    }
                }
            };
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

        private void PlayBtn_Click_1(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    stretchGraphs[i, j].playGraph(j + i * 4 + 1);
                }
            }
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    stretchGraphs[i, j].clearGraph();
                }
            }
        }
    }
}
