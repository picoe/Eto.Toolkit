using System;
using ScintillaNET;

namespace Eto.CodeEditor.XamMac2
{
    public class SCNotifyEventArgs : EventArgs
    {
        private SCNotification notification;

        public SCNotifyEventArgs(SCNotification notification)
        {
            this.notification = notification;
        }

        public SCNotification Notification => notification;
    }
}
