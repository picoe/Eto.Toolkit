using System;
using Eto.Forms;
using Eto.Drawing;

namespace Eto.HtmlRenderer.TestApp.Desktop
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
            var platform = Eto.Platform.Detect;

            // test using ImageSharp
            //platform.LoadAssembly("Eto.HtmlRenderer.ImageSharp");

            new Application(platform).Run(new MainForm());
		}
	}
}
