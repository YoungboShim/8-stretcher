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
using System.Threading;
using System.IO.Ports;

namespace StretcherSandbox
{
    /// <summary>
    /// StretchGraph.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class StretchGraph : UserControl
    {
        int RecID = 0;
        StretchTactor tactorTimeLine;
        SerialPort serial;

        public StretchGraph(SerialPort sp)
        {
            InitializeComponent();

            SGcanvas.MouseLeftButtonDown += new MouseButtonEventHandler(canvas_MouseLeftButtonDown);
            serial = sp;
            tactorTimeLine = new StretchTactor(serial);
        }

        public void initSretchGraph(double width, double height)
        {
            Line xAxisLine = new Line();
            xAxisLine.Stroke = Brushes.Black;
            xAxisLine.StrokeThickness = 4;
            xAxisLine.X1 = 5;
            xAxisLine.X2 = width - 5;
            xAxisLine.Y1 = height - 5;
            xAxisLine.Y2 = height - 5;
            SGcanvas.Children.Add(xAxisLine);

            Line yAxisLine = new Line();
            yAxisLine.Stroke = Brushes.Black;
            yAxisLine.StrokeThickness = 4;
            yAxisLine.X1 = 5;
            yAxisLine.X2 = 5;
            yAxisLine.Y1 = 5;
            yAxisLine.Y2 = height - 5;
            SGcanvas.Children.Add(yAxisLine);

            Line[] xSubLines = new Line[4];
            for (int i = 0; i < 4; i++)
            {
                xSubLines[i] = new Line();
                xSubLines[i].Stroke = Brushes.Gray;
                xSubLines[i].StrokeThickness = 1;
                xSubLines[i].X1 = 5;
                xSubLines[i].X2 = width - 5;
                xSubLines[i].Y1 = 5 + (height - 10) / 4 * i;
                xSubLines[i].Y2 = 5 + (height - 10) / 4 * i;
                SGcanvas.Children.Add(xSubLines[i]);
            }

            Line[] ySubLines = new Line[5];
            for (int i = 0; i < 5; i++)
            {
                ySubLines[i] = new Line();
                ySubLines[i].Stroke = Brushes.Gray;
                ySubLines[i].StrokeThickness = 1;
                ySubLines[i].X1 = 5 + (width - 10) / 5 * (i + 1);
                ySubLines[i].X2 = 5 + (width - 10) / 5 * (i + 1);
                ySubLines[i].Y1 = 5;
                ySubLines[i].Y2 = height - 5;
                SGcanvas.Children.Add(ySubLines[i]);
            }

            Rectangle startRec = new Rectangle();
            startRec.Stroke = Brushes.Blue;
            startRec.Fill = Brushes.Blue;
            startRec.Margin = new Thickness(0, height - 10, 10, height);
            startRec.Width = 10;
            startRec.Height = 10;
            startRec.Uid = "startRec";
            SGcanvas.Children.Add(startRec);

            Rectangle endRec = new Rectangle();
            endRec.Stroke = Brushes.Blue;
            endRec.Fill = Brushes.Blue;
            endRec.Margin = new Thickness(width - 10, height - 10, width, height);
            endRec.Width = 10;
            endRec.Height = 10;
            endRec.Uid = "endRec";
            SGcanvas.Children.Add(endRec);
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SGcanvas.IsMouseDirectlyOver)
            {
                Point clickPoint = e.GetPosition(SGcanvas);
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
            tmpRec.Uid = "tpRec" + (RecID++).ToString();

            SGcanvas.Children.Add(tmpRec);
            return tmpRec;
        }

        private TimePosition tpFromPoint(Point point, Rectangle rect)
        {
            double tpTime = point.X / SGcanvas.ActualWidth * 5000.0;
            double tpDegree = (SGcanvas.ActualHeight - point.Y) / SGcanvas.ActualHeight * 180.0;
            Console.WriteLine("tpTime: " + tpTime.ToString() + ", tpDegree: " + tpDegree.ToString());
            return new TimePosition(tpTime, tpDegree, rect);
        }

        private void rec_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            tactorTimeLine.removeTP(((Rectangle)sender).Uid);
            SGcanvas.Children.Remove((UIElement)sender);
            Console.WriteLine(tactorTimeLine.ToString());
        }

        public void playGraph(int graphNum)
        {
            Thread playThread = new Thread(() => tactorTimeLine.playPattern(graphNum));
            playThread.Start();
        }

        public string ToLogLine()
        {
            return tactorTimeLine.ToLogFormat();
        }

        public void clearGraph()
        {
            List<UIElement> removeRecList = new List<UIElement>();
            tactorTimeLine = new StretchTactor(serial);
            foreach (UIElement elem in SGcanvas.Children)
            {
                if(elem.Uid.Contains("tpRec"))
                {
                    removeRecList.Add(elem);
                }
            }
            foreach (UIElement elem in removeRecList)
            {
                SGcanvas.Children.Remove(elem);
            }
            RecID = 0;
        }
    }
}
