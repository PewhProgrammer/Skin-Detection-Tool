using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;

namespace _3DReconstructionWPF.Control
{
    class ButtonClickHandler
    {
        public ButtonClickHandler()
        {
            //init all buttons

            Button btn = new Button();
            btn.Name = "button";
            btn.Click += startScan_Click;
        }

        private void startScan_Click(object sender, RoutedEventArgs e)
        {
           
            
        }
    }
}
