using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eto.Forms;
using Eto.UnitTest.UI;

namespace Eto.UnitTest.App
{

    public class MainForm : Form
    {
        public MainForm(IReadOnlyCollection<FileInfo> assemblies)
        {
            var viewModel = new UnitTestViewModel();
            DataContext = viewModel;

            Title = "Eto Unit Test";

            NUnit.NUnitTestRunnerType.Register();
            Xunit.XunitTestRunnerType.Register();

            var unitTestPanel = new UnitTestPanel(true);
            unitTestPanel.BindDataContext(c => c.Runner, (UnitTestViewModel m) => m.Runner);

            Content = unitTestPanel;

            CreateMenu();

            if (assemblies?.Count > 0)
            {
                Application.Instance.AsyncInvoke(() =>
                {
                    foreach (var assembly in assemblies)
                    {
                        viewModel.OpenFile(this, assembly.FullName);
                    }
                });
            }

        }

        void CreateMenu()
        {
            var openItem = new ButtonMenuItem { Text = "Open...", Shortcut = Application.Instance.CommonModifier | Keys.O };
            openItem.BindDataContext(c => c.Command, (UnitTestViewModel m) => m.OpenCommand);
            openItem.CommandParameter = this;

            var clear = new ButtonMenuItem { Text = "Close tests" };
            clear.BindDataContext(c => c.Command, (UnitTestViewModel m) => m.ClearCommand);

            var showOutput = new CheckMenuItem { Text = "Show output" };
            showOutput.BindDataContext(c => c.Checked, (UnitTestViewModel m) => m.ShowOutput);

            var showOnlyFailed = new CheckMenuItem { Text = "Show only failed tests" };
            showOnlyFailed.BindDataContext(c => c.Checked, (UnitTestViewModel m) => m.ShowOnlyFailed);

            Menu = new MenuBar
            {
                Items =
                {
                    new ButtonMenuItem { Text = "&File", Items = { openItem, clear } },
                    new ButtonMenuItem { Text = "&View", Items = { showOutput, showOnlyFailed } }
                }
            };
        }
    }
}
