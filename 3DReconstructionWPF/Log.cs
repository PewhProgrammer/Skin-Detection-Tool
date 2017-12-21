using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;

namespace _3DReconstructionWPF
{
    class Log
    {
        public static TextBox LogArea;
        public static Label StatsLabel;

        public enum Stats
        {
            FPS,
        }

        public static void InitLog(TextBox tb, Label l)
        {
            StatsLabel = l;
            LogArea = tb;
        }

        public static void WriteLog(String s)
        {
            
            if(LogArea != null)
            LogArea.AppendText(DateTime.Now.TimeOfDay.Hours +":"+ DateTime.Now.TimeOfDay.Minutes + " >> " + s+"\n");
            //System.Windows.Forms.Application.DoEvents();
        }

        public static void WriteStats(String s, Stats stats)
        {
            var content = "";
            switch (stats)
            {
                case Stats.FPS:
                    {
                        content += "fps: ";
                        break;
                    }
            }
            StatsLabel.Content = content + s;
            

        }
    }
}
