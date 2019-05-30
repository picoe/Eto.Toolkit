using System;
using System.Runtime.InteropServices;
using System.Text;
using ScintillaNET;

namespace Eto.CodeEditor.XamMac2
{
    class EtoScintillaNotificationProtocol : ScintillaNotificationProtocol
    {
        public event EventHandler<SCNotifyEventArgs> Notify;

        public override void Notification(IntPtr notification)
        {
            var scnotify = Marshal.PtrToStructure<SCNotification>(notification);
            Notify?.Invoke(this, new SCNotifyEventArgs(scnotify));
        }
    }
}