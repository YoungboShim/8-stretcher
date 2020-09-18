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
using System.IO;
using Newtonsoft.Json;

namespace StretcherSandbox
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort serial = new SerialPort();
        delegate void SetTextCallBack(String text);
        StretchTactor tactorTimeLine;
        StretchGraph[,] stretchGraphs = new StretchGraph[2, 4];
        Dictionary<string, List<TimePosition>[,]> efxList = new Dictionary<string, List<TimePosition>[,]>();

        public MainWindow()
        {
            InitializeComponent();

            InitSerialPort();
            CmdTextBox.KeyDown += new KeyEventHandler(EnterKeyDownHandler);
            tactorTimeLine = new StretchTactor();
            InitLoadEffectList();

            Loaded += delegate
            {
                for (int i = 0;i < 2;i++)
                {
                    for (int j = 0;j < 4; j++)
                    {
                        stretchGraphs[i, j] = new StretchGraph();
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
                    stretchGraphs[i, j].playGraph(j + i * 4 + 1, serial);
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"..\..\" + "EffectList.txt");

            string effectName = EffectNameTextBox.Text;
            EffectNameTextBox.Text = "";

            List<TimePosition>[,] tactorList = new List<TimePosition>[2,4];

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    string tactorJson = stretchGraphs[i, j].ToJson();
                    List<TimePosition> tactor = JsonConvert.DeserializeObject<List<TimePosition>>(tactorJson);
                    tactorList[i, j] = tactor;
                    //Console.WriteLine(tactorList[i, j].ToString());
                }
            }

            efxList.Add(effectName, tactorList);
            string json = JsonConvert.SerializeObject(efxList);
            sw.WriteLine(json);
            sw.Flush();
            sw.Close();

            EffectListBox.Items.Add(effectName);
        }

        private void InitLoadEffectList()
        {
            StreamReader sw = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + @"..\..\" + "EffectList.txt");

            string json = sw.ReadToEnd();
            efxList = JsonConvert.DeserializeObject<Dictionary<string, List<TimePosition>[,]>>(json);

            foreach(KeyValuePair<string, List<TimePosition>[,]> kvp in efxList)
            {
                EffectListBox.Items.Add(kvp.Key);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"..\..\" + "EffectList.txt");

            string item = (string)EffectListBox.SelectedItem;
            if(item == null)
            {
                return;
            }
            else
            {
                efxList.Remove(item);
                string json = JsonConvert.SerializeObject(efxList);
                sw.WriteLine(json);
                sw.Flush();
                sw.Close();

                EffectListBox.Items.Remove(EffectListBox.SelectedItem);
            }

        }

        private void EffectListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void EffectListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("efxbox clicked~");
            string item = (string)EffectListBox.SelectedItem;
            if(item != null)
            {
                LoadEffect(item);
            }
        }

        private void LoadEffect(string effectName)
        {
            List<TimePosition>[,] tpList = efxList[effectName];
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    stretchGraphs[i, j].clearGraph();
                    for (int k = 1; k < tpList[i, j].Count - 1; k++)
                    {
                        double tmpTime = tpList[i, j][k].time;
                        double tmpDegree = tpList[i, j][k].degree;
                        stretchGraphs[i, j].AddTpPoint(stretchGraphs[i, j].pointFromTp(tmpTime, tmpDegree));
                    }                    
                }
            }
        }
    }
}
