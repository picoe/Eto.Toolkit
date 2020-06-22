using System;
using System.Threading.Tasks;

namespace Eto.UnitTest.Runners
{
    public abstract class ProgressTestRunner : BaseTestRunner, IDisposable
    {
        UnitTestProgressEventArgs _progressArgs;

        bool _isRunning;
        public override bool IsRunning => _isRunning;

        void SetIsRunning(bool value)
        {
            if (_isRunning != value)
            {
                _isRunning = value;
                OnIsRunningChanged(EventArgs.Empty);
            }
        }

        protected override void OnTestFinished(UnitTestResultEventArgs e)
        {
            base.OnTestFinished(e);
            var result = e.Result;

            if (!result.Test.IsSuite)
            {
                _progressArgs.AddResult(result);
                OnProgress(_progressArgs);
            }
        }

        public override sealed async Task<ITestResult> RunAsync(ITestFilter filter)
        {
            SetIsRunning(true);
            try
            {
                var testCount = await GetTestCount(filter);
                _progressArgs = new UnitTestProgressEventArgs(testCount);
                return await RunInternalAsync(filter);
            }
            finally
            {
                SetIsRunning(false);
            }
        }

        protected abstract Task<ITestResult> RunInternalAsync(ITestFilter filter);
    }
}
