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

[assembly: ExportHandler(typeof(CodeEditor), typeof(CodeEditorHandler))]

namespace Eto.CodeEditor.Wpf
{
    public class CodeEditorHandler : Eto.Wpf.Forms.WindowsFormsHostHandler<ScintillaNET.Scintilla, CodeEditor, CodeEditor.ICallback>, CodeEditor.IHandler
    {
        public CodeEditorHandler()
        {
            WinFormsControl = new ScintillaNET.Scintilla();
            SetupTheme();
            WinFormsControl.CharAdded += WinFormsControl_CharAdded;
            WinFormsControl.TextChanged += WinFormsControl_TextChanged;
            WinFormsControl.AutoCMaxHeight = 10;
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
            if (section == Section.Comment)
            {
                if (forecolor != Eto.Drawing.Colors.Transparent)
                {
                    WinFormsControl.Styles[ScintillaNET.Style.Cpp.Comment].ForeColor = fg;
                    WinFormsControl.Styles[ScintillaNET.Style.Cpp.CommentLine].ForeColor = fg;
                    WinFormsControl.Styles[ScintillaNET.Style.Cpp.CommentDoc].ForeColor = fg;
                }
                if (backcolor != Eto.Drawing.Colors.Transparent)
                {
                    WinFormsControl.Styles[ScintillaNET.Style.Cpp.Comment].BackColor = bg;
                    WinFormsControl.Styles[ScintillaNET.Style.Cpp.CommentLine].BackColor = bg;
                    WinFormsControl.Styles[ScintillaNET.Style.Cpp.CommentDoc].BackColor = bg;
                }
            }
            if (section == Section.Keyword)
            {
                if (forecolor != Eto.Drawing.Colors.Transparent)
                {
                    WinFormsControl.Styles[ScintillaNET.Style.Cpp.Word].ForeColor = fg;
                    WinFormsControl.Styles[ScintillaNET.Style.Cpp.Word2].ForeColor = fg;
                }
                if (backcolor != Eto.Drawing.Colors.Transparent)
                {
                    WinFormsControl.Styles[ScintillaNET.Style.Cpp.Word].BackColor = bg;
                    WinFormsControl.Styles[ScintillaNET.Style.Cpp.Word2].BackColor = bg;
                }
            }
            if (section == Section.LineNumber)
            {
                if (forecolor != Eto.Drawing.Colors.Transparent)
                {
                    WinFormsControl.Styles[ScintillaNET.Style.LineNumber].ForeColor = fg;
                }
                if (backcolor != Eto.Drawing.Colors.Transparent)
                {
                    WinFormsControl.Styles[ScintillaNET.Style.LineNumber].BackColor = bg;
                }

            }
        }

        private const int ErrorIndex = 20;
        private const int WarningIndex = 21;
        private const int TypeNameIndex = 22;

        public event EventHandler<Eto.CodeEditor.CharAddedEventArgs> CharAdded;
        public event EventHandler<Eto.CodeEditor.TextChangedEventArgs> TextChanged;

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

        void SetupTheme()
        {
            // just style things enough that you can tell you're working in a code editor

            //WinFormsControl.Lexer = ScintillaNET.Lexer.Cpp;
            //WinFormsControl.SetKeywords(0, "abstract as base break case catch checked continue default delegate do else event explicit extern false finally fixed for foreach goto if implicit in interface internal is lock namespace new null object operator out override params private protected public readonly ref return sealed sizeof stackalloc switch this throw true try typeof unchecked unsafe using virtual while");
            //WinFormsControl.SetKeywords(1, "bool byte char class const decimal double enum float int long sbyte short static string struct uint ulong ushort void");
            //WinFormsControl.Styles[ScintillaNET.Style.Cpp.Comment].ForeColor = System.Drawing.Color.Black;
            //WinFormsControl.Styles[ScintillaNET.Style.Cpp.CommentLine].ForeColor = System.Drawing.Color.Black;
            //WinFormsControl.Styles[ScintillaNET.Style.Cpp.CommentDoc].ForeColor = System.Drawing.Color.Black;
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.Number].ForeColor = System.Drawing.Color.Black;
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.String].ForeColor = System.Drawing.Color.Brown;
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.Character].ForeColor = System.Drawing.Color.Brown;
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.Preprocessor].ForeColor = System.Drawing.Color.Black;
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.Operator].ForeColor = System.Drawing.Color.Black;
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.Regex].ForeColor = System.Drawing.Color.Crimson;
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.CommentLineDoc].ForeColor = System.Drawing.Color.Black;
            //WinFormsControl.Styles[ScintillaNET.Style.Cpp.Word].ForeColor = System.Drawing.Color.Blue;
            //WinFormsControl.Styles[ScintillaNET.Style.Cpp.Word2].ForeColor = System.Drawing.Color.CadetBlue;

            //WinFormsControl.Styles[ScintillaNET.Style.Default].Font = "Consolas";
            //WinFormsControl.Styles[ScintillaNET.Style.Default].Size = 10;
            // Show line numbers
            //WinFormsControl.Margins[0].Width = 60;

            //WinFormsControl.Styles[ScintillaNET.Style.LineNumber].BackColor = System.Drawing.Color.White;
            //WinFormsControl.Styles[ScintillaNET.Style.LineNumber].ForeColor = System.Drawing.Color.CadetBlue;

            WinFormsControl.AutomaticFold = AutomaticFold.Click;

            FontName = "Consolas";
            FontSize = 10;
            LineNumberColumnWidth = 40;
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

        public bool AutoCompleteActive { get { return WinFormsControl.AutoCActive; } }
        public void InsertText(int position, string text) { WinFormsControl.InsertText(position, text); }
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

    }
}
