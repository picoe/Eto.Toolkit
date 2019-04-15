using System;
using System.Runtime.InteropServices;
using ObjCRuntime;
//using Scintilla;

namespace ScintillaNET
{

    static class CFunctions
    {
        // extern int Scintilla_LinkLexers ();
        [DllImport("__Internal")]
        //[Verify (PlatformInvoke)]
        static extern int Scintilla_LinkLexers();
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Sci_CharacterRange
    {
        public nint cpMin;

        public nint cpMax;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Sci_TextRange
    {
        public Sci_CharacterRange chrg;

        public unsafe sbyte* lpstrText;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Sci_TextToFind
    {
        public Sci_CharacterRange chrg;

        public unsafe sbyte* lpstrText;

        public Sci_CharacterRange chrgText;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Sci_Rectangle
    {
        public int left;

        public int top;

        public int right;

        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Sci_RangeToFormat
    {
        public unsafe void* hdc;

        public unsafe void* hdcTarget;

        public Sci_Rectangle rc;

        public Sci_Rectangle rcPage;

        public Sci_CharacterRange chrg;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Sci_NotifyHeader
    {
        public unsafe void* hwndFrom;

        public UIntPtr idFrom;

        //public ulong code;
        public uint code;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SCNotification
    {
        public Sci_NotifyHeader nmhdr;

        public long position;

        public int ch;

        public int modifiers;

        public int modificationType;

        //public unsafe sbyte* text;
        public IntPtr text;

        public long length;

        public long linesAdded;

        public int message;

        public UIntPtr wParam;

        public IntPtr lParam;

        public long line;

        public int foldLevelNow;

        public int foldLevelPrev;

        public int margin;

        public int listType;

        public int x;

        public int y;

        public int token;

        public long annotationLinesAdded;

        public int updated;

        public int listCompletionMethod;
    }
    //{
    //    public Sci_NotifyHeader nmhdr;
    //
    //    public long position;
    //
    //    public long ch;
    //
    //    public long modifiers;
    //
    //    public long modificationType;
    //
    //    public unsafe sbyte* text;
    //
    //    public long length;
    //
    //    public long linesAdded;
    //
    //    public long message;
    //
    //    public UIntPtr wParam;
    //
    //    public IntPtr lParam;
    //
    //    public long line;
    //
    //    public long foldLevelNow;
    //
    //    public long foldLevelPrev;
    //
    //    public long margin;
    //
    //    public long listType;
    //
    //    public long x;
    //
    //    public long y;
    //
    //    public long token;
    //
    //    public long annotationLinesAdded;
    //
    //    public long updated;
    //
    //    public long listCompletionMethod;
    //}
}
