using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Forms;
using Eto;
using Eto.CodeEditor;
using Eto.CodeEditor.XamMac2;
using AppKit;

[assembly: ExportHandler(typeof(CodeEditor), typeof(CodeEditorHandler))]

namespace Eto.CodeEditor.XamMac2
{
	public class CodeEditorHandler : Eto.Mac.Forms.MacView<NSText, CodeEditor, CodeEditor.ICallback>, CodeEditor.IHandler
    {
		public CodeEditorHandler()
		{
			Control = new NSText();
		}

		public string Text
		{
			get => Control.Value;
			set => Control.Value = value ?? string.Empty;
		}

		public override NSView ContainerControl => Control;

		public override bool Enabled { get; set; }
	}
}
