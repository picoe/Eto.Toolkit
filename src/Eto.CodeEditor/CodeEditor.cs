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

    public void SetKeywords(int set, string keywords)
    {
      Handler.SetKeywords(set, keywords);
    }

		public new interface IHandler : Control.IHandler
		{
			string Text { get; set; }
      void SetKeywords(int set, string keywords);
		}
	}
}
