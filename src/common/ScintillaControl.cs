using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScintillaNET;
using Eto.CodeEditor;
using System.Drawing;

namespace Scintilla
{
    public partial class ScintillaControl //: CodeEditor.IHandler
    {
        public NativeMethods.Scintilla_DirectFunction directFunction;

        public unsafe void SetKeywords(int set, string keywords)
        {
            //scintilla.SetKeywords(set, keywords);
            set = Helpers.Clamp(set, 0, NativeMethods.KEYWORDSET_MAX);
            var bytes = Helpers.GetBytes(keywords ?? string.Empty, Encoding.ASCII, zeroTerminated: true);

            fixed (byte* bp = bytes)
                DirectMessage(NativeMethods.SCI_SETKEYWORDS, new IntPtr(set), new IntPtr(bp));
        }

        ProgrammingLanguage _language = ProgrammingLanguage.None;
        public ProgrammingLanguage Language
        {
            get { return _language; }
            set
            {
                _language = value;
                int which = ScintillaNET.NativeMethods.SCLEX_CPP;
                switch (_language)
                {
                    case ProgrammingLanguage.CSharp:
                    case ProgrammingLanguage.GLSL:
                        which = ScintillaNET.NativeMethods.SCLEX_CPP;
                        break;
                    case ProgrammingLanguage.VB:
                        which = ScintillaNET.NativeMethods.SCLEX_VB;
                        break;
                    case ProgrammingLanguage.Python:
                        which = ScintillaNET.NativeMethods.SCLEX_PYTHON;
                        break;
                }
                DirectMessage(ScintillaNET.NativeMethods.SCI_SETLEXER, new IntPtr(which));
            }
        }
        #region IHandler impl
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
                    fixed (byte* bp = Helpers.GetBytes(value, /*Encoding*/System.Text.Encoding.UTF8, zeroTerminated: true))
                        DirectMessage(NativeMethods.SCI_SETTEXT, IntPtr.Zero, new IntPtr(bp));
                }
            }
        }

        public void SetProgrammingLanguage(ProgrammingLanguage language, string[] keywordSets)
        {
            Language = language;
            if(keywordSets!=null)
            {
                for( int i=0; i<keywordSets.Length; i++ )
                {
                    SetKeywords(i, keywordSets[i]);
                }
            }
        }

        public string FontName
        {
            get
            {
                var length = DirectMessage(NativeMethods.SCI_STYLEGETFONT, new IntPtr(ScintillaNET.NativeMethods.STYLE_DEFAULT), IntPtr.Zero).ToInt32();
                var font = new byte[length];
                unsafe
                {
                    fixed (byte* bp = font)
                        DirectMessage(NativeMethods.SCI_STYLEGETFONT, new IntPtr(ScintillaNET.NativeMethods.STYLE_DEFAULT), new IntPtr(bp));
                }

                var name = Encoding.UTF8.GetString(font, 0, length);
                return name;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    value = "Verdana";

                // Scintilla expects UTF-8
                var font = Helpers.GetBytes(value, Encoding.UTF8, true);
                unsafe
                {
                    fixed (byte* bp = font)
                        DirectMessage(NativeMethods.SCI_STYLESETFONT, new IntPtr(ScintillaNET.NativeMethods.STYLE_DEFAULT), new IntPtr(bp));
                    DirectMessage(NativeMethods.SCI_STYLECLEARALL, IntPtr.Zero, IntPtr.Zero);
                }
            }
        }

        public void SetColor(Section section, Eto.Drawing.Color foreground, Eto.Drawing.Color background)
        {
            int fg = (foreground.Bb << 16) + (foreground.Gb << 8) + foreground.Rb;
            int bg = (background.Bb << 16) + (background.Gb << 8) + background.Rb;

            if (section == Section.Default)
            {
                DirectMessage(NativeMethods.SCI_STYLESETFORE, new IntPtr(NativeMethods.STYLE_DEFAULT), new IntPtr(fg));
                int argb = foreground.ToArgb();
                DirectMessage(NativeMethods.SCI_SETCARETFORE, new IntPtr(argb), new IntPtr(0));
                DirectMessage(NativeMethods.SCI_STYLESETBACK, new IntPtr(NativeMethods.STYLE_DEFAULT), new IntPtr(bg));
                DirectMessage(NativeMethods.SCI_STYLECLEARALL, new IntPtr(0), new IntPtr(0));
            }
            if (section == Section.Comment)
            {
                foreach (var id in CommentStyleIds(Language))
                {
                    DirectMessage(NativeMethods.SCI_STYLESETFORE, new IntPtr(id), new IntPtr(fg));
                    DirectMessage(NativeMethods.SCI_STYLESETBACK, new IntPtr(id), new IntPtr(bg));
                }
            }
            if (section == Section.Keyword1)
            {
                foreach (var id in Keyword1Ids(Language))
                {
                    DirectMessage(NativeMethods.SCI_STYLESETFORE, new IntPtr(id), new IntPtr(fg));
                    DirectMessage(NativeMethods.SCI_STYLESETBACK, new IntPtr(id), new IntPtr(bg));
                }
            }
            if (section == Section.Keyword2)
            {
                foreach (var id in Keyword2Ids(Language))
                {
                    DirectMessage(NativeMethods.SCI_STYLESETFORE, new IntPtr(id), new IntPtr(fg));
                    DirectMessage(NativeMethods.SCI_STYLESETBACK, new IntPtr(id), new IntPtr(bg));
                }
            }
            if (section == Section.Strings)
            {
                foreach (var id in StringStyleIds(Language))
                {
                    DirectMessage(NativeMethods.SCI_STYLESETFORE, new IntPtr(id), new IntPtr(fg));
                    DirectMessage(NativeMethods.SCI_STYLESETBACK, new IntPtr(id), new IntPtr(bg));
                }
            }
            if (section == Section.LineNumber)
            {
                DirectMessage(NativeMethods.SCI_STYLESETFORE, new IntPtr(NativeMethods.STYLE_LINENUMBER), new IntPtr(fg));
                DirectMessage(NativeMethods.SCI_STYLESETBACK, new IntPtr(NativeMethods.STYLE_LINENUMBER), new IntPtr(bg));
            }
            if (section == Section.DefName && Language == ProgrammingLanguage.Python)
            {
                DirectMessage(NativeMethods.SCI_STYLESETFORE, new IntPtr(NativeMethods.SCE_P_DEFNAME), new IntPtr(fg));
                DirectMessage(NativeMethods.SCI_STYLESETBACK, new IntPtr(NativeMethods.SCE_P_DEFNAME), new IntPtr(bg));
            }
            if (section == Section.Preprocessor)
            {
                foreach (var id in PreprocessorIds(Language))
                {
                    DirectMessage(NativeMethods.SCI_STYLESETFORE, new IntPtr(id), new IntPtr(fg));
                    DirectMessage(NativeMethods.SCI_STYLESETBACK, new IntPtr(id), new IntPtr(bg));
                }

            }
        }

        public int CurrentPosition
        {
            get => DirectMessage(NativeMethods.SCI_GETCURRENTPOS).ToInt32();
            set => DirectMessage(NativeMethods.SCI_SETCURRENTPOS, new IntPtr(value));
        }

        public int CurrentPositionInLine => CurrentPosition - DirectMessage(NativeMethods.SCI_POSITIONFROMLINE, new IntPtr(CurrentPosition)).ToInt32();

        public int CurrentLineNumber => DirectMessage(NativeMethods.SCI_LINEFROMPOSITION, new IntPtr(CurrentPosition)).ToInt32();

        public string GetLineText(int lineNumber)
        {
            //var line = new Line(scintilla, lineNumber);
            //return line?.Text ?? "";
            var start = DirectMessage(NativeMethods.SCI_POSITIONFROMLINE, new IntPtr(lineNumber));
            var length = DirectMessage(NativeMethods.SCI_LINELENGTH, new IntPtr(lineNumber));
            var ptr = DirectMessage(NativeMethods.SCI_GETRANGEPOINTER, start, length);
            if (ptr == IntPtr.Zero)
                return string.Empty;
            var text = Helpers.GetString(ptr, length.ToInt32(), Encoding.UTF8); // new string((sbyte*)ptr, 0, length.ToInt32(), scintilla.Encoding);
            return text;
        }

        public unsafe void InsertText(int position, string text) 
        { 
            //scintilla.InsertText(position, text); 
            if (position < -1)
                throw new ArgumentOutOfRangeException(nameof(position), "Position must be greater or equal to -1");
            if (position != -1)
            {
                int textLength = DirectMessage(NativeMethods.SCI_GETLENGTH, IntPtr.Zero, IntPtr.Zero).ToInt32();
                if (position > textLength)
                    throw new ArgumentOutOfRangeException(nameof(position), "Position cannot exceed document length");
            }

            fixed (byte* bp = Helpers.GetBytes(text ?? string.Empty, Encoding.UTF8, zeroTerminated: true))
                DirectMessage(NativeMethods.SCI_INSERTTEXT, new IntPtr(position), new IntPtr(bp));
        }

        public unsafe int ReplaceTarget(string text, int start, int end)
        {
            //scintilla.SetTargetRange(start, end);
            //return scintilla.ReplaceTarget(text);

            SetTargetRange(start, end);
            if (text == null)
                text = string.Empty;

            var bytes = Helpers.GetBytes(text, Encoding.UTF8, false);
            fixed (byte* bp = bytes)
                DirectMessage(NativeMethods.SCI_REPLACETARGET, new IntPtr(bytes.Length), new IntPtr(bp));

            return text.Length;
        }

        public unsafe void ReplaceFirstOccuranceInLine(string oldText, string newText, int lineNumber)
        {
            //var line = scintilla.Lines[lineNumber];
            //scintilla.SetTargetRange(line.Position, line.EndPosition);

            //var pos = scintilla.SearchInTarget(oldText);
            //if (pos == -1)
            //  return;

            //scintilla.SetTargetRange(pos, pos + oldText.Length);

            //scintilla.ReplaceTarget(newText);

            var lineStartPos = DirectMessage(NativeMethods.SCI_POSITIONFROMLINE, new IntPtr(CurrentLineNumber));
            var lineEndPos = DirectMessage(NativeMethods.SCI_GETLINEENDPOSITION, new IntPtr(CurrentLineNumber));
            DirectMessage(NativeMethods.SCI_SETTARGETRANGE, lineStartPos, lineEndPos);

            int bytePos = 0;
            var bytes = Helpers.GetBytes(oldText ?? string.Empty, Encoding.UTF8, zeroTerminated: false);
            fixed (byte* bp = bytes)
                bytePos = DirectMessage(NativeMethods.SCI_SEARCHINTARGET, new IntPtr(bytes.Length), new IntPtr(bp)).ToInt32();

            if (bytePos == -1)
                return;

            DirectMessage(NativeMethods.SCI_SETTARGETRANGE, new IntPtr(bytePos), new IntPtr(bytePos + bytes.Length));

            bytes = Helpers.GetBytes(newText ?? string.Empty, Encoding.UTF8, zeroTerminated:false);
            fixed (byte* bp = bytes)
                DirectMessage(NativeMethods.SCI_REPLACETARGET, new IntPtr(bytes.Length), new IntPtr(bp));
        }

        public void DeleteRange(int position, int length) 
        { 
            //scintilla.DeleteRange(position, length); 
            var textLength = DirectMessage(NativeMethods.SCI_GETLENGTH).ToInt32();
            position = Helpers.Clamp(position, 0, textLength);
            length = Helpers.Clamp(length, 0, textLength - position);

            DirectMessage(NativeMethods.SCI_DELETERANGE, new IntPtr(position), new IntPtr(length));
        }

        public int WordStartPosition(int position, bool onlyWordCharacters)
        {
            //return scintilla.WordStartPosition(position, onlyWordCharacters);
            var onlyWordChars = (onlyWordCharacters ? new IntPtr(1) : IntPtr.Zero);
            int textLength = DirectMessage(NativeMethods.SCI_GETLENGTH, IntPtr.Zero, IntPtr.Zero).ToInt32();
            position = Helpers.Clamp(position, 0, textLength);
            position = DirectMessage(NativeMethods.SCI_WORDSTARTPOSITION, new IntPtr(position), onlyWordChars).ToInt32();
            return position;
        }

        public unsafe void AutoCompleteShow(int lenEntered, string list)
        {
            //scintilla.AutoCShow(lenEntered, list);
            if (string.IsNullOrEmpty(list))
                return;
            lenEntered = Helpers.ClampMin(lenEntered, 0);
            if( lenEntered > 0 )
            {
                int endPos = DirectMessage(NativeMethods.SCI_GETCURRENTPOS, IntPtr.Zero, IntPtr.Zero).ToInt32();
                int startPos = endPos;
                for (int i = 0; i < lenEntered; i++)
                    startPos = DirectMessage(NativeMethods.SCI_POSITIONRELATIVE, new IntPtr(startPos), new IntPtr(-1)).ToInt32();
                lenEntered = (endPos - startPos);
            }

            var bytes = Helpers.GetBytes(list, Encoding.UTF8, zeroTerminated: true);
            fixed (byte* bp = bytes)
                DirectMessage(NativeMethods.SCI_AUTOCSHOW, new IntPtr(lenEntered), new IntPtr(bp));
            // if the following property is not set, items after 'import' that start with an uppercase
            // closes the completion window. Ex: 'import R' closes the window even though 'Rhino' is
            // in the list.
            DirectMessage(NativeMethods.SCI_AUTOCSETIGNORECASE, new IntPtr(1), IntPtr.Zero);
        }
        #endregion
        
        private void SetTargetRange(int start, int end)
        {
            var textLength = DirectMessage(NativeMethods.SCI_GETLENGTH, IntPtr.Zero, IntPtr.Zero).ToInt32();
            start = Helpers.Clamp(start, 0, textLength);
            end = Helpers.Clamp(end, 0, textLength);

            DirectMessage(NativeMethods.SCI_SETTARGETRANGE, new IntPtr(start), new IntPtr(end));
        }

        static int[] CommentStyleIds(ProgrammingLanguage language)
        {
            //if (language == ProgrammingLanguage.Python)
            //    return new int[] { ScintillaNET.Style.Python.CommentBlock, ScintillaNET.Style.Python.CommentLine};

            //if (language == ProgrammingLanguage.VB)
            //    return new int[] { ScintillaNET.Style.Vb.Comment, ScintillaNET.Style.Vb.CommentBlock,
            //        ScintillaNET.Style.Vb.DocBlock, ScintillaNET.Style.Vb.Preprocessor};

            //return new int[] {ScintillaNET.Style.Cpp.Comment, ScintillaNET.Style.Cpp.CommentLine,
            //    ScintillaNET.Style.Cpp.CommentDoc, ScintillaNET.Style.Cpp.CommentLineDoc};
            if (language == ProgrammingLanguage.Python)
                return new int[] { NativeMethods.SCE_P_COMMENTBLOCK, NativeMethods.SCE_P_COMMENTLINE };

            if (language == ProgrammingLanguage.VB)
                return new int[] { NativeMethods.SCE_B_COMMENT, NativeMethods.SCE_B_COMMENTBLOCK,
                    NativeMethods.SCE_B_DOCBLOCK};

            return new int[] { NativeMethods.SCE_C_COMMENT, NativeMethods.SCE_C_COMMENTLINE,
                NativeMethods.SCE_C_COMMENTDOC, NativeMethods.SCE_C_COMMENTLINEDOC };
        }

        static int[] StringStyleIds(ProgrammingLanguage language)
        {
            //if (language == ProgrammingLanguage.Python)
            //    return new int[] { ScintillaNET.Style.Python.Character, ScintillaNET.Style.Python.String,
            //        ScintillaNET.Style.Python.Triple, ScintillaNET.Style.Python.TripleDouble };

            //if (language == ProgrammingLanguage.VB)
            //    return new int[] { ScintillaNET.Style.Vb.String };
            //return new int[] {ScintillaNET.Style.Cpp.String, ScintillaNET.Style.Cpp.Character};

            if (language == ProgrammingLanguage.Python)
                return new int[] { NativeMethods.SCE_P_CHARACTER, NativeMethods.SCE_P_STRING,
                    NativeMethods.SCE_P_TRIPLE, NativeMethods.SCE_P_TRIPLEDOUBLE };

            if (language == ProgrammingLanguage.VB)
                return new int[] { NativeMethods.SCE_B_STRING };
            return new int[] { NativeMethods.SCE_C_STRING, NativeMethods.SCE_C_CHARACTER };
        }

        static int[] Keyword1Ids(ProgrammingLanguage language)
        {
            //if (language == ProgrammingLanguage.Python)
            //    return new int[] { ScintillaNET.Style.Python.Word };

            //if (language == ProgrammingLanguage.VB)
            //    return new int[] { ScintillaNET.Style.Vb.Keyword };

            //return new int[] { ScintillaNET.Style.Cpp.Word };

            if (language == ProgrammingLanguage.Python)
                return new int[] { NativeMethods.SCE_P_WORD };

            if (language == ProgrammingLanguage.VB)
                return new int[] { NativeMethods.SCE_B_KEYWORD };

            return new int[] { NativeMethods.SCE_C_WORD };
        }

        static int[] Keyword2Ids(ProgrammingLanguage language)
        {
            //if (language == ProgrammingLanguage.Python)
            //    return new int[] { ScintillaNET.Style.Python.Word2 };

            //if (language == ProgrammingLanguage.VB)
            //    return new int[] { ScintillaNET.Style.Vb.Keyword2, ScintillaNET.Style.Vb.Keyword3, ScintillaNET.Style.Vb.Keyword4 };

            //return new int[] { ScintillaNET.Style.Cpp.Word2 };

            if (language == ProgrammingLanguage.Python)
                return new int[] { NativeMethods.SCE_P_WORD2 };

            if (language == ProgrammingLanguage.VB)
                return new int[] { NativeMethods.SCE_B_KEYWORD2, NativeMethods.SCE_B_KEYWORD3, NativeMethods.SCE_B_KEYWORD4 };

            return new int[] { NativeMethods.SCE_C_WORD2 };
        }

        static int[] PreprocessorIds(ProgrammingLanguage language)
        {
            if (language == ProgrammingLanguage.Python)
                return new int[] { };

            if (language == ProgrammingLanguage.VB)
                return new int[] { /*ScintillaNET.Style.Vb.Preprocessor*/NativeMethods.SCE_B_PREPROCESSOR };

            return new int[] { /*ScintillaNET.Style.Cpp.Preprocessor*/NativeMethods.SCE_C_PREPROCESSOR };
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
            var result = DirectMessage(SciPointer, msg, wParam, lParam);
            return result;
        }

        internal IntPtr DirectMessage(IntPtr sciPtr, int msg, IntPtr wParam, IntPtr lParam)
        {
            // Like Win32 SendMessage but directly to Scintilla
            var result = directFunction(sciPtr, msg, wParam, lParam);
            return result;
        }
    }
}
