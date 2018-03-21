using System;
using System.Runtime.InteropServices;
using ObjCRuntime;
//using Scintilla;

namespace ScintillaNET
{
	
static class CFunctions
{
  // extern int Scintilla_LinkLexers ();
  [DllImport ("__Internal")]
  //[Verify (PlatformInvoke)]
  static extern int Scintilla_LinkLexers ();
}

[StructLayout (LayoutKind.Sequential)]
public struct Sci_CharacterRange
{
  public nint cpMin;

  public nint cpMax;
}

[StructLayout (LayoutKind.Sequential)]
public struct Sci_TextRange
{
  public Sci_CharacterRange chrg;

  public unsafe sbyte* lpstrText;
}

[StructLayout (LayoutKind.Sequential)]
public struct Sci_TextToFind
{
  public Sci_CharacterRange chrg;

  public unsafe sbyte* lpstrText;

  public Sci_CharacterRange chrgText;
}

[StructLayout (LayoutKind.Sequential)]
public struct Sci_Rectangle
{
  public int left;

  public int top;

  public int right;

  public int bottom;
}

[StructLayout (LayoutKind.Sequential)]
public struct Sci_RangeToFormat
{
  public unsafe void* hdc;

  public unsafe void* hdcTarget;

  public Sci_Rectangle rc;

  public Sci_Rectangle rcPage;

  public Sci_CharacterRange chrg;
}

[StructLayout (LayoutKind.Sequential)]
public struct Sci_NotifyHeader
{
  public unsafe void* hwndFrom;

  public UIntPtr idFrom;

  public uint code;
}

[StructLayout (LayoutKind.Sequential)]
public struct SCNotification
{
  public Sci_NotifyHeader nmhdr;

  public int position;

  public int ch;

  public int modifiers;

  public int modificationType;

  public unsafe sbyte* text;

  public int length;

  public int linesAdded;

  public int message;

  public UIntPtr wParam;

  public IntPtr lParam;

  public int line;

  public int foldLevelNow;

  public int foldLevelPrev;

  public int margin;

  public int listType;

  public int x;

  public int y;

  public int token;

  public int annotationLinesAdded;

  public int updated;

  public int listCompletionMethod;
}

}