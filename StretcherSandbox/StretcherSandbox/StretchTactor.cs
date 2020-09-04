using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows.Shapes;
using System.Threading;
using Newtonsoft.Json;

namespace StretcherSandbox
{
    class StretchTactor
    {
        List<TimePosition> TimePositionList;

        public StretchTactor()
        {
            TimePositionList = new List<TimePosition>();
            Rectangle startRec = new Rectangle();
            startRec.Uid = "startRec";
            Rectangle endRec = new Rectangle();
            endRec.Uid = "endRec";
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

        public bool removeTP(string rmRecUid)
        {
            foreach(TimePosition tp in TimePositionList)
            {
                if(tp.getRecUid().Equals(rmRecUid))
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

        public void playPattern(int tactorNum, SerialPort sp)
        {
            for (int i = 0; i < TimePositionList.Count - 1; i++)
            {
                double patternTime = TimePositionList[i + 1].time - TimePositionList[i].time;
                double startDegree = TimePositionList[i].degree;
                double endDegree = TimePositionList[i + 1].degree;

                playTactor(patternTime, startDegree, endDegree, tactorNum, sp);
            }
            sp.WriteLine($"t{tactorNum}p000");
            Thread.Sleep(100);
            sp.WriteLine($"t{tactorNum}p999"); // turn off servo
        }

        void playTactor(double actTime, double degFrom, double degTo, int tNum, SerialPort sp)
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

        public List<TimePosition> getList()
        {
            return TimePositionList;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(TimePositionList);
        }
    }

    class TimePosition
    {
        public double time;
        public double degree;
        Rectangle tpRec;
        delegate string RecUidBack();

        public TimePosition(double t, double d, Rectangle tmpRec)
        {
            time = t;
            degree = d;
            tpRec = tmpRec;
        }

        public string getRecUid()
        {
            if (tpRec.Dispatcher.CheckAccess())
            {
                return tpRec.Uid;
            }
            else
            {
                RecUidBack d = new RecUidBack(getRecUid);
                return (string)tpRec.Dispatcher.Invoke(d, new object[] { });
            }
        }

        public override string ToString()
        {
            return tpRec.Uid + ": " + time.ToString() + ", " + degree.ToString();
        }
    }
}
