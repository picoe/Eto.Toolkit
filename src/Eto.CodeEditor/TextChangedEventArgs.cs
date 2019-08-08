using System;
using System.Collections.Generic;

namespace Eto.CodeEditor
{
    public class CharAddedEventArgs : EventArgs
    {
        public CharAddedEventArgs(char c)
        {
            Char = c;
        }

        public char Char { get; }
    }

    public class TextChangedEventArgs : EventArgs
    {
    }
}
