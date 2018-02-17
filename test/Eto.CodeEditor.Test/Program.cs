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


			Content = new CodeEditor { Text =
        @"// Just some sample code
for( int i=0; i<10; i++ )
{
  print(i);
}"
      };
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
