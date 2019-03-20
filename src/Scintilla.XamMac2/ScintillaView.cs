using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ScintillaNET
{
    public partial class ScintillaView
    {
        public void SetKeywords(int set, string keywords)
        {
            var bytes = ASCIIEncoding.ASCII.GetBytes(keywords.ToCharArray());
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            // retrieve a raw pointer to pass to the native code:
            IntPtr ptr = handle.AddrOfPinnedObject();

            SetReferenceProperty(ScintillaNET.NativeMethods.SCI_SETKEYWORDS, set, ptr);
            // later, possibly in some other method:
            handle.Free();
        }
    }
}