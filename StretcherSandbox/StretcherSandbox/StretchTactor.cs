using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows.Shapes;
using System.Threading;

namespace StretcherSandbox
{
    class StretchTactor
    {
        List<TimePosition> TimePositionList;
        SerialPort sp;

        public StretchTactor(SerialPort serial)
        {
            TimePositionList = new List<TimePosition>();
            sp = serial;
            Rectangle startRec = new Rectangle();
            startRec.Name = "startRec";
            Rectangle endRec = new Rectangle();
            endRec.Name = "endRec";
            TimePosition startTP = new TimePosition(0, 0, startRec);
            TimePosition endTP = new TimePosition(5000, 0, endRec);
            TimePositionList.Add(startTP);
            TimePositionList.Add(endTP);
        }

        public bool addTP(TimePosition newTP)
        {
            foreach(TimePosition tp in TimePositionList)
            {
                if(tp.time > newTP.time)
                {
                    TimePositionList.Insert(TimePositionList.IndexOf(tp), newTP);
                    return true;
                }
            }
            return false;
        }

        public bool removeTP(string rmRecName)
        {
            foreach(TimePosition tp in TimePositionList)
            {
                if(tp.getRecName().Equals(rmRecName))
                {
                    TimePositionList.Remove(tp);
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            string logString = "";
            foreach (TimePosition tp in TimePositionList)
            {
                logString += tp.ToString() + " | ";
            }
            return logString;
        }

        public void playPattern(int tactorNum)
        {
            for (int i = 0; i < TimePositionList.Count - 1; i++)
            {
                double patternTime = TimePositionList[i + 1].time - TimePositionList[i].time;
                double startDegree = TimePositionList[i].degree;
                double endDegree = TimePositionList[i + 1].degree;

                playTactor(patternTime, startDegree, endDegree, tactorNum);
            }
            sp.WriteLine($"t{tactorNum}p000");
            Thread.Sleep(100);
            sp.WriteLine($"t{tactorNum}p999"); // turn off servo
        }

        void playTactor(double actTime, double degFrom, double degTo, int tNum)
        {
            double currDeg = degFrom;
            double incDeg = (degTo - degFrom) / actTime * 25.0;

            DateTime startTime = DateTime.Now;
            while((DateTime.Now - startTime).TotalMilliseconds < actTime)
            {
                string cmd = String.Format("t{0}p{1,3:D3}", tNum, (int)currDeg);
                sp.WriteLine(cmd);
                //Console.WriteLine(cmd);
                currDeg += incDeg;
                Thread.Sleep(25);
            }

            
        }
    }

    class TimePosition
    {
        public double time;
        public double degree;
        Rectangle tpRec;
        delegate string RecNameBack();

        public TimePosition(double t, double d, Rectangle tmpRec)
        {
            time = t;
            degree = d;
            tpRec = tmpRec;
        }

        public string getRecName()
        {
            if (tpRec.Dispatcher.CheckAccess())
            {
                return tpRec.Name;
            }
            else
            {
                RecNameBack d = new RecNameBack(getRecName);
                return (string)tpRec.Dispatcher.Invoke(d, new object[] { });
            }
        }

        public override string ToString()
        {
            return tpRec.Name + ": " + time.ToString() + ", " + degree.ToString();
        }
    }
}
