using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BurinXApp.Observer
{
    public class TimeObserver : IObserver
    {
        private TextBlock timeTextBlock;

        public TimeObserver(TextBlock timeTextBlock)
        {
            this.timeTextBlock = timeTextBlock;
        }

        public void Update(string time)
        {
            timeTextBlock.Text = time;
        }
    }

}
