using System;
using Scintilla;

namespace Scintilla
{
    public partial class ScintillaControl : ScintillaNET.ScintillaView
    {
        private IntPtr SciPointer => new IntPtr(-1); // not needed on macOS

        public ScintillaControl()
        {
            directFunction = (_, m, w, l) => Message((uint)m, w, l);
        }
    }
}
