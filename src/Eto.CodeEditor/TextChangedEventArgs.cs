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

    //public class InsertCheckEventArgs : EventArgs
    //{
    //    public InsertCheckEventArgs(string text)
    //    {
    //        Text = text;
    //    }

    //    public string Text { get; }
    //}
}
