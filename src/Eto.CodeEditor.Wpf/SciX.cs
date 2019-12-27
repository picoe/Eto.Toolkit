using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScintillaNET;

namespace Scintilla
{
    public class SciX
    {
        //public delegate IntPtr Scintilla_DirectFunction(IntPtr ptr, int iMessage, IntPtr wParam, IntPtr lParam);

        public IntPtr ScintillaHandle;
        public NativeMethods.Scintilla_DirectFunction DirectFunction;

        public bool IsNotInitialized => ScintillaHandle == null || DirectFunction == null;

        //private Lazy<Scintilla_DirectFunction> lazy_direct_function; 
        //private Scintilla_DirectFunction direct_function => lazy_direct_function.Value;

        //private Lazy<IntPtr> lazy_scintilla_handle;
        //private IntPtr scinctilla_handle => lazy_scintilla_handle.Value;

        //public SciX(Lazy<Scintilla_DirectFunction> lazy_direct_function, Lazy<IntPtr> lazy_scintilla_handle)
        //{
        //    this.lazy_direct_function = lazy_direct_function;
        //    this.lazy_scintilla_handle = lazy_scintilla_handle;
        //}

        public unsafe string Text
        {
            get
            {
                var length = DirectMessage(NativeMethods.SCI_GETTEXTLENGTH).ToInt32();
                var ptr = DirectMessage(NativeMethods.SCI_GETRANGEPOINTER, new IntPtr(0), new IntPtr(length));
                if (ptr == IntPtr.Zero)
                    return string.Empty;

                // Assumption is that moving the gap will always be equal to or less expensive
                // than using one of the APIs which requires an intermediate buffer.
                var text = new string((sbyte*)ptr, 0, length, /*Encoding*/System.Text.Encoding.UTF8);
                return text;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    DirectMessage(NativeMethods.SCI_CLEARALL);
                }
                else
                {
                    fixed (byte* bp = /*Helpers.*/GetBytes(value, /*Encoding*/System.Text.Encoding.UTF8, zeroTerminated: true))
                        DirectMessage(NativeMethods.SCI_SETTEXT, IntPtr.Zero, new IntPtr(bp));
                }
            }
        }

        private unsafe byte[] GetBytes(string text, System.Text.Encoding encoding, bool zeroTerminated)
        {
            if (string.IsNullOrEmpty(text))
                return (zeroTerminated ? new byte[] { 0 } : new byte[0]);

            int count = encoding.GetByteCount(text);
            byte[] buffer = new byte[count + (zeroTerminated ? 1 : 0)];

            fixed (byte* bp = buffer)
            fixed (char* ch = text)
            {
                encoding.GetBytes(ch, text.Length, bp, count);
            }

            if (zeroTerminated)
                buffer[buffer.Length - 1] = 0;

            return buffer;
        }

        internal IntPtr DirectMessage(int msg)
        {
            return DirectMessage(msg, IntPtr.Zero, IntPtr.Zero);
        }

        internal IntPtr DirectMessage(int msg, IntPtr wParam)
        {
            return DirectMessage(msg, wParam, IntPtr.Zero);
        }

        public virtual IntPtr DirectMessage(int msg, IntPtr wParam, IntPtr lParam)
        {
            // If the control handle, ptr, direct function, etc... hasn't been created yet, it will be now.
            var result = DirectMessage(ScintillaHandle, msg, wParam, lParam);
            return result;
        }

        internal IntPtr DirectMessage(IntPtr sciPtr, int msg, IntPtr wParam, IntPtr lParam)
        {
            if (IsNotInitialized)
                throw new Exception("SciX is not initialized");
            // Like Win32 SendMessage but directly to Scintilla
            var result = DirectFunction(sciPtr, msg, wParam, lParam);
            return result;
        }
    }
}
