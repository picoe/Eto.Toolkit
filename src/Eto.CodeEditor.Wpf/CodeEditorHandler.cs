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

		public string Text
		{
			get => WinFormsControl.Text;
			set => WinFormsControl.Text = value;
		}

        void SetupTheme()
        {
            // just style things enough that you can tell you're working in a code editor

            WinFormsControl.Lexer = ScintillaNET.Lexer.Cpp;
            WinFormsControl.SetKeywords(0, "abstract as base break case catch checked continue default delegate do else event explicit extern false finally fixed for foreach goto if implicit in interface internal is lock namespace new null object operator out override params private protected public readonly ref return sealed sizeof stackalloc switch this throw true try typeof unchecked unsafe using virtual while");
            WinFormsControl.SetKeywords(1, "bool byte char class const decimal double enum float int long sbyte short static string struct uint ulong ushort void");
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.Comment].ForeColor = System.Drawing.Color.Gray;
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.CommentLine].ForeColor = System.Drawing.Color.Gray;
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.CommentDoc].ForeColor = System.Drawing.Color.Gray;
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.Number].ForeColor = System.Drawing.Color.Black;
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.String].ForeColor = System.Drawing.Color.Red;
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.Character].ForeColor = System.Drawing.Color.Black;
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.Preprocessor].ForeColor = System.Drawing.Color.Black;
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.Operator].ForeColor = System.Drawing.Color.Black;
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.Regex].ForeColor = System.Drawing.Color.Black;
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.CommentLineDoc].ForeColor = System.Drawing.Color.Black;
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.Word].ForeColor = System.Drawing.Color.Blue;
            WinFormsControl.Styles[ScintillaNET.Style.Cpp.Word2].ForeColor = System.Drawing.Color.CadetBlue;

            WinFormsControl.Styles[ScintillaNET.Style.Default].Font = "Consolas";
            WinFormsControl.Styles[ScintillaNET.Style.Default].Size = 10;
            // Show line numbers
            WinFormsControl.Margins[0].Width = 30;

            WinFormsControl.Styles[ScintillaNET.Style.LineNumber].BackColor = System.Drawing.Color.White;
            WinFormsControl.Styles[ScintillaNET.Style.LineNumber].ForeColor = System.Drawing.Color.CadetBlue;

        }
    }
}
