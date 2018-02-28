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
using System.Runtime.InteropServices;

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
			get => Control.Text;
			set
			{
				Control.Text = value ?? string.Empty;
			}
		}

		public override NSView ContainerControl => Control;

		public override bool Enabled { get; set; }

    public void SetKeywords(int set, string keywords)
    {
      Control.SetKeywords(set, keywords);
    }
	}
}
