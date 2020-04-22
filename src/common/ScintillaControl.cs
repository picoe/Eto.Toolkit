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
        private const int BREAKPOINT_MARKER = 3; // arbitrary number
        private const int BREAK_MARKER = 4; // arbitrary number

        private const int BREAKPOINTS_MARGIN = 1;
        private const int LINENUMBERS_MARGIN = 2;

        public NativeMethods.Scintilla_DirectFunction directFunction;

        private void init()
        {
            // breakpoints margin
            DirectMessage(NativeMethods.SCI_SETMARGINSENSITIVEN, BREAKPOINTS_MARGIN, 1);
            DirectMessage(NativeMethods.SCI_SETMARGINTYPEN, BREAKPOINTS_MARGIN, NativeMethods.SC_MARGIN_SYMBOL);
            DirectMessage(NativeMethods.SCI_SETMARGINMASKN, BREAKPOINTS_MARGIN, int.MaxValue); // ScintillaNet -> public const uint MaskAll = unchecked((uint)-1);
            DirectMessage(NativeMethods.SCI_MARKERDEFINE, BREAKPOINTS_MARGIN, NativeMethods.SC_MARK_FULLRECT);
            IsBreakpointsMarginVisible = false;

            // line numbers margin
            DirectMessage(NativeMethods.SCI_SETMARGINSENSITIVEN, LINENUMBERS_MARGIN, 0);
            DirectMessage(NativeMethods.SCI_SETMARGINTYPEN, LINENUMBERS_MARGIN, NativeMethods.SC_MARGIN_NUMBER);

            // breakpoint marker
            DirectMessage(NativeMethods.SCI_MARKERDEFINE, BREAKPOINT_MARKER, NativeMethods.SC_MARK_CIRCLE); // default
            var red = 255; // 0xFF0000; // */ 16711680;
            DirectMessage(NativeMethods.SCI_MARKERSETFORE, BREAKPOINT_MARKER, red);
            DirectMessage(NativeMethods.SCI_MARKERSETBACK, BREAKPOINT_MARKER, red);

            // break marker
            DirectMessage(NativeMethods.SCI_MARKERDEFINE, BREAK_MARKER, NativeMethods.SC_MARK_ARROW);
            var yellow = 0x00FFFF; // */ 16776960;
            DirectMessage(NativeMethods.SCI_MARKERSETFORE, BREAK_MARKER, 0xFFFFFF); //black
            DirectMessage(NativeMethods.SCI_MARKERSETBACK, BREAK_MARKER, yellow);

            // use spaces for indentation by default. Auto indent doesn't work well at the moment
            ReplaceTabsWithSpaces = true;
        }

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
        public unsafe override string Text
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

        public int FontSize
        {
            get
            {
                return DirectMessage(NativeMethods.SCI_STYLEGETSIZE, new IntPtr(ScintillaNET.NativeMethods.STYLE_DEFAULT), IntPtr.Zero).ToInt32();
            }
            set
            {
                DirectMessage(NativeMethods.SCI_STYLESETSIZE, new IntPtr(ScintillaNET.NativeMethods.STYLE_DEFAULT), new IntPtr(value));
            }
        }

        public float FontSizeFractional
        {
            get
            {
                var fraction = DirectMessage(NativeMethods.SCI_STYLEGETSIZEFRACTIONAL, new IntPtr(ScintillaNET.NativeMethods.STYLE_DEFAULT), IntPtr.Zero).ToInt32();
                return (float)fraction / NativeMethods.SC_FONT_SIZE_MULTIPLIER;
            }
            set
            {
                var fraction = (int)(value * NativeMethods.SC_FONT_SIZE_MULTIPLIER);
                DirectMessage(NativeMethods.SCI_STYLESETSIZEFRACTIONAL, new IntPtr(ScintillaNET.NativeMethods.STYLE_DEFAULT), new IntPtr(fraction));
            }
        }

        public bool Bold
        {
            get
            {
                return DirectMessage(NativeMethods.SCI_STYLEGETBOLD, new IntPtr(ScintillaNET.NativeMethods.STYLE_DEFAULT), IntPtr.Zero) != IntPtr.Zero;
            }
            set
            {
                var bold = (value ? new IntPtr(1) : IntPtr.Zero);
                DirectMessage(NativeMethods.SCI_STYLESETBOLD, new IntPtr(ScintillaNET.NativeMethods.STYLE_DEFAULT), bold);
            }
        }

        public bool Italic
        {
            get
            {
                return DirectMessage(NativeMethods.SCI_STYLEGETITALIC, new IntPtr(ScintillaNET.NativeMethods.STYLE_DEFAULT), IntPtr.Zero) != IntPtr.Zero;
            }
            set
            {
                var italic = (value ? new IntPtr(1) : IntPtr.Zero);
                DirectMessage(NativeMethods.SCI_STYLESETITALIC, new IntPtr(ScintillaNET.NativeMethods.STYLE_DEFAULT), italic);
            }
        }

        public int TabWidth
        {
            get => DirectMessage(NativeMethods.SCI_GETTABWIDTH).ToInt32();
            set => DirectMessage(NativeMethods.SCI_SETTABWIDTH, new IntPtr(value));
        }

        public bool ReplaceTabsWithSpaces
        {
            get => DirectMessage(NativeMethods.SCI_GETUSETABS) == IntPtr.Zero;
            set
            {
                var useTabs = (value ? IntPtr.Zero : new IntPtr(1));
                DirectMessage(NativeMethods.SCI_SETUSETABS, useTabs);
            }
        }

        public bool BackspaceUnindents
        {
          get => false;
          set
            {
                //pass
            }
        }

        public int LineNumberColumnWidth
        {
            get
            {
                return DirectMessage(NativeMethods.SCI_GETMARGINWIDTHN, new IntPtr(LINENUMBERS_MARGIN), IntPtr.Zero).ToInt32();
            }
            set
            {
                //scintilla.Margins[0].Width = value;
                DirectMessage(NativeMethods.SCI_SETMARGINWIDTHN, new IntPtr(LINENUMBERS_MARGIN), new IntPtr(value));
            }
        }

        private int MarkerNext(int lineNumber) => 
            DirectMessage(NativeMethods.SCI_MARKERNEXT, new IntPtr(lineNumber), new IntPtr(1 << BREAKPOINT_MARKER)).ToInt32();

        public IEnumerable<int> Breakpoints
        {
            get
            {
                int lineIndex = MarkerNext(0);
                while (lineIndex != -1)
                {
                    // increment lineIndex before returning it because line numbers start at 1 on the client
                    lineIndex++;
                    yield return lineIndex;
                    // start searching on the next (incremented) index
                    lineIndex = MarkerNext(lineIndex);
                }
            }
        }

        public bool IsBreakpointsMarginVisible
        {
            get
            {
                var i = (DirectMessage(NativeMethods.SCI_GETMARGINWIDTHN, new IntPtr(BREAKPOINTS_MARGIN), IntPtr.Zero)).ToInt32();
                return i != 0;
            }
            set
            {
                DirectMessage(2242, new IntPtr(BREAKPOINTS_MARGIN), value ? new IntPtr(16) : IntPtr.Zero);
            }
        }

        public void BreakOnLine(int lineNumber)
        {
            ClearBreak();
            DirectMessage(NativeMethods.SCI_MARKERADD, new IntPtr(lineNumber), new IntPtr(BREAK_MARKER));
        }

        public void ClearBreak() => DirectMessage(NativeMethods.SCI_MARKERDELETEALL, new IntPtr(BREAK_MARKER), IntPtr.Zero);

        public void ClearBreakpoints()
        {
            //Control.SetGeneralProperty(NativeMethods.SCI_MARKERDELETEALL, BREAKPOINT_MARKER);
            DirectMessage(NativeMethods.SCI_MARKERDELETEALL, new IntPtr(BREAKPOINT_MARKER), IntPtr.Zero);
            BreakpointsChanged?.Invoke(this, new BreakpointsChangedEventArgs(BreakpointChangeType.Clear));
        }

        public event EventHandler<CharAddedEventArgs> CharAdded;
        public new event EventHandler<EventArgs> TextChanged; // hides inherited TextChanged
        public event EventHandler<BreakpointsChangedEventArgs> BreakpointsChanged;

        public void SetColor(Section section, Eto.Drawing.Color foreground, Eto.Drawing.Color background)
        {
            if (section == Section.Default)
            {
                DirectMessage(NativeMethods.SCI_STYLESETFORE, NativeMethods.STYLE_DEFAULT, foreground);
                int argb = foreground.ToArgb();
                DirectMessage(NativeMethods.SCI_SETCARETFORE, new IntPtr(argb), new IntPtr(0));
                DirectMessage(NativeMethods.SCI_STYLESETBACK, NativeMethods.STYLE_DEFAULT, background);
                DirectMessage(NativeMethods.SCI_STYLECLEARALL, new IntPtr(0), new IntPtr(0));
            }
            if (section == Section.Comment)
            {
                foreach (var id in CommentStyleIds(Language))
                {
                    DirectMessage(NativeMethods.SCI_STYLESETFORE, id, foreground);
                    DirectMessage(NativeMethods.SCI_STYLESETBACK, id, background);
                }
            }
            if (section == Section.Keyword1)
            {
                foreach (var id in Keyword1Ids(Language))
                {
                    DirectMessage(NativeMethods.SCI_STYLESETFORE, id, foreground);
                    DirectMessage(NativeMethods.SCI_STYLESETBACK, id, background);
                }
            }
            if (section == Section.Keyword2)
            {
                foreach (var id in Keyword2Ids(Language))
                {
                    DirectMessage(NativeMethods.SCI_STYLESETFORE, id, foreground);
                    DirectMessage(NativeMethods.SCI_STYLESETBACK, id, background);
                }
            }
            if (section == Section.Strings)
            {
                foreach (var id in StringStyleIds(Language))
                {
                    DirectMessage(NativeMethods.SCI_STYLESETFORE, id, foreground);
                    DirectMessage(NativeMethods.SCI_STYLESETBACK, id, background);
                }
            }
            if (section == Section.LineNumber)
            {
                DirectMessage(NativeMethods.SCI_STYLESETFORE, NativeMethods.STYLE_LINENUMBER, foreground);
                DirectMessage(NativeMethods.SCI_STYLESETBACK, NativeMethods.STYLE_LINENUMBER, background);
            }
            if (section == Section.DefName && Language == ProgrammingLanguage.Python)
            {
                DirectMessage(NativeMethods.SCI_STYLESETFORE, NativeMethods.SCE_P_DEFNAME, foreground);
                DirectMessage(NativeMethods.SCI_STYLESETBACK, NativeMethods.SCE_P_DEFNAME, background);
            }
            if (section == Section.Preprocessor)
            {
                foreach (var id in PreprocessorIds(Language))
                {
                    DirectMessage(NativeMethods.SCI_STYLESETFORE, id, foreground);
                    DirectMessage(NativeMethods.SCI_STYLESETBACK, id, background);
                }

            }
        }

        public int CurrentPosition
        {
            get => DirectMessage(NativeMethods.SCI_GETCURRENTPOS).ToInt32();
            set => DirectMessage(NativeMethods./*SCI_SETCURRENTPOS*/SCI_GOTOPOS, new IntPtr(value));
        }

        public int CurrentPositionInLine => CurrentPosition - DirectMessage(NativeMethods.SCI_POSITIONFROMLINE, new IntPtr(CurrentPosition)).ToInt32();

        public int CurrentLineNumber => DirectMessage(NativeMethods.SCI_LINEFROMPOSITION, new IntPtr(CurrentPosition)).ToInt32();

        public string WordAtCurrentPosition
        {
            get
            {
                var onlyWordChars = new IntPtr(1);
                var currentPosition = DirectMessage(NativeMethods.SCI_GETCURRENTPOS);
                var wordStartPos = DirectMessage(NativeMethods.SCI_WORDSTARTPOSITION, currentPosition, onlyWordChars).ToInt32();
                var wordEndPos = DirectMessage(NativeMethods.SCI_WORDENDPOSITION, currentPosition, onlyWordChars).ToInt32();

                var ptr = DirectMessage(NativeMethods.SCI_GETRANGEPOINTER, wordStartPos, wordEndPos - wordStartPos);

                return ptr != IntPtr.Zero ? Helpers.GetString(ptr, wordEndPos - wordStartPos) : string.Empty;
            }
        }

        public int GetLineIndentation(int lineNumber)
        {
            //var line = new Line(scintilla, lineNumber);
            //return line?.Indentation ?? 0;
            return DirectMessage(NativeMethods.SCI_GETLINEINDENTATION, new IntPtr(lineNumber)).ToInt32();
        }

        public void SetLineIndentation(int lineNumber, int indentation)
        {
            //var line = new Line(scintilla, lineNumber);
            //if (line != null)
            //{
            //    line.Indentation = indentation;
            //    scintilla.GotoPosition(line.Position + indentation);
            //}
            DirectMessage(NativeMethods.SCI_SETLINEINDENTATION, new IntPtr(lineNumber), new IntPtr(indentation));
        }

        public char GetLineLastChar(int lineNumber)
        {
            //var line = new Line(scintilla, lineNumber);
            //return line?.Text.Reverse().SkipWhile(c => c == '\n' || c == '\r').FirstOrDefault() ?? '\0';
            var lineEndPos = DirectMessage(NativeMethods.SCI_GETLINEENDPOSITION, new IntPtr(lineNumber)).ToInt32();
            var lineStartPos = DirectMessage(NativeMethods.SCI_POSITIONFROMLINE, new IntPtr(lineNumber)).ToInt32();
            char lineLastChar;
            do
            {
                lineLastChar = (char)DirectMessage(NativeMethods.SCI_GETCHARAT, new IntPtr(lineEndPos--));
            }
            while (lineEndPos >= lineStartPos && (lineLastChar == '\n' || lineLastChar == '\r'));
            return lineLastChar;
        }

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

        public int GetLineLength(int lineNumber)
        {
            //var line = new Line(scintilla, lineNumber);
            //return line?.Length ?? 0;
            return DirectMessage(NativeMethods.SCI_LINELENGTH, new IntPtr(lineNumber)).ToInt32();
        }


        private const int ErrorIndex = 20;
        private const int WarningIndex = 21;
        private const int TypeNameIndex = 22;

        public void SetupIndicatorStyles()
        {
            //scintilla.Indicators[ErrorIndex].Style = IndicatorStyle.CompositionThick;
            DirectMessage(NativeMethods.SCI_INDICSETSTYLE, new IntPtr(ErrorIndex), new IntPtr(NativeMethods.INDIC_COMPOSITIONTHICK));
            //scintilla.Indicators[ErrorIndex].ForeColor = System.Drawing.Color.Crimson;
            DirectMessage(NativeMethods.SCI_INDICSETFORE, ErrorIndex, Eto.Drawing.Colors.Crimson);
            //scintilla.Indicators[ErrorIndex].Alpha = 255;
   
            //scintilla.Indicators[WarningIndex].Style = IndicatorStyle.CompositionThick;
            DirectMessage(NativeMethods.SCI_INDICSETSTYLE, new IntPtr(WarningIndex), new IntPtr(NativeMethods.INDIC_COMPOSITIONTHICK));
            //scintilla.Indicators[WarningIndex].ForeColor = System.Drawing.Color.DarkOrange;
            DirectMessage(NativeMethods.SCI_INDICSETFORE, WarningIndex, Eto.Drawing.Colors.DarkOrange);
            //scintilla.Indicators[WarningIndex].Alpha = 255;

            //scintilla.Indicators[TypeNameIndex].Style = IndicatorStyle.TextFore;
            DirectMessage(NativeMethods.SCI_INDICSETSTYLE, new IntPtr(TypeNameIndex), new IntPtr(NativeMethods.INDIC_TEXTFORE));
            //scintilla.Indicators[TypeNameIndex].ForeColor = System.Drawing.Color.FromArgb(43, 145, 175);
            DirectMessage(NativeMethods.SCI_INDICSETFORE, TypeNameIndex, Eto.Drawing.Color.FromArgb(43, 145, 175));
        }

        public void ClearAllErrorIndicators()
        {
            //scintilla.IndicatorCurrent = ErrorIndex;
            DirectMessage(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(ErrorIndex));
            //scintilla.IndicatorClearRange(0, scintilla.TextLength);
            DirectMessage(NativeMethods.SCI_INDICATORCLEARRANGE, IntPtr.Zero, new IntPtr(Text.Length));
        }
        public void ClearAllWarningIndicators()
        {
            //scintilla.IndicatorCurrent = WarningIndex;
            DirectMessage(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(WarningIndex));
            //scintilla.IndicatorClearRange(0, scintilla.TextLength);
            DirectMessage(NativeMethods.SCI_INDICATORCLEARRANGE, IntPtr.Zero, new IntPtr(Text.Length));
        }
        public void ClearAllTypeNameIndicators()
        {
            //scintilla.IndicatorCurrent = TypeNameIndex;
            DirectMessage(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(TypeNameIndex));
            //scintilla.IndicatorClearRange(0, scintilla.TextLength);
            DirectMessage(NativeMethods.SCI_INDICATORCLEARRANGE, IntPtr.Zero, new IntPtr(Text.Length));
        }

        public void AddErrorIndicator(int position, int length)
        {
            //scintilla.IndicatorCurrent = ErrorIndex;
            DirectMessage(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(ErrorIndex));
            //scintilla.IndicatorFillRange(position, length);
            DirectMessage(NativeMethods.SCI_INDICATORFILLRANGE, new IntPtr(position), new IntPtr(length));
        }
        public void AddWarningIndicator(int position, int length)
        {
            //scintilla.IndicatorCurrent = WarningIndex;
            DirectMessage(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(WarningIndex));
            //scintilla.IndicatorFillRange(position, length);
            DirectMessage(NativeMethods.SCI_INDICATORFILLRANGE, new IntPtr(position), new IntPtr(length));
        }
        public void AddTypeNameIndicator(int position, int length)
        {
            //scintilla.IndicatorCurrent = TypeNameIndex;
            DirectMessage(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(TypeNameIndex));
            //scintilla.IndicatorFillRange(position, length);
            DirectMessage(NativeMethods.SCI_INDICATORFILLRANGE, new IntPtr(position), new IntPtr(length));
        }

        public bool IsWhitespaceVisible => DirectMessage(NativeMethods.SCI_GETVIEWWS).ToInt32() != NativeMethods.SCWS_INVISIBLE;

        public void ShowWhitespace()
        {
            //scintilla.ViewWhitespace = WhitespaceMode.VisibleAlways;
            DirectMessage(NativeMethods.SCI_SETVIEWWS, new IntPtr(NativeMethods.SCWS_VISIBLEALWAYS));
        }

        public void HideWhitespace()
        {
            //scintilla.ViewWhitespace = WhitespaceMode.Invisible;
            DirectMessage(NativeMethods.SCI_SETVIEWWS, new IntPtr(0));
        }

        public void ShowWhitespaceWithColor(Eto.Drawing.Color color)
        {
            ShowWhitespace();
            //scintilla.SetWhitespaceBackColor(true, System.Drawing.Color.FromArgb(color.ToArgb()));
            DirectMessage(NativeMethods.SCI_SETWHITESPACEBACK, color);
        }

        //public bool AreIndentationGuidesVisible => scintilla.IndentationGuides != IndentView.None;
        public bool AreIndentationGuidesVisible => DirectMessage(NativeMethods.SCI_GETINDENTATIONGUIDES).ToInt32() != NativeMethods.SC_IV_NONE;

        public void ShowIndentationGuides()
        {
            //scintilla.IndentationGuides = IndentView.LookBoth;
            DirectMessage(NativeMethods.SCI_SETINDENTATIONGUIDES, new IntPtr(NativeMethods.SC_IV_LOOKBOTH));
        }

        public void HideIndentationGuides()
        {
            //scintilla.IndentationGuides = IndentView.None;
            DirectMessage(NativeMethods.SCI_SETINDENTATIONGUIDES, new IntPtr(NativeMethods.SC_IV_NONE));
        }

        public bool AutoCompleteActive { get { return DirectMessage(NativeMethods.SCI_AUTOCACTIVE) != IntPtr.Zero; } }

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

        public string GetTextRange(int position, int length)
        {
            //return scintilla.GetTextRange(position, length);
            //TODO: use SCI_GETRANGEPOINTER instead
            string txt = Text;
            return txt.Substring(position, length);
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

        internal IntPtr DirectMessage(int msg, int wParam, int lParam)
        {
            return DirectMessage(msg, new IntPtr(wParam), new IntPtr(lParam));
        }

        internal IntPtr DirectMessage(int msg)
        {
            return DirectMessage(msg, IntPtr.Zero, IntPtr.Zero);
        }

        internal IntPtr DirectMessage(int msg, IntPtr wParam)
        {
            return DirectMessage(msg, wParam, IntPtr.Zero);
        }

        internal IntPtr DirectMessage(int msg, int wParam, Eto.Drawing.Color color)
        {
            int c = (color.Bb << 16) + (color.Gb << 8) + color.Rb;
            return DirectMessage(msg, new IntPtr(wParam), new IntPtr(c));
        }

        internal IntPtr DirectMessage(int msg, Eto.Drawing.Color color)
        {
            return DirectMessage(msg, 1, color);
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

        public void HandleScintillaMessage(int message, char c, int position)
        {
            switch (message)
            {

                case NativeMethods.SCN_CHARADDED:
                    CharAdded?.Invoke(this, new CharAddedEventArgs(c));
                    break;
                case NativeMethods.SCN_MODIFIED:
                    /*if ((n.modificationType & NativeMethods.SC_MOD_INSERTCHECK) > 0)
                    {
                        var text = Helpers.GetString(n.text, (int)n.length, Encoding);
                        InsertCheck?.Invoke(this, new InsertCheckEventArgs(text));
                    }*/
                    TextChanged?.Invoke(this, EventArgs.Empty);
                    break;
                case NativeMethods.SCN_MARGINCLICK:
                    const uint bmmask = (1 << BREAKPOINT_MARKER);
                    var lineNumber = DirectMessage(NativeMethods.SCI_LINEFROMPOSITION, new IntPtr(position));
                    var mask = DirectMessage(NativeMethods.SCI_MARKERGET, lineNumber).ToInt32();
                    var uimask = unchecked((uint)mask);
                    var addOrRemove = ((uimask & bmmask) > 0) ? BreakpointChangeType.Remove : BreakpointChangeType.Add;
                    if (addOrRemove == BreakpointChangeType.Add && string.IsNullOrWhiteSpace(GetLineText(lineNumber.ToInt32())))
                        return;
                    //Control.SetGeneralProperty(addOrRemove == BreakpointChangeType.Add ? NativeMethods.SCI_MARKERADD : NativeMethods.SCI_MARKERDELETE, lineNumber, BREAKPOINT_MARKER);
                    DirectMessage(addOrRemove == BreakpointChangeType.Add ? NativeMethods.SCI_MARKERADD : NativeMethods.SCI_MARKERDELETE, lineNumber, new IntPtr(BREAKPOINT_MARKER));
                    BreakpointsChanged?.Invoke(this, new BreakpointsChangedEventArgs(addOrRemove, lineNumber.ToInt32()));
                    break;
                default:
                    break;
            }
        }
    }
}
