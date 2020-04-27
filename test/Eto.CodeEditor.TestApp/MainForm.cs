using System;
using Eto.Drawing;
using Eto.Forms;

namespace Eto.CodeEditor.TestApp
{
    public class MainForm : Form
    {
        static MainForm()
        {
            Eto.UnitTest.NUnit.NUnitTestRunnerType.Register();
        }

        public MainForm()
        {
            Title = $"CodeEditor Test, Platform: {Platform.ID}";
            ClientSize = new Size(1400, 400);


            var editor = new CodeEditor(ProgrammingLanguage.CSharp);
            editor.Text =
@"// Just some sample code
for( int i=0; i<10; i++ )
{
  print(i);
}";

            editor.SetupIndicatorStyles();
            editor.AddErrorIndicator(13, 6);

            Action<Font, string> pp = (f, pfx) => MessageBox.Show($"{pfx}: name: {editor.Font.FamilyName}, size: {editor.FontSize}");

            var btn = new Button { Text = "Font" };
            btn.Click += (s, e) =>
            {
                var originalFont = editor.Font ?? SystemFonts.Default();
                pp(originalFont, "first");
                var fd = new Eto.Forms.FontDialog { Font = originalFont };
                fd.FontChanged += (ss, ee) =>
                {
                    editor.Font = fd.Font;
                    pp(editor.Font, "FontChanged");
                };
                var r = fd.ShowDialog(this);
                editor.Font = (r == DialogResult.Ok || r == DialogResult.Yes)
                  ? fd.Font
                  : originalFont;

                pp(fd.Font, "fd");
                pp(editor.Font, "editor");
                //editor.ShowWhitespaceWithColor(Colors.Red);
                //editor.Text = $"name: {editor.FontName}, size: {editor.FontSize}";
            };
            

            var tests = new Eto.UnitTest.UI.UnitTestPanel(true);

            this.LoadComplete += async (s,e) =>
            {
                var testSource = new UnitTest.TestSource(System.Reflection.Assembly.GetExecutingAssembly());
                var mtr = new Eto.UnitTest.Runners.MultipleTestRunner();
                await mtr.Load(testSource);
                tests.Runner = new UnitTest.Runners.LoggingTestRunner(mtr);
                CodeEditorTests.editor = editor;
            };

            var splitter = new Splitter
            {
                Panel1 = tests,
                Panel2 = editor
            };
            Content = new TableLayout { Rows = { splitter } };
        }
    }
}
