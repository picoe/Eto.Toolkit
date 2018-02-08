using System;
using Eto.Drawing;
using Eto.Forms;

namespace Eto.CodeEditor.Test
{
	public class MainForm : Form
	{
		public MainForm()
		{
			Title = $"CodeEditor Test, Platform: {Platform.ID}";
			Menu = new MenuBar();
			ClientSize = new Size(400, 400);


			Content = new CodeEditor { Text = "Some code" };
		}
	}

    class Program
    {
		[STAThread]
        static void Main(string[] args)
        {
			new Application(Eto.Platform.Detect).Run(new MainForm());
        }
    }
}
