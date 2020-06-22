using System;
using Eto.CodeEditor.TestApp;
using Eto.Drawing;
using Eto.Forms;

namespace Eto.CodeEditor.Test
{

    public class Program
    {
		[STAThread]
        public static void Main(string[] args)
        {
			new Application(Eto.Platform.Detect).Run(new MainForm());
        }
    }
}
