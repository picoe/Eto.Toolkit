using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto;
using Eto.Forms;
using Eto.CodeEditor;
using Eto.CodeEditor.Wpf;

[assembly: ExportHandler(typeof(CodeEditor), typeof(CodeEditorHandler))]

namespace Eto.CodeEditor.Wpf
{
    public class CodeEditorHandler : Eto.Wpf.Forms.WindowsFormsHostHandler<ScintillaNET.Scintilla, CodeEditor, CodeEditor.ICallback>, CodeEditor.IHandler
    {
        public CodeEditorHandler()
        {
            WinFormsControl = new ScintillaNET.Scintilla();
            SetupTheme();
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

        public event EventHandler TextChanged
        {
            add
            {
                WinFormsControl.TextChanged += value;
            }
            remove
            {
                WinFormsControl.TextChanged -= value;
            }
        }

        void SetupTheme()
        {
            // just style things enough that you can tell you're working in a code editor

            //WinFormsControl.Lexer = ScintillaNET.Lexer.Cpp;
            //WinFormsControl.SetKeywords(0, "abstract as base break case catch checked continue default delegate do else event explicit extern false finally fixed for foreach goto if implicit in interface internal is lock namespace new null object operator out override params private protected public readonly ref return sealed sizeof stackalloc switch this throw true try typeof unchecked unsafe using virtual while");
            //WinFormsControl.SetKeywords(1, "bool byte char class const decimal double enum float int long sbyte short static string struct uint ulong ushort void");
            //WinFormsControl.Styles[ScintillaNET.Style.Cpp.Comment].ForeColor = System.Drawing.Color.Gray;
            //WinFormsControl.Styles[ScintillaNET.Style.Cpp.CommentLine].ForeColor = System.Drawing.Color.Gray;
            //WinFormsControl.Styles[ScintillaNET.Style.Cpp.CommentDoc].ForeColor = System.Drawing.Color.Gray;
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.Number].ForeColor = System.Drawing.Color.Black;
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.String].ForeColor = System.Drawing.Color.Red;
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.Character].ForeColor = System.Drawing.Color.Black;
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.Preprocessor].ForeColor = System.Drawing.Color.Black;
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.Operator].ForeColor = System.Drawing.Color.Black;
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.Regex].ForeColor = System.Drawing.Color.Black;
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.CommentLineDoc].ForeColor = System.Drawing.Color.Black;
            //WinFormsControl.Styles[ScintillaNET.Style.Cpp.Word].ForeColor = System.Drawing.Color.Blue;
            //WinFormsControl.Styles[ScintillaNET.Style.Cpp.Word2].ForeColor = System.Drawing.Color.CadetBlue;

            //WinFormsControl.Styles[ScintillaNET.Style.Default].Font = "Consolas";
            //WinFormsControl.Styles[ScintillaNET.Style.Default].Size = 10;
            // Show line numbers
            //WinFormsControl.Margins[0].Width = 60;

            //WinFormsControl.Styles[ScintillaNET.Style.LineNumber].BackColor = System.Drawing.Color.White;
            //WinFormsControl.Styles[ScintillaNET.Style.LineNumber].ForeColor = System.Drawing.Color.CadetBlue;

            FontName = "Consolas";
            FontSize = 10;
            LineNumberColumnWidth = 40;
        }
    }
}
