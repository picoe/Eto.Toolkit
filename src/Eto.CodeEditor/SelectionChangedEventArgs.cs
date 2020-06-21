using System;
namespace Eto.CodeEditor
{
    public class SelectionChangedEventArgs : EventArgs
    {
        public SelectionChangedEventArgs(bool selectionIsEmpty, int selectionStart, int selectionEnd, string selectionText)
        {
            SelectionIsEmpty = selectionIsEmpty;
            SelectionStart = selectionStart;
            SelectionEnd = selectionEnd;
            SelectionText = selectionText;
        }

        public bool SelectionIsEmpty { get; }
        public int SelectionStart { get; }
        public int SelectionEnd { get; }
        public string SelectionText { get; }
    }
}
