using System;
using System.Text;
using Eto;
using Eto.CodeEditor;
using Eto.CodeEditor.XamMac2;
using AppKit;
using ScintillaNET;
using Foundation;
using System.IO;
using ObjCRuntime;
using System.Collections.Generic;
using Scintilla;

[assembly: ExportHandler(typeof(CodeEditor), typeof(CodeEditorHandler))]

namespace Eto.CodeEditor
{
    public partial class CodeEditorHandler : Eto.Mac.Forms.MacView<Scintilla.ScintillaControl, CodeEditor, CodeEditor.ICallback>, CodeEditor.IHandler, Eto.Mac.Forms.IMacControl
    {
        public WeakReference WeakHandler { get; set; }

        static CodeEditorHandler()
        {
            var path = Path.Combine(NSBundle.MainBundle.PrivateFrameworksPath, "Scintilla.framework", "Scintilla");
            Dlfcn.dlopen(path, 4);
        }

        private Scintilla.ScintillaControl scintilla;

        private EtoScintillaNotificationProtocol notificationProtocol;
        public CodeEditorHandler()
        {
            scintilla = new Scintilla.ScintillaControl();
            notificationProtocol = new EtoScintillaNotificationProtocol();
            notificationProtocol.Notify += NotificationProtocol_Notify;
            scintilla.WeakDelegate = notificationProtocol;
            Control = scintilla;

            FontName = "Menlo";
            FontSize = 14;
            LineNumberColumnWidth = 40;
            ShowIndentationGuides();
            Control.Message(NativeMethods.SCI_AUTOCSETMAXHEIGHT, new IntPtr(10), IntPtr.Zero);
        }

        public override NSView ContainerControl => Control;

        public override bool Enabled { get; set; }

        public void SetKeywords(int set, string keywords)
        {
            Control.SetKeywords(set, keywords);
        }

        unsafe void NotificationProtocol_Notify(object sender, SCNotifyEventArgs e)
        {
            var n = e.Notification;
            Control.HandleScintillaMessage((int)n.nmhdr.code, (char)n.ch, (int)n.position, n.margin);
        }

        Encoding Encoding
        {
            get
            {
                int codePage = (int)Control.Message(NativeMethods.SCI_GETCODEPAGE, IntPtr.Zero, IntPtr.Zero);
                return (codePage == 0) ? Encoding.Default : Encoding.GetEncoding(codePage);
            }
        }
    }
}
