using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eto.UnitTest.Runners
{
    public class MultipleTestRunner : ProgressTestRunner
    {
        ITestRunner _currentRunner;
        Queue<ITestRunner> _runnersToTest;
        ITestFilter _testFilter;

        List<ITestRunner> Runners { get; } = new List<ITestRunner>();

        public override async Task<IEnumerable<string>> GetCategories(ITestFilter filter)
        {
            var categoryList = await Task.WhenAll(Runners.Select(r => r.GetCategories(filter)));
            return categoryList.SelectMany(r => r).Distinct();
        }

        public override async Task<int> GetTestCount(ITestFilter filter)
        {
            var countList = await Task.WhenAll(Runners.Select(r => r.GetTestCount(filter)));
            return countList.Sum();
        }
        MultipleTest _testSuite;

        public override ITest TestSuite => _testSuite;

        public override void StopTests()
        {
            lock (this)
            {
                if (IsRunning)
                {
                    WriteLog("Stopping tests...");
                    _runnersToTest?.Clear();
                    _currentRunner?.StopTests();
                }
            }
        }

        public override async Task Load(ITestSource source)
        {
            var runnerType = TestRunnerType.Find(source);
            if (runnerType != null)
            {
                var runner = runnerType.CreateRunner();
                await runner.Load(source);
                Runners.Add(runner);
            }
            _testSuite = new MultipleTest(Runners.Select(r => r.TestSuite));
        }

        public void Add(ITestRunner runner)
        {
            Runners.Add(runner);
            _testSuite = new MultipleTest(Runners.Select(r => r.TestSuite));
        }

        public void Clear()
        {
            Runners.Clear();
            _testSuite = null;
        }

        protected override async Task<ITestResult> RunInternalAsync(ITestFilter filter)
        {
            var results = new MultipleTestResult(_testSuite);
            _testFilter = filter;
            _runnersToTest = new Queue<ITestRunner>(Runners.Where(r => _testFilter.Pass((r.TestSuite))));

            OnTestStarted(new UnitTestTestEventArgs(_testSuite));
            try
            {
                while (_runnersToTest.Count > 0)
                {
                    lock (this)
                    {
                        _currentRunner = _runnersToTest.Dequeue();
                        _currentRunner.Log += currentRunner_Log;
                        _currentRunner.Progress += currentRunner_Progress;
                        _currentRunner.TestStarted += currentRunner_TestStarted;
                        _currentRunner.TestFinished += currentRunner_TestFinished;
                    }
                    var result = await _currentRunner.RunAsync(filter);
                    results.Results.Add(result);

                    _currentRunner.Log -= currentRunner_Log;
                    _currentRunner.Progress -= currentRunner_Progress;
                    _currentRunner.TestStarted -= currentRunner_TestStarted;
                    _currentRunner.TestFinished -= currentRunner_TestFinished;
                }
                OnTestFinished(new UnitTestResultEventArgs(results));

            }
            finally
            {
                _currentRunner = null;
            }
            return results;

        }

        private void currentRunner_TestFinished(object sender, UnitTestResultEventArgs e)
        {
            OnTestFinished(e);
        }

        private void currentRunner_TestStarted(object sender, UnitTestTestEventArgs e)
        {
            OnTestStarted(e);
        }

        private void currentRunner_Progress(object sender, UnitTestProgressEventArgs e)
        {
            OnProgress(e);
        }

        private void currentRunner_Log(object sender, UnitTestLogEventArgs e)
        {
            //OnLog(e);
        }
    }
}
