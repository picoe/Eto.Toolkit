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


      var editor = new CodeEditor
      {
        Text =
        @"// Just some sample code
for( int i=0; i<10; i++ )
{
  print(i);
}"
      };

      editor.Lexer = Lexer.Cpp;
      editor.SetKeywords(0, "abstract as base break case catch checked continue default delegate do else event explicit extern false finally fixed for foreach goto if implicit in interface internal is lock namespace new null object operator out override params private protected public readonly ref return sealed sizeof stackalloc switch this throw true try typeof unchecked unsafe using virtual while");
      editor.SetKeywords(1, "bool byte char class const decimal double enum float int long sbyte short static string struct uint ulong ushort void");
      editor.SetColor(Section.Comment, Colors.Gray, Colors.Transparent);
      editor.SetColor(Section.Keyword, Colors.SeaGreen, Colors.Transparent);
      if (Eto.Forms.Application.Instance.Platform.IsMac)
      {
        editor.FontName = "Menlo";
        editor.FontSize = 14;
      }
      Content = editor;
		}
	}

    public class Program
    {
		[STAThread]
        public static void Main(string[] args)
        {
			new Application(Eto.Platform.Detect).Run(new MainForm());
        }
    }
}
