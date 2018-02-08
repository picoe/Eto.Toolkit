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
	public class CodeEditorHandler : Eto.Wpf.Forms.WindowsFormsHostHandler<System.Windows.Forms.TextBox, CodeEditor, CodeEditor.ICallback>, CodeEditor.IHandler
    {
		public CodeEditorHandler()
		{
			WinFormsControl = new System.Windows.Forms.TextBox { Multiline = true };
		}

		public string Text
		{
			get => WinFormsControl.Text;
			set => WinFormsControl.Text = value;
		}
    }
}
