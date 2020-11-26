using System;
using System.Collections.Generic;
using System.Text;

namespace Eto.CodeEditor
{
    public enum CallTipMove { Current, Previous, Next };
    public class CallTipClickedEventArgs : EventArgs
    {
        public CallTipClickedEventArgs(int position)
        {
            if (position < 0 || position > 2)
                position = 0;
            Move = (CallTipMove)position;
        }

        public CallTipMove Move { get; }
    }
}
