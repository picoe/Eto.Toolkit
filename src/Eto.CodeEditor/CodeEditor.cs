using System;
using System.Reflection;
using Eto.Forms;

namespace Eto.CodeEditor
{
	[Handler(typeof(IHandler))]
	public class CodeEditor : Control
	{
		new IHandler Handler => (IHandler)base.Handler;

		public string Text
		{
			get => Handler.Text;
			set => Handler.Text = value;
		}

		public new interface IHandler : Control.IHandler
		{
			string Text { get; set; }
		}
	}
}
