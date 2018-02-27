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
using ScintillaNET;
using Foundation;
using System.IO;
using ObjCRuntime;

[assembly: ExportHandler(typeof(CodeEditor), typeof(CodeEditorHandler))]

namespace Eto.CodeEditor.XamMac2
{
	public class CodeEditorHandler : Eto.Mac.Forms.MacView<ScintillaView, CodeEditor, CodeEditor.ICallback>, CodeEditor.IHandler
    {
		public CodeEditorHandler()
		{
			Control = new ScintillaView();
		}

		public string Text
		{
			get => null;
			set
			{
				//Control.Value = value ?? string.Empty;
			}
		}

		public override NSView ContainerControl => Control;

		public override bool Enabled { get; set; }
	}
}
