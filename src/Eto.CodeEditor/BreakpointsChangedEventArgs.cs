using System;
namespace Eto.CodeEditor
{
    public enum BreakpointChangeType {Add, Remove};

    public class BreakpointsChangedEventArgs : EventArgs
    {
        public BreakpointsChangedEventArgs(int lineNumber, BreakpointChangeType addOrRemove)
        {
            LineNumber = lineNumber;
            AddOrRemove = addOrRemove;
        }

        public int LineNumber { get; private set; }
        public BreakpointChangeType AddOrRemove { get; private set; }
    }
}
