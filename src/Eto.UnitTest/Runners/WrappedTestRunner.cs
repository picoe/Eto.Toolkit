using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eto.UnitTest.Runners
{
    public class WrappedTestRunner : BaseTestRunner
    {
        ITestRunner _inner;
        public override bool IsRunning => _inner.IsRunning;

        public override ITest TestSuite => _inner.TestSuite;

        public WrappedTestRunner(ITestRunner inner)
        {
            _inner = inner;
            _inner.Log += _inner_Log;
            _inner.Progress += _inner_Progress;
            _inner.IsRunningChanged += _inner_IsRunningChanged;
            _inner.TestFinished += _inner_TestFinished;
            _inner.TestStarted += _inner_TestStarted;
        }

        private void _inner_TestStarted(object sender, UnitTestTestEventArgs e) => OnTestStarted(e);
        private void _inner_TestFinished(object sender, UnitTestResultEventArgs e) => OnTestFinished(e);
        private void _inner_IsRunningChanged(object sender, EventArgs e) => OnIsRunningChanged(e);
        private void _inner_Progress(object sender, UnitTestProgressEventArgs e) => OnProgress(e);
        private void _inner_Log(object sender, UnitTestLogEventArgs e) => OnLog(e);

        public override Task<IEnumerable<string>> GetCategories(ITestFilter filter) => _inner.GetCategories(filter);
        public override Task<int> GetTestCount(ITestFilter filter) => _inner.GetTestCount(filter);
        public override Task Load(ITestSource source) => _inner.Load(source);
        public override Task<ITestResult> RunAsync(ITestFilter filter) => _inner.RunAsync(filter);
        public override void StopTests() => _inner.StopTests();

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && _inner is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
