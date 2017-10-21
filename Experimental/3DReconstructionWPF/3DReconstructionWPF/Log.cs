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

        public static void initLog(TextBox tb)
        {
            LogArea = tb;
        }

        public static void writeLog(String s)
        {
            
            if(LogArea != null)
            LogArea.AppendText(DateTime.Now.TimeOfDay.Hours +":"+ DateTime.Now.TimeOfDay.Minutes + " >> " + s+"\n");
        }
    }
}
