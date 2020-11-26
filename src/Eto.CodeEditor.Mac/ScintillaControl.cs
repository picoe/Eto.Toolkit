using System;
using Scintilla;

namespace Scintilla
{
    public partial class ScintillaControl : ScintillaNET.ScintillaView, Eto.Mac.Forms.IMacControl
    {
        public WeakReference WeakHandler { get; set; }

        private IntPtr SciPointer => new IntPtr(-1); // not needed on macOS

        public ScintillaControl()
        {
            directFunction = (_, m, w, l) => Message((uint)m, w, l);
            init();
        }
    }
}
