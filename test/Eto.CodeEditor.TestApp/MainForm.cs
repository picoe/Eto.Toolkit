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


            var editor = new CodeEditor(ProgrammingLanguage.CSharp)
            {
                Text =
              @"// Just some sample code
for( int i=0; i<10; i++ )
{
  print(i);
}"
            };
            editor.TextInput += Editor_TextInput;

            Content = editor;
        }

        private void Editor_TextInput(object sender, TextInputEventArgs e)
        {
            int i = 0;
        }
    }
}
