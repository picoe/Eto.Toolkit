using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eto.UnitTest.Runners
{
    public abstract class BaseTestRunner : ITestRunner, IDisposable
    {
        public abstract bool IsRunning { get; }

        public abstract ITest TestSuite { get; }

        public event EventHandler<UnitTestLogEventArgs> Log;
        public event EventHandler<UnitTestProgressEventArgs> Progress;
        public event EventHandler<UnitTestResultEventArgs> TestFinished;
        public event EventHandler<UnitTestTestEventArgs> TestStarted;
        public event EventHandler<EventArgs> IsRunningChanged;

        protected virtual void OnLog(UnitTestLogEventArgs e) => Log?.Invoke(this, e);
        protected virtual void OnProgress(UnitTestProgressEventArgs e) => Progress?.Invoke(this, e);
        protected virtual void OnTestFinished(UnitTestResultEventArgs e) => TestFinished?.Invoke(this, e);
        protected virtual void OnTestStarted(UnitTestTestEventArgs e) => TestStarted?.Invoke(this, e);
        protected virtual void OnIsRunningChanged(EventArgs e) => IsRunningChanged?.Invoke(this, e);

        public abstract Task<IEnumerable<string>> GetCategories(ITestFilter filter);
        public abstract Task<int> GetTestCount(ITestFilter filter);
        public abstract Task Load(ITestSource source);
        public abstract Task<ITestResult> RunAsync(ITestFilter filter);
        public abstract void StopTests();

        protected void WriteLog(string message) => OnLog(new UnitTestLogEventArgs(message));

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
