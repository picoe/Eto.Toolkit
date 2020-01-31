using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto;
using Eto.Forms;
using Eto.CodeEditor;
using Eto.CodeEditor.Wpf;
using ScintillaNET;
using Eto.Drawing;
using System.Windows.Forms;
using Eto.Wpf.Forms;

[assembly: ExportHandler(typeof(CodeEditor), typeof(CodeEditorHandler))]

namespace Eto.CodeEditor.Wpf
{
    public class CodeEditorHandler : Eto.Wpf.Forms.WindowsFormsHostHandler<Scintilla.ScintillaControl, CodeEditor, CodeEditor.ICallback>, CodeEditor.IHandler
    {
        private const int BREAKPOINT_MARKER = 3; // arbitrary number
        private const int BREAK_MARKER = 4; // arbitrary number

        private const int BREAKPOINTS_MARGIN = 1;
        private const int LINENUMBERS_MARGIN = 2;

        private Scintilla.ScintillaControl scintilla;

        public CodeEditorHandler()
        {
            //string path = ScintillaControl.UnpackNativeScintilla();
            //ScintillaNET.Scintilla.SetModulePath(path);
            scintilla = new Scintilla.ScintillaControl(); // new ScintillaNET.Scintilla();
            WinFormsControl = scintilla;

            /*scintilla*/this.CharAdded += WinFormsControl_CharAdded;
            scintilla.TextChanged += WinFormsControl_TextChanged;
            /*scintilla.*/this.InsertCheck += WinFormsControl_InsertCheck;
            //scintilla.AutoCMaxHeight = 10;
            scintilla.DirectMessage(NativeMethods.SCI_AUTOCSETMAXHEIGHT, new IntPtr(10));
            //scintilla.AutomaticFold = AutomaticFold.Click;
            scintilla.DirectMessage(NativeMethods.SCI_SETAUTOMATICFOLD, new IntPtr(NativeMethods.SC_AUTOMATICFOLD_CLICK));
            
            FontName = "Consolas";
            FontSize = 11;
            LineNumberColumnWidth = 40;

            Action<int, int, int> SetGeneralProperty = (c, i, j) => scintilla.DirectMessage(c, new IntPtr(i), new IntPtr(j));
            // breakpoints margin
            SetGeneralProperty(NativeMethods.SCI_SETMARGINSENSITIVEN, BREAKPOINTS_MARGIN, 1);
            SetGeneralProperty(NativeMethods.SCI_SETMARGINTYPEN, BREAKPOINTS_MARGIN, NativeMethods.SC_MARGIN_SYMBOL);
            SetGeneralProperty(NativeMethods.SCI_SETMARGINMASKN, BREAKPOINTS_MARGIN, int.MaxValue); // ScintillaNet -> public const uint MaskAll = unchecked((uint)-1);
            SetGeneralProperty(NativeMethods.SCI_MARKERDEFINE, BREAKPOINTS_MARGIN, NativeMethods.SC_MARK_FULLRECT);
            IsBreakpointsMarginVisible = false;

            // line numbers margin
            SetGeneralProperty(NativeMethods.SCI_SETMARGINSENSITIVEN, LINENUMBERS_MARGIN, 0);
            SetGeneralProperty(NativeMethods.SCI_SETMARGINTYPEN, LINENUMBERS_MARGIN, NativeMethods.SC_MARGIN_NUMBER);

            // breakpoint marker
            SetGeneralProperty(NativeMethods.SCI_MARKERDEFINE, BREAKPOINT_MARKER, NativeMethods.SC_MARK_CIRCLE); // default
            var red = 255; // 0xFF0000; // */ 16711680;
            SetGeneralProperty(NativeMethods.SCI_MARKERSETFORE, BREAKPOINT_MARKER, red);
            SetGeneralProperty(NativeMethods.SCI_MARKERSETBACK, BREAKPOINT_MARKER, red);

            // break marker
            SetGeneralProperty(NativeMethods.SCI_MARKERDEFINE, BREAK_MARKER, NativeMethods.SC_MARK_ARROW);
            var yellow = 0x00FFFF; // */ 16776960;
            SetGeneralProperty(NativeMethods.SCI_MARKERSETFORE, BREAK_MARKER, 0xFFFFFF); //black
            SetGeneralProperty(NativeMethods.SCI_MARKERSETBACK, BREAK_MARKER, yellow);

        }

        private void WinFormsControl_InsertCheck(object sender, /*ScintillaNET.*/InsertCheckEventArgs e)
        {
            InsertCheck?.Invoke(this, new Eto.CodeEditor.InsertCheckEventArgs(e.Text));
        }

        public void ChangeInsertion(string text)
        {
            // this method shouldn't be part of the handler interface as it's not needed on windows.
            // on windows the handler set `e.Text = "some text"` and the setter calls the native ChangeInsertion
            throw new NotImplementedException("InsertCheck handler needs to be reworked.");
        }

        private int MarkerNext(int lineNumber) => 
            scintilla.DirectMessage(NativeMethods.SCI_MARKERNEXT, new IntPtr(lineNumber), new IntPtr(1 << BREAKPOINT_MARKER)).ToInt32();
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

        private void WinFormsControl_CharAdded(object sender, /*ScintillaNET.*/CharAddedEventArgs e)
        {
            CharAdded?.Invoke(this, new Eto.CodeEditor.CharAddedEventArgs((char)e.Char));
        }

        private void WinFormsControl_TextChanged(object sender, EventArgs e)
        {
            TextChanged?.Invoke(this, new Eto.CodeEditor.TextChangedEventArgs());

        }


        #region IHandler impl
        public string Text
        {
            get => scintilla.Text;
            set => scintilla.Text = value;
        }

        public void SetProgrammingLanguage(ProgrammingLanguage language, string[] keywordSets)
        {
            scintilla.SetProgrammingLanguage(language, keywordSets);
        }

        public string FontName
        {
            get => scintilla.FontName;
            set => scintilla.FontName = value;
        }

        public void SetColor(Section section, Eto.Drawing.Color foreground, Eto.Drawing.Color background)
        {
            scintilla.SetColor(section, foreground, background);
        }

        public int CurrentPosition
        {
            get => scintilla.CurrentPosition;
            set => scintilla.CurrentPosition = value;
        }

        public int CurrentPositionInLine => scintilla.CurrentPositionInLine;

        public int CurrentLineNumber => scintilla.CurrentLineNumber;

        public string GetLineText(int lineNumber)
        {
            return scintilla.GetLineText(lineNumber);
        }

        public unsafe void InsertText(int position, string text) 
        {
            scintilla.InsertText(position, text);
        }

        public unsafe int ReplaceTarget(string text, int start, int end)
        {
            return scintilla.ReplaceTarget(text, start, end);
        }

        public void DeleteRange(int position, int length) 
        {
            scintilla.DeleteRange(position, length);
        }

        public int WordStartPosition(int position, bool onlyWordCharacters)
        {
            return scintilla.WordStartPosition(position, onlyWordCharacters);
        }

        public unsafe void AutoCompleteShow(int lenEntered, string list)
        {
            scintilla.AutoCompleteShow(lenEntered, list);
        }
        #endregion

        public int FontSize
        {
            //get { return scintilla.Styles[ScintillaNET.Style.Default].Size; }
            //set { scintilla.Styles[ScintillaNET.Style.Default].Size = value; }
            get
            {
                return scintilla.DirectMessage(NativeMethods.SCI_STYLEGETSIZE, new IntPtr(ScintillaNET.NativeMethods.STYLE_DEFAULT), IntPtr.Zero).ToInt32();
            }
            set
            {
                scintilla.DirectMessage(NativeMethods.SCI_STYLESETSIZE, new IntPtr(ScintillaNET.NativeMethods.STYLE_DEFAULT), new IntPtr(value));
            }
        }

        private const int ErrorIndex = 20;
        private const int WarningIndex = 21;
        private const int TypeNameIndex = 22;

        public event EventHandler<Eto.CodeEditor.CharAddedEventArgs> CharAdded;
        //public event EventHandler<Eto.CodeEditor.TextChangedEventArgs> TextChanged;
        public event EventHandler<EventArgs> TextChanged;
        public event EventHandler<Eto.CodeEditor.InsertCheckEventArgs> InsertCheck;

        public void SetupIndicatorStyles()
        {
            //scintilla.Indicators[ErrorIndex].Style = IndicatorStyle.CompositionThick;
            scintilla.DirectMessage(NativeMethods.SCI_INDICSETSTYLE, new IntPtr(ErrorIndex), new IntPtr(NativeMethods.INDIC_COMPOSITIONTHICK));
            //scintilla.Indicators[ErrorIndex].ForeColor = System.Drawing.Color.Crimson;
            scintilla.DirectMessage(NativeMethods.SCI_INDICSETFORE, new IntPtr(ErrorIndex), new IntPtr(ColorTranslator.ToWin32(System.Drawing.Color.Crimson)));
            //scintilla.Indicators[ErrorIndex].Alpha = 255;
   
            //scintilla.Indicators[WarningIndex].Style = IndicatorStyle.CompositionThick;
            scintilla.DirectMessage(NativeMethods.SCI_INDICSETSTYLE, new IntPtr(WarningIndex), new IntPtr(NativeMethods.INDIC_COMPOSITIONTHICK));
            //scintilla.Indicators[WarningIndex].ForeColor = System.Drawing.Color.DarkOrange;
            scintilla.DirectMessage(NativeMethods.SCI_INDICSETFORE, new IntPtr(WarningIndex), new IntPtr(ColorTranslator.ToWin32(System.Drawing.Color.DarkOrange)));
            //scintilla.Indicators[WarningIndex].Alpha = 255;

            //scintilla.Indicators[TypeNameIndex].Style = IndicatorStyle.TextFore;
            scintilla.DirectMessage(NativeMethods.SCI_INDICSETSTYLE, new IntPtr(TypeNameIndex), new IntPtr(NativeMethods.INDIC_TEXTFORE));
            //scintilla.Indicators[TypeNameIndex].ForeColor = System.Drawing.Color.FromArgb(43, 145, 175);
            scintilla.DirectMessage(NativeMethods.SCI_INDICSETFORE, new IntPtr(TypeNameIndex), new IntPtr(ColorTranslator.ToWin32(System.Drawing.Color.FromArgb(43, 145, 175))));
        }
        public void ClearAllErrorIndicators()
        {
            //scintilla.IndicatorCurrent = ErrorIndex;
            scintilla.DirectMessage(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(ErrorIndex));
            //scintilla.IndicatorClearRange(0, scintilla.TextLength);
            scintilla.DirectMessage(NativeMethods.SCI_INDICATORCLEARRANGE, IntPtr.Zero, new IntPtr(Text.Length));
        }
        public void ClearAllWarningIndicators()
        {
            //scintilla.IndicatorCurrent = WarningIndex;
            scintilla.DirectMessage(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(WarningIndex));
            //scintilla.IndicatorClearRange(0, scintilla.TextLength);
            scintilla.DirectMessage(NativeMethods.SCI_INDICATORCLEARRANGE, IntPtr.Zero, new IntPtr(Text.Length));
        }
        public void ClearAllTypeNameIndicators()
        {
            //scintilla.IndicatorCurrent = TypeNameIndex;
            scintilla.DirectMessage(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(TypeNameIndex));
            //scintilla.IndicatorClearRange(0, scintilla.TextLength);
            scintilla.DirectMessage(NativeMethods.SCI_INDICATORCLEARRANGE, IntPtr.Zero, new IntPtr(Text.Length));
        }

        public void AddErrorIndicator(int position, int length)
        {
            //scintilla.IndicatorCurrent = ErrorIndex;
            scintilla.DirectMessage(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(ErrorIndex));
            //scintilla.IndicatorFillRange(position, length);
            scintilla.DirectMessage(NativeMethods.SCI_INDICATORFILLRANGE, new IntPtr(position), new IntPtr(length));
        }
        public void AddWarningIndicator(int position, int length)
        {
            //scintilla.IndicatorCurrent = WarningIndex;
            scintilla.DirectMessage(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(WarningIndex));
            //scintilla.IndicatorFillRange(position, length);
            scintilla.DirectMessage(NativeMethods.SCI_INDICATORFILLRANGE, new IntPtr(position), new IntPtr(length));
        }
        public void AddTypeNameIndicator(int position, int length)
        {
            //scintilla.IndicatorCurrent = TypeNameIndex;
            scintilla.DirectMessage(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(TypeNameIndex));
            //scintilla.IndicatorFillRange(position, length);
            scintilla.DirectMessage(NativeMethods.SCI_INDICATORFILLRANGE, new IntPtr(position), new IntPtr(length));
        }

        public int LineNumberColumnWidth
        {
            get
            {
                //return scintilla.Margins[0].Width;
                return scintilla.DirectMessage(NativeMethods.SCI_GETMARGINWIDTHN, new IntPtr(LINENUMBERS_MARGIN), IntPtr.Zero).ToInt32();
            }
            set
            {
                //scintilla.Margins[0].Width = value;
                scintilla.DirectMessage(NativeMethods.SCI_SETMARGINWIDTHN, new IntPtr(LINENUMBERS_MARGIN), new IntPtr(value));
            }
        }

        public int TabWidth
        {
            get => scintilla.DirectMessage(NativeMethods.SCI_GETTABWIDTH).ToInt32();
            set => scintilla.DirectMessage(NativeMethods.SCI_SETTABWIDTH, new IntPtr(value));
        }
        
        public bool ReplaceTabsWithSpaces
        {
            get => scintilla.DirectMessage(NativeMethods.SCI_GETUSETABS) != IntPtr.Zero;
            set
            {
                var useTabs = (value ? new IntPtr(1) : IntPtr.Zero);
                scintilla.DirectMessage(NativeMethods.SCI_SETUSETABS, useTabs);
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

        public bool IsWhitespaceVisible => scintilla.DirectMessage(NativeMethods.SCI_GETVIEWWS).ToInt32() != NativeMethods.SCWS_INVISIBLE;

        public void ShowWhitespace()
        {
            //scintilla.ViewWhitespace = WhitespaceMode.VisibleAlways;
            scintilla.DirectMessage(NativeMethods.SCI_SETVIEWWS, new IntPtr(NativeMethods.SCWS_VISIBLEALWAYS));
        }

        public void HideWhitespace()
        {
            //scintilla.ViewWhitespace = WhitespaceMode.Invisible;
            scintilla.DirectMessage(NativeMethods.SCI_SETVIEWWS, new IntPtr(0));
        }

        public void ShowWhitespaceWithColor(Eto.Drawing.Color color)
        {
            ShowWhitespace();
            //scintilla.SetWhitespaceBackColor(true, System.Drawing.Color.FromArgb(color.ToArgb()));
            var colour = ColorTranslator.ToWin32(System.Drawing.Color.FromArgb(color.ToArgb()));
            scintilla.DirectMessage(NativeMethods.SCI_SETWHITESPACEBACK, new IntPtr(1), new IntPtr(colour));
        }

        //public bool AreIndentationGuidesVisible => scintilla.IndentationGuides != IndentView.None;
        public bool AreIndentationGuidesVisible => scintilla.DirectMessage(NativeMethods.SCI_GETINDENTATIONGUIDES).ToInt32() != NativeMethods.SC_IV_NONE;

        public void ShowIndentationGuides()
        {
            //scintilla.IndentationGuides = IndentView.LookBoth;
            scintilla.DirectMessage(NativeMethods.SCI_SETINDENTATIONGUIDES, new IntPtr(NativeMethods.SC_IV_LOOKBOTH));
        }

        public void HideIndentationGuides()
        {
            //scintilla.IndentationGuides = IndentView.None;
            scintilla.DirectMessage(NativeMethods.SCI_SETINDENTATIONGUIDES, new IntPtr(NativeMethods.SC_IV_NONE));
        }

        public int GetLineIndentation(int lineNumber)
        {
            //var line = new Line(scintilla, lineNumber);
            //return line?.Indentation ?? 0;
            return scintilla.DirectMessage(NativeMethods.SCI_GETLINEINDENTATION, new IntPtr(lineNumber)).ToInt32();
        }

        public void SetLineIndentation(int lineNumber, int indentation)
        {
            //var line = new Line(scintilla, lineNumber);
            //if (line != null)
            //{
            //    line.Indentation = indentation;
            //    scintilla.GotoPosition(line.Position + indentation);
            //}
            scintilla.DirectMessage(NativeMethods.SCI_SETLINEINDENTATION, new IntPtr(lineNumber), new IntPtr(indentation));
        }

        public char GetLineLastChar(int lineNumber)
        {
            //var line = new Line(scintilla, lineNumber);
            //return line?.Text.Reverse().SkipWhile(c => c == '\n' || c == '\r').FirstOrDefault() ?? '\0';
            var lineEndPos = scintilla.DirectMessage(NativeMethods.SCI_GETLINEENDPOSITION, new IntPtr(lineNumber)).ToInt32();
            var lineStartPos = scintilla.DirectMessage(NativeMethods.SCI_POSITIONFROMLINE, new IntPtr(lineNumber)).ToInt32();
            char lineLastChar;
            do
            {
                lineLastChar = (char)scintilla.DirectMessage(NativeMethods.SCI_GETCHARAT, new IntPtr(lineEndPos--));
            }
            while (lineEndPos >= lineStartPos && (lineLastChar == '\n' || lineLastChar == '\r'));
            return lineLastChar;
        }


        Encoding Encoding
        {
            get
            {
                int codePage = scintilla.DirectMessage(NativeMethods.SCI_GETCODEPAGE, IntPtr.Zero, IntPtr.Zero).ToInt32();
                return (codePage == 0) ? Encoding.Default : Encoding.GetEncoding(codePage);
            }
        }

        public int GetLineLength(int lineNumber)
        {
            //var line = new Line(scintilla, lineNumber);
            //return line?.Length ?? 0;
            return scintilla.DirectMessage(NativeMethods.SCI_LINELENGTH, new IntPtr(lineNumber)).ToInt32();
        }

        public bool AutoCompleteActive { get { return scintilla.DirectMessage(NativeMethods.SCI_AUTOCACTIVE) != IntPtr.Zero; } }
        public unsafe void ReplaceFirstOccuranceInLine(string oldText, string newText, int lineNumber)
        {
            scintilla.ReplaceFirstOccuranceInLine(oldText, newText, lineNumber);
        }

        public string GetTextRange(int position, int length)
        {
            //return scintilla.GetTextRange(position, length);
            string txt = Text;
            return txt.Substring(position, length);
        }

        public void BreakOnLine(int lineNumber)
        {
            ClearBreak();
            scintilla.DirectMessage(NativeMethods.SCI_MARKERADD, new IntPtr(lineNumber), new IntPtr(BREAK_MARKER));
        }

        public event EventHandler<BreakpointsChangedEventArgs> BreakpointsChanged;

        public void ClearBreak() => scintilla.DirectMessage(NativeMethods.SCI_MARKERDELETEALL, new IntPtr(BREAK_MARKER), IntPtr.Zero);

        public void ClearBreakpoints()
        {
            //Control.SetGeneralProperty(NativeMethods.SCI_MARKERDELETEALL, BREAKPOINT_MARKER);
            scintilla.DirectMessage(NativeMethods.SCI_MARKERDELETEALL, new IntPtr(BREAKPOINT_MARKER), IntPtr.Zero);
            BreakpointsChanged?.Invoke(this, new BreakpointsChangedEventArgs(BreakpointChangeType.Clear));
        }
        public bool IsBreakpointsMarginVisible
        {
            get
            {
                var i = (scintilla.DirectMessage(NativeMethods.SCI_GETMARGINWIDTHN, new IntPtr(BREAKPOINTS_MARGIN), IntPtr.Zero)).ToInt32();
                return i != 0;
            }
            set
            {
                scintilla.DirectMessage(2242, new IntPtr(BREAKPOINTS_MARGIN), value ? new IntPtr(16) : IntPtr.Zero);
            }
        }

        public override Eto.Drawing.Color BackgroundColor
        {
          get
          {
            return Eto.Drawing.Colors.Transparent;
          }
          set
          {
            throw new NotImplementedException();
          }
        }
    }


}
