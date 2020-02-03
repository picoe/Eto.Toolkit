using System;
using System.Windows.Forms;
using System.IO;
using ScintillaNET;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Scintilla;
using Eto.CodeEditor;

namespace Scintilla
{
    public partial class ScintillaControl : Control//, CodeEditor.IHandler
    {
        private static IntPtr moduleHandle;
        private IntPtr sciPtr;
        private BorderStyle borderStyle;
        //private static NativeMethods.Scintilla_DirectFunction directFunction;

        private IntPtr SciPointer
        {
            get
            {
                // Enforce illegal cross-thread calls the way the Handle property does
                if (Control.CheckForIllegalCrossThreadCalls && InvokeRequired)
                {
                    string message = string.Format(CultureInfo.InvariantCulture, "Control '{0}' accessed from a thread other than the thread it was created on.", Name);
                    throw new InvalidOperationException(message);
                }

                if (sciPtr == IntPtr.Zero)
                {
                    // Get a pointer to the native Scintilla object (i.e. C++ 'this') to use with the
                    // direct function. This will happen for each Scintilla control instance.
                    sciPtr = NativeMethods.SendMessage(new HandleRef(this, Handle), NativeMethods.SCI_GETDIRECTPOINTER, IntPtr.Zero, IntPtr.Zero);
                }

                return sciPtr;
            }
        }

        public ScintillaControl()
        {
            base.SetStyle(ControlStyles.UserPaint, false);
            init();
        }

        /// <summary>
        /// Gets the required creation parameters when the control handle is created.
        /// </summary>
        /// <returns>A CreateParams that contains the required creation parameters when the handle to the control is created.</returns>
        protected override CreateParams CreateParams
        {
            get
            {
                if (moduleHandle == IntPtr.Zero)
                {
                    var path = UnpackNativeScintilla(); // GetModulePath();
                    //var path = @"C:\Users\alain\src\Eto.Toolkit\artifacts\core\Debug\SciLexer.dll";
                    //path = @"C:\Users\alain\src\Eto.Toolkit\src\Eto.CodeEditor.Wpf\scintilla\x64\SciLexer.dll";

                    // Load the native Scintilla library
                    moduleHandle = NativeMethods.LoadLibrary(path);
                    if (moduleHandle == IntPtr.Zero)
                    {
                        var message = string.Format(CultureInfo.InvariantCulture, "Could not load the Scintilla module at the path '{0}'.", path);
                        throw new Win32Exception(message, new Win32Exception()); // Calls GetLastError
                    }

                    // Get the native Scintilla direct function -- the only function the library exports
                    var directFunctionPointer = NativeMethods.GetProcAddress(new HandleRef(this, moduleHandle), "Scintilla_DirectFunction");
                    if (directFunctionPointer == IntPtr.Zero)
                    {
                        var message = "The Scintilla module has no export for the 'Scintilla_DirectFunction' procedure.";
                        throw new Win32Exception(message, new Win32Exception()); // Calls GetLastError
                    }

                    // Create a managed callback
                    directFunction = (NativeMethods.Scintilla_DirectFunction)Marshal.GetDelegateForFunctionPointer(
                        directFunctionPointer,
                        typeof(NativeMethods.Scintilla_DirectFunction));
                }

                CreateParams cp = base.CreateParams;
                //cp.ClassName = "ScintillaControl";
                cp.ClassName = "Scintilla";

                // The border effect is achieved through a native Windows style
                cp.ExStyle &= (~NativeMethods.WS_EX_CLIENTEDGE);
                cp.Style &= (~NativeMethods.WS_BORDER);
                switch (borderStyle)
                {
                    case BorderStyle.Fixed3D:
                        cp.ExStyle |= NativeMethods.WS_EX_CLIENTEDGE;
                        break;
                    case BorderStyle.FixedSingle:
                        cp.Style |= NativeMethods.WS_BORDER;
                        break;
                }

                return cp;
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case (NativeMethods.WM_REFLECT + NativeMethods.WM_NOTIFY):
                    WmReflectNotify(ref m);
                    break;

                case NativeMethods.WM_SETCURSOR:
                    DefWndProc(ref m);
                    break;

                case NativeMethods.WM_LBUTTONDBLCLK:
                case NativeMethods.WM_RBUTTONDBLCLK:
                case NativeMethods.WM_MBUTTONDBLCLK:
                //case NativeMethods.WM_XBUTTONDBLCLK:
                //    doubleClick = true;
                //    goto default;

                //case NativeMethods.WM_DESTROY:
                //    WmDestroy(ref m);
                //    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private void WmReflectNotify(ref Message m)
        {
            // A standard Windows notification and a Scintilla notification header are compatible
            NativeMethods.SCNotification scn = (NativeMethods.SCNotification)Marshal.PtrToStructure(m.LParam, typeof(NativeMethods.SCNotification));
            if (scn.nmhdr.code >= NativeMethods.SCN_STYLENEEDED && scn.nmhdr.code <= NativeMethods.SCN_AUTOCCOMPLETED)
            {
                //var handler = Events[scNotificationEventKey] as EventHandler<SCNotificationEventArgs>;
                //if (handler != null)
                //    handler(this, new SCNotificationEventArgs(scn));

                switch (scn.nmhdr.code)
                {
                    //case NativeMethods.SCN_PAINTED:
                    //    OnPainted(EventArgs.Empty);
                    //    break;

                    case NativeMethods.SCN_MODIFIED:
                        //ScnModified(ref scn);
                        break;

                    //case NativeMethods.SCN_MODIFYATTEMPTRO:
                    //    OnModifyAttempt(EventArgs.Empty);
                    //    break;

                    //case NativeMethods.SCN_STYLENEEDED:
                    //    OnStyleNeeded(new StyleNeededEventArgs(this, scn.position));
                    //    break;

                    //case NativeMethods.SCN_SAVEPOINTLEFT:
                    //    OnSavePointLeft(EventArgs.Empty);
                    //    break;

                    //case NativeMethods.SCN_SAVEPOINTREACHED:
                    //    OnSavePointReached(EventArgs.Empty);
                    //    break;

                    case NativeMethods.SCN_MARGINCLICK:
                    case NativeMethods.SCN_MARGINRIGHTCLICK:
                        //ScnMarginClick(ref scn);
                        break;

                    //case NativeMethods.SCN_UPDATEUI:
                    //    OnUpdateUI(new UpdateUIEventArgs((UpdateChange)scn.updated));
                    //    break;

                    case NativeMethods.SCN_CHARADDED:
                        //OnCharAdded(new CharAddedEventArgs(scn.ch));
                        break;

                    //case NativeMethods.SCN_AUTOCSELECTION:
                    //    OnAutoCSelection(new AutoCSelectionEventArgs(this, scn.position, scn.text, scn.ch, (ListCompletionMethod)scn.listCompletionMethod));
                    //    break;

                    //case NativeMethods.SCN_AUTOCCOMPLETED:
                    //    OnAutoCCompleted(new AutoCSelectionEventArgs(this, scn.position, scn.text, scn.ch, (ListCompletionMethod)scn.listCompletionMethod));
                    //    break;

                    //case NativeMethods.SCN_AUTOCCANCELLED:
                    //    OnAutoCCancelled(EventArgs.Empty);
                    //    break;

                    //case NativeMethods.SCN_AUTOCCHARDELETED:
                    //    OnAutoCCharDeleted(EventArgs.Empty);
                    //    break;

                    //case NativeMethods.SCN_DWELLSTART:
                    //    OnDwellStart(new DwellEventArgs(this, scn.position, scn.x, scn.y));
                    //    break;

                    //case NativeMethods.SCN_DWELLEND:
                    //    OnDwellEnd(new DwellEventArgs(this, scn.position, scn.x, scn.y));
                    //    break;

                    //case NativeMethods.SCN_DOUBLECLICK:
                    //    ScnDoubleClick(ref scn);
                    //    break;

                    //case NativeMethods.SCN_NEEDSHOWN:
                    //    OnNeedShown(new NeedShownEventArgs(this, scn.position, scn.length));
                    //    break;

                    //case NativeMethods.SCN_HOTSPOTCLICK:
                    //case NativeMethods.SCN_HOTSPOTDOUBLECLICK:
                    //case NativeMethods.SCN_HOTSPOTRELEASECLICK:
                    //    ScnHotspotClick(ref scn);
                    //    break;

                    //case NativeMethods.SCN_INDICATORCLICK:
                    //case NativeMethods.SCN_INDICATORRELEASE:
                    //    ScnIndicatorClick(ref scn);
                    //    break;

                    //case NativeMethods.SCN_ZOOM:
                    //    OnZoomChanged(EventArgs.Empty);
                    //    break;

                    default:
                        // Not our notification
                        base.WndProc(ref m);
                        break;
                }
            }
        }

        /// <summary>
        /// SciLexer.dll is embedded in this assembly. Unpack it to a temp directory and
        /// return the path that to the unpacked library
        /// </summary>
        /// <returns></returns>
        public static string UnpackNativeScintilla()
        {
            const string scilexerVersion = "4.2.0";
            string bitness = IntPtr.Size == 4 ? "x86" : "x64";
            string path = Path.Combine(Path.GetTempPath(), "Eto.CodeEditor.Wpf", scilexerVersion, bitness, "SciLexer.dll");
            if (!File.Exists(path))
            {
                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                string resourceName = $"Eto.CodeEditor.Wpf.scintilla.{bitness}.SciLexer.dll";
                using (var resourceStream = typeof(ScintillaControl).Assembly.GetManifestResourceStream(resourceName))
                using (var fileStream = File.Create(path))
                {
                    resourceStream.CopyTo(fileStream);
                }
            }
            return path;
        }
    }
}
