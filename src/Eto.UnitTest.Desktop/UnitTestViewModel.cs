using System.ComponentModel;
using System.Reflection;
using System.Windows.Input;
using Eto.Forms;
using Eto.UnitTest.UI;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System;
using Eto.UnitTest.Runners;

namespace Eto.UnitTest.App
{
    class UnitTestViewModel : INotifyPropertyChanged
    {
        ITestRunner _runner;
        MultipleTestRunner _multipleTestRunner;

        public ICommand OpenCommand => new RelayCommand<Control>(OpenRunner);

        public ICommand ClearCommand => new RelayCommand(ClearRunner);

        public bool ShowOnlyFailed
        {
            get => (_runner as LoggingTestRunner)?.ShowOnlyFailed ?? false;
            set
            {
                if (_runner is LoggingTestRunner runner)
                    runner.ShowOnlyFailed = value;
                OnPropertyChanged();
            }
        }

        public bool ShowOutput
        {
            get => (_runner as LoggingTestRunner)?.ShowOutput ?? false;
            set
            {
                if (_runner is LoggingTestRunner runner)
                    runner.ShowOutput = value;
                OnPropertyChanged();
            }
        }

    private void ClearRunner()
        {
            _multipleTestRunner.Clear();
            Runner = null;
        }

        public ITestRunner Runner
        {
            get => _runner;
            set
            {
                _runner = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowOutput));
                OnPropertyChanged(nameof(ShowOnlyFailed));
            }
        }

        void OnPropertyChanged([CallerMemberName] string memberName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OpenRunner(Control parent)
        {
            var open = new OpenFileDialog();
            open.CheckFileExists = true;
            open.Filters.Add(new FileFilter("Assembly", ".dll"));
            open.Filters.Add(new FileFilter("Executable", ".exe"));
            open.CurrentFilterIndex = 0;
            if (open.ShowDialog(parent) == DialogResult.Ok)
            {
                OpenFile(parent, open.FileName);
            }
        }

        public void OpenFile(Control parent, string fileName)
        {
            Application.Instance.Invoke(async () =>
            {
                var source = new TestSource(fileName);
                if (_multipleTestRunner == null)
                    _multipleTestRunner = new MultipleTestRunner();
                try
                {
                    await _multipleTestRunner.Load(source);

                    Runner = new LoggingTestRunner(_multipleTestRunner);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(parent, $"Error loading assembly: {ex.Message}", MessageBoxType.Information);
                }
            });
        }
    }
}
