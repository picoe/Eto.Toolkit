using System;
using System.Collections.Generic;
using sd = System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto;
using Eto.Forms;
using Eto.CodeEditor;
using ScintillaNET;
using ed = Eto.Drawing;

namespace Eto.CodeEditor
{
    public partial class CodeEditorHandler
    {
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

        private ed.FontStyle FontStyle(bool b, bool i) => (b ? ed.FontStyle.Bold : ed.FontStyle.None) | (i ? ed.FontStyle.Italic : ed.FontStyle.None);

        public Eto.Drawing.Font Font
        {
            get => new ed.Font(scintilla.FontName, scintilla.FontSizeFractional, FontStyle(scintilla.Bold, scintilla.Italic)) ;
            set
            {
                scintilla.FontName = value.FamilyName;
                scintilla.FontSizeFractional = value.Size;
                scintilla.Bold = value.Bold;
                scintilla.Italic = value.Italic;
            }
        }

        public string FontName
        {
            get => scintilla.FontName;
            set => scintilla.FontName = value;
        }

        public int FontSize
        {
            get => scintilla.FontSize;
            set => scintilla.FontSize = value;
        }

        public int TabWidth
        {
            get => scintilla.TabWidth;
            set => scintilla.TabWidth = value;
        }

        public bool ReplaceTabsWithSpaces
        {
            get => scintilla.ReplaceTabsWithSpaces;
            set => scintilla.ReplaceTabsWithSpaces = value;
        }

        public bool BackspaceUnindents
        {
            get => scintilla.BackspaceUnindents;
            set => scintilla.BackspaceUnindents = value;
        }

        public int LineNumberColumnWidth
        {
            get => scintilla.LineNumberColumnWidth;
            set => scintilla.LineNumberColumnWidth = value;
        }

        public IEnumerable<int> Breakpoints => scintilla.Breakpoints;

        public bool IsBreakpointsMarginVisible
        {
            get => scintilla.IsBreakpointsMarginVisible;
            set => scintilla.IsBreakpointsMarginVisible = value;
        }

        public void BreakOnLine(int lineNumber) => scintilla.BreakOnLine(lineNumber);

        public void ClearBreak() => scintilla.ClearBreak();

        public void ClearBreakpoints() => scintilla.ClearBreakpoints();

        public bool IsFoldingMarginVisible
        {
            get => scintilla.IsFoldingMarginVisible;
            set => scintilla.IsFoldingMarginVisible = value;
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

        public string WordAtCurrentPosition => scintilla.WordAtCurrentPosition;

        public int GetLineIndentation(int lineNumber) => scintilla.GetLineIndentation(lineNumber);

        public void SetLineIndentation(int lineNumber, int indentation) => scintilla.SetLineIndentation(lineNumber, indentation);

        public char GetLineLastChar(int lineNumber) => scintilla.GetLineLastChar(lineNumber);

        public string GetLineText(int lineNumber)
        {
            return scintilla.GetLineText(lineNumber);
        }

        public int GetLineLength(int lineNumber) => scintilla.GetLineLength(lineNumber);

        public void SetupIndicatorStyles() => scintilla.SetupIndicatorStyles();
        public void ClearAllErrorIndicators() => scintilla.ClearAllErrorIndicators();
        public void ClearAllWarningIndicators() => scintilla.ClearAllWarningIndicators();
        public void ClearAllTypeNameIndicators() => scintilla.ClearAllTypeNameIndicators();
        public void AddErrorIndicator(int position, int length) => scintilla.AddErrorIndicator(position, length);
        public void AddWarningIndicator(int position, int length) => scintilla.AddWarningIndicator(position, length);
        public void AddTypeNameIndicator(int position, int length) => scintilla.AddTypeNameIndicator(position, length);
        public Eto.Drawing.Color HighlightColor
        {
            get => scintilla.HighlightColor;
            set => scintilla.HighlightColor = value;
        }
        public void HighlightRange(int position, int length) => scintilla.AddHighlightIndicator(position, length);
        public void ClearHighlights() => scintilla.ClearAllHighlightIndicators();

        public void SelectRange(int start, int length) => scintilla.SetSelection(start, start + length);

        public bool IsWhitespaceVisible => scintilla.IsWhitespaceVisible;
        public void ShowWhitespace() => scintilla.ShowWhitespace();
        public void HideWhitespace() => scintilla.HideWhitespace();
        public void ShowWhitespaceWithColor(Eto.Drawing.Color color) => scintilla.ShowWhitespaceWithColor(color);
        public bool AreIndentationGuidesVisible => scintilla.AreIndentationGuidesVisible;
        public void ShowIndentationGuides() => scintilla.ShowIndentationGuides();
        public void HideIndentationGuides() => scintilla.HideIndentationGuides();

        public bool AutoCompleteActive => scintilla.AutoCompleteActive;

        public unsafe void InsertText(int position, string text) 
        {
            scintilla.InsertText(position, text);
        }

        public unsafe IList<int> SearchInAll(string text, bool matchCase, bool wholeWord, bool highlight)
            => scintilla.SearchInAll(text, matchCase, wholeWord, highlight);

        public unsafe int ReplaceTarget(string text, int start, int end)
        {
            return scintilla.ReplaceTarget(text, start, end);
        }

        public unsafe void ReplaceFirstOccuranceInLine(string oldText, string newText, int lineNumber)
        {
            scintilla.ReplaceFirstOccuranceInLine(oldText, newText, lineNumber);
        }

        public void DeleteRange(int position, int length) 
        {
            scintilla.DeleteRange(position, length);
        }

        public int WordStartPosition(int position, bool onlyWordCharacters)
        {
            return scintilla.WordStartPosition(position, onlyWordCharacters);
        }

        public string GetTextRange(int position, int length) => scintilla.GetTextRange(position, length);

        public unsafe void AutoCompleteShow(int lenEntered, string list)
        {
            scintilla.AutoCompleteShow(lenEntered, list);
        }

        public unsafe void CallTipsShow(int position, string calltips)
        {
            scintilla.CallTipsShow(position, calltips);
        }

        public unsafe void CallTipsSetHighlight(int start, int end)
        {
            scintilla.CallTipSetHighlight(start, end);
        }

        public bool CallTipIsActive => scintilla.CallTipIsActive;

        public void CallTipCancel() => scintilla.CallTipCancel();

        public event EventHandler<CharAddedEventArgs> CharAdded
        {
            add { scintilla.CharAdded += value; }
            remove { scintilla.CharAdded -= value; }
        }

        public event EventHandler<EventArgs> TextChanged
        {
            add { scintilla.TextChanged += value; }
            remove { scintilla.TextChanged -= value; }
        }

        public event EventHandler<CallTipClickedEventArgs> CallTipClicked
        {
            add { scintilla.CallTipClicked += value; }
            remove { scintilla.CallTipClicked -= value; }
        }

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged
        {
            add { scintilla.SelectionChanged += value; }
            remove { scintilla.SelectionChanged -= value; }
        }

        public event EventHandler<BreakpointsChangedEventArgs> BreakpointsChanged
        {
            add { scintilla.BreakpointsChanged += value; }
            remove { scintilla.BreakpointsChanged -= value; }
        }

#if DEBUG
        public int direct_message(int msg, int val) => scintilla.DirectMessage(msg, new IntPtr(val), IntPtr.Zero).ToInt32();
#endif
#endregion
    }
}
