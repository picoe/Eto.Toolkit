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

[assembly: ExportHandler(typeof(CodeEditor), typeof(CodeEditorHandler))]

namespace Eto.CodeEditor.Wpf
{
    public class CodeEditorHandler : Eto.Wpf.Forms.WindowsFormsHostHandler<ScintillaNET.Scintilla, CodeEditor, CodeEditor.ICallback>, CodeEditor.IHandler
    {
        public CodeEditorHandler()
        {
            string path = ScintillaControl.UnpackNativeScintilla();
            ScintillaNET.Scintilla.SetModulePath(path);
            WinFormsControl = new ScintillaNET.Scintilla();

            WinFormsControl.CharAdded += WinFormsControl_CharAdded;
            WinFormsControl.TextChanged += WinFormsControl_TextChanged;
            WinFormsControl.InsertCheck += WinFormsControl_InsertCheck;
            WinFormsControl.AutoCMaxHeight = 10;
            WinFormsControl.AutomaticFold = AutomaticFold.Click;

            FontName = "Consolas";
            FontSize = 11;
            LineNumberColumnWidth = 40;
        }

        private void WinFormsControl_InsertCheck(object sender, ScintillaNET.InsertCheckEventArgs e)
        {
            InsertCheck?.Invoke(this, new Eto.CodeEditor.InsertCheckEventArgs(e.Text));
        }

        public void ChangeInsertion(string text)
        {
            // this method shouldn't be part of the handler interface as it's not needed on windows.
            // on windows the handler set `e.Text = "some text"` and the setter calls the native ChangeInsertion
            throw new NotImplementedException("InsertCheck handler needs to be reworked.");
        }

        private void WinFormsControl_CharAdded(object sender, ScintillaNET.CharAddedEventArgs e)
        {
            CharAdded?.Invoke(this, new Eto.CodeEditor.CharAddedEventArgs((char)e.Char));
        }

        private void WinFormsControl_TextChanged(object sender, EventArgs e)
        {
            TextChanged?.Invoke(this, new Eto.CodeEditor.TextChangedEventArgs());

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

        public string Text
        {
            get => WinFormsControl.Text;
            set => WinFormsControl.Text = value;
        }

        public void SetKeywords(int set, string keywords)
        {
            WinFormsControl.SetKeywords(set, keywords);
        }

        ProgrammingLanguage _language = ProgrammingLanguage.None;
        public ProgrammingLanguage Language
        {
            get { return _language; }
            set
            {
                _language = value;
                WinFormsControl.Lexer = ToScintillaNet(value);
            }
        }

        static ScintillaNET.Lexer ToScintillaNet(ProgrammingLanguage l)
        {
            switch (l)
            {
                case ProgrammingLanguage.CSharp:
                case ProgrammingLanguage.GLSL:
                    return ScintillaNET.Lexer.Cpp;
                case ProgrammingLanguage.VB:
                    return ScintillaNET.Lexer.Vb;
                case ProgrammingLanguage.Python:
                    return ScintillaNET.Lexer.Python;
            }
            return ScintillaNET.Lexer.Null;
        }

        public string FontName
        {
            get { return WinFormsControl.Styles[ScintillaNET.Style.Default].Font; }
            set { WinFormsControl.Styles[ScintillaNET.Style.Default].Font = value; }
        }

        public int FontSize
        {
            get { return WinFormsControl.Styles[ScintillaNET.Style.Default].Size; }
            set { WinFormsControl.Styles[ScintillaNET.Style.Default].Size = value; }
        }

        public void SetColor(Section section, Eto.Drawing.Color forecolor, Eto.Drawing.Color backcolor)
        {
            var fg = System.Drawing.Color.FromArgb(forecolor.Rb, forecolor.Gb, forecolor.Bb);
            var bg = System.Drawing.Color.FromArgb(backcolor.Rb, backcolor.Gb, backcolor.Bb);

            if ( section == Section.Default)
            {
                WinFormsControl.Styles[ScintillaNET.Style.Default].ForeColor = fg;
                WinFormsControl.CaretForeColor = fg;
                WinFormsControl.Styles[ScintillaNET.Style.Default].BackColor = bg;
                WinFormsControl.StyleClearAll();
            }
            if (section == Section.Comment)
            {
                foreach (var id in CommentStyleIds(Language))
                {
                    WinFormsControl.Styles[id].ForeColor = fg;
                    WinFormsControl.Styles[id].BackColor = bg;
                }
            }
            if (section == Section.Keyword1)
            {
                foreach (var id in Keyword1Ids(Language))
                {
                    WinFormsControl.Styles[id].ForeColor = fg;
                    WinFormsControl.Styles[id].BackColor = bg;
                }
            }
            if (section == Section.Keyword2)
            {
                foreach (var id in Keyword2Ids(Language))
                {
                    WinFormsControl.Styles[id].ForeColor = fg;
                    WinFormsControl.Styles[id].BackColor = bg;
                }
            }
            if (section == Section.Strings)
            {
                foreach (var id in StringStyleIds(Language))
                {
                    WinFormsControl.Styles[id].ForeColor = fg;
                    WinFormsControl.Styles[id].BackColor = bg;
                }
            }
            if (section == Section.LineNumber)
            {
                WinFormsControl.Styles[ScintillaNET.Style.LineNumber].ForeColor = fg;
                WinFormsControl.Styles[ScintillaNET.Style.LineNumber].BackColor = bg;
            }
            if( section == Section.DefName && Language == ProgrammingLanguage.Python)
            {
                WinFormsControl.Styles[ScintillaNET.Style.Python.DefName].ForeColor = fg;
                WinFormsControl.Styles[ScintillaNET.Style.Python.DefName].BackColor = bg;
            }
            if(section == Section.Preprocessor)
            {
                foreach (var id in PreprocessorIds(Language))
                {
                    WinFormsControl.Styles[id].ForeColor = fg;
                    WinFormsControl.Styles[id].BackColor = bg;
                }

            }
        }

        static int[] CommentStyleIds(ProgrammingLanguage language)
        {
            if (language == ProgrammingLanguage.Python)
                return new int[] { ScintillaNET.Style.Python.CommentBlock, ScintillaNET.Style.Python.CommentLine};

            if (language == ProgrammingLanguage.VB)
                return new int[] { ScintillaNET.Style.Vb.Comment, ScintillaNET.Style.Vb.CommentBlock,
                    ScintillaNET.Style.Vb.DocBlock, ScintillaNET.Style.Vb.Preprocessor};

            return new int[] {ScintillaNET.Style.Cpp.Comment, ScintillaNET.Style.Cpp.CommentLine,
                ScintillaNET.Style.Cpp.CommentDoc, ScintillaNET.Style.Cpp.CommentLineDoc};
        }

        static int[] StringStyleIds(ProgrammingLanguage language)
        {
            if (language == ProgrammingLanguage.Python)
                return new int[] { ScintillaNET.Style.Python.Character, ScintillaNET.Style.Python.String,
                    ScintillaNET.Style.Python.Triple, ScintillaNET.Style.Python.TripleDouble };

            if (language == ProgrammingLanguage.VB)
                return new int[] { ScintillaNET.Style.Vb.String };
            return new int[] {ScintillaNET.Style.Cpp.String, ScintillaNET.Style.Cpp.Character};
        }

        static int[] Keyword1Ids(ProgrammingLanguage language)
        {
            if (language == ProgrammingLanguage.Python)
                return new int[] { ScintillaNET.Style.Python.Word };

            if (language == ProgrammingLanguage.VB)
                return new int[] { ScintillaNET.Style.Vb.Keyword };

            return new int[] { ScintillaNET.Style.Cpp.Word };
        }

        static int[] Keyword2Ids(ProgrammingLanguage language)
        {
            if (language == ProgrammingLanguage.Python)
                return new int[] { ScintillaNET.Style.Python.Word2 };

            if (language == ProgrammingLanguage.VB)
                return new int[] { ScintillaNET.Style.Vb.Keyword2, ScintillaNET.Style.Vb.Keyword3, ScintillaNET.Style.Vb.Keyword4 };

            return new int[] { ScintillaNET.Style.Cpp.Word2 };
        }

        static int[] PreprocessorIds(ProgrammingLanguage language)
        {
            if (language == ProgrammingLanguage.Python)
                return new int[] { };

            if (language == ProgrammingLanguage.VB)
                return new int[] { ScintillaNET.Style.Vb.Preprocessor };

            return new int[] { ScintillaNET.Style.Cpp.Preprocessor };
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
            WinFormsControl.Indicators[ErrorIndex].Style = IndicatorStyle.CompositionThick;
            WinFormsControl.Indicators[ErrorIndex].ForeColor = System.Drawing.Color.Crimson;
            //WinFormsControl.Indicators[ErrorIndex].Alpha = 255;
   
            WinFormsControl.Indicators[WarningIndex].Style = IndicatorStyle.CompositionThick;
            WinFormsControl.Indicators[WarningIndex].ForeColor = System.Drawing.Color.DarkOrange;
            //WinFormsControl.Indicators[WarningIndex].Alpha = 255;

            WinFormsControl.Indicators[TypeNameIndex].Style = IndicatorStyle.TextFore;
            WinFormsControl.Indicators[TypeNameIndex].ForeColor = System.Drawing.Color.FromArgb(43, 145, 175);
        }
        public void ClearAllErrorIndicators()
        {
            WinFormsControl.IndicatorCurrent = ErrorIndex;
            WinFormsControl.IndicatorClearRange(0, WinFormsControl.TextLength);
        }
        public void ClearAllWarningIndicators()
        {
            WinFormsControl.IndicatorCurrent = WarningIndex;
            WinFormsControl.IndicatorClearRange(0, WinFormsControl.TextLength);
        }
        public void ClearAllTypeNameIndicators()
        {
            WinFormsControl.IndicatorCurrent = TypeNameIndex;
            WinFormsControl.IndicatorClearRange(0, WinFormsControl.TextLength);
        }

        public void AddErrorIndicator(int position, int length)
        {
            WinFormsControl.IndicatorCurrent = ErrorIndex;
            WinFormsControl.IndicatorFillRange(position, length);
        }
        public void AddWarningIndicator(int position, int length)
        {
            WinFormsControl.IndicatorCurrent = WarningIndex;
            WinFormsControl.IndicatorFillRange(position, length);
        }
        public void AddTypeNameIndicator(int position, int length)
        {
            WinFormsControl.IndicatorCurrent = TypeNameIndex;
            WinFormsControl.IndicatorFillRange(position, length);
        }

        public int LineNumberColumnWidth
        {
            get
            {
                return WinFormsControl.Margins[0].Width;
            }
            set
            {
                WinFormsControl.Margins[0].Width = value;
            }
        }

        public int TabWidth { get => WinFormsControl.TabWidth; set => WinFormsControl.TabWidth = value; }
        public bool ReplaceTabsWithSpaces { get => !WinFormsControl.UseTabs; set => WinFormsControl.UseTabs = !value; }
        public bool BackspaceUnindents { get => WinFormsControl.BackspaceUnindents; set => WinFormsControl.BackspaceUnindents = value;}}
        public int CurrentPosition { get => WinFormsControl.CurrentPosition; set => WinFormsControl.CurrentPosition = value; }

        public int CurrentPositionInLine => CurrentPosition - WinFormsControl.Lines[CurrentLineNumber].Position;

        public int CurrentLineNumber => WinFormsControl.CurrentLine;

        public bool IsWhitespaceVisible => WinFormsControl.ViewWhitespace != WhitespaceMode.Invisible;

        public void ShowWhitespace()
        {
            WinFormsControl.ViewWhitespace = WhitespaceMode.VisibleAlways;
        }

        public void HideWhitespace()
        {
            WinFormsControl.ViewWhitespace = WhitespaceMode.Invisible;
        }

        public void ShowWhitespaceWithColor(Eto.Drawing.Color color)
        {
            ShowWhitespace();
            WinFormsControl.SetWhitespaceBackColor(true, System.Drawing.Color.FromArgb(color.ToArgb()));
        }

        public bool AreIndentationGuidesVisible => WinFormsControl.IndentationGuides != IndentView.None;

        public void ShowIndentationGuides()
        {
            WinFormsControl.IndentationGuides = IndentView.LookBoth;
        }

        public void HideIndentationGuides()
        {
            WinFormsControl.IndentationGuides = IndentView.None;
        }

        public int GetLineIndentation(int lineNumber)
        {
            var line = new Line(WinFormsControl, lineNumber);
            return line?.Indentation ?? 0;
        }

        public void SetLineIndentation(int lineNumber, int indentation)
        {
            var line = new Line(WinFormsControl, lineNumber);
            if (line != null)
            {
                line.Indentation = indentation;
                WinFormsControl.GotoPosition(line.Position + indentation);
            }
        }

        public char GetLineLastChar(int lineNumber)
        {
            var line = new Line(WinFormsControl, lineNumber);
            return line?.Text.Reverse().SkipWhile(c => c == '\n' || c == '\r').FirstOrDefault() ?? '\0';
        }

        public string GetLineText(int lineNumber)
        {
            var line = new Line(WinFormsControl, lineNumber);
            return line?.Text ?? "";
        }

        public int GetLineLength(int lineNumber)
        {
            var line = new Line(WinFormsControl, lineNumber);
            return line?.Length ?? 0;
        }

        public bool AutoCompleteActive { get { return WinFormsControl.AutoCActive; } }
        public void InsertText(int position, string text) { WinFormsControl.InsertText(position, text); }
        public void DeleteRange(int position, int length) { WinFormsControl.DeleteRange(position, length); }

        public int ReplaceTarget(string text, int start, int end)
        {
            WinFormsControl.SetTargetRange(start, end);
            return WinFormsControl.ReplaceTarget(text);
        }

        public void ReplaceFirstOccuranceInLine(string oldText, string newText, int lineNumber)
        {
            var line = WinFormsControl.Lines[lineNumber];
            WinFormsControl.SetTargetRange(line.Position, line.EndPosition);

            var pos = WinFormsControl.SearchInTarget(oldText);
            if (pos == -1)
              return;

            WinFormsControl.SetTargetRange(pos, pos + oldText.Length);

            WinFormsControl.ReplaceTarget(newText);
        }

        public int WordStartPosition(int position, bool onlyWordCharacters)
        {
            return WinFormsControl.WordStartPosition(position, onlyWordCharacters);
        }
        public string GetTextRange(int position, int length)
        {
            return WinFormsControl.GetTextRange(position, length);
        }
        public void AutoCompleteShow(int lenEntered, string list)
        {
            WinFormsControl.AutoCShow(lenEntered, list);
        }

        public void BreakOnLine(int lineNumber) => throw new NotImplementedException();
        public event EventHandler<BreakpointsChangedEventArgs> BreakpointsChanged;
        public void ClearBreak() => throw new NotImplementedException();
        public void ClearBreakpoints() => throw new NotImplementedException();
        public bool IsBreakpointsMarginVisible
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
    }


}
