using System;
namespace Eto.CodeEditor
{
    public enum BreakpointChangeType {Add, Remove, Clear};

    public class BreakpointsChangedEventArgs : EventArgs
    {
        public BreakpointsChangedEventArgs(BreakpointChangeType addOrRemoveOrClear, int lineNumber = -1)
        {
            LineNumber = lineNumber;
            ChangeType = addOrRemoveOrClear;
        }

        public int LineNumber { get; private set; }
        public BreakpointChangeType ChangeType { get; private set; }
    }
}
