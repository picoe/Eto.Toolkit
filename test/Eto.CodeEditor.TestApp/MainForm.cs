using Eto.Drawing;
using Eto.Forms;

namespace Eto.CodeEditor.TestApp
{
    public class MainForm : Form
    {
        public MainForm()
        {
            Title = $"CodeEditor Test, Platform: {Platform.ID}";
            Menu = new MenuBar();
            ClientSize = new Size(400, 400);


            var editor = new CodeEditor(ProgrammingLanguage.CSharp);
            editor.Text =
@"// Just some sample code
for( int i=0; i<10; i++ )
{
  print(i);
}";

            editor.SetupIndicatorStyles();
            editor.AddErrorIndicator(13, 6);

            var btn = new Button { Text = "Font" };
            btn.Click += (s, e) =>
            {
                //MessageBox.Show(editor.GetLineText(1));
                //editor.FontName = "Wingdings";
                editor.ShowWhitespaceWithColor(Colors.Red);
            };
            Content = new TableLayout { Rows = { btn, editor } };
        }
    }
}
