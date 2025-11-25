using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeriphericalControl
{
    public class InputEvent
    {
        public TimeSpan Time { get; }
        public string Text { get; }

        public InputEvent(TimeSpan time, string text)
        {
            Time = time;
            Text = text;
        }
    }
}
