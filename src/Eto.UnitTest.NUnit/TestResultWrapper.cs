using System;
using System.Collections.Generic;
using System.Linq;
using nui = NUnit.Framework.Interfaces;

namespace Eto.UnitTest.NUnit
{
    class TestResultWrapper : ITestResult
    {
        nui.ITestResult _result;
        TestStatus? _status;
        ITest _test;

        public TestResultWrapper(nui.ITestResult result)
        {
            _result = result;
        }

        public int AssertCount => _result.AssertCount;

        public int FailCount => _result.FailCount;

        public int ErrorCount => 0;

        public int WarningCount => _result.WarningCount;

        public int PassCount => _result.PassCount;

        public int InconclusiveCount => _result.InconclusiveCount;

        public int SkipCount => _result.SkipCount;

        public TestStatus Status => _status ?? (_status = GetStatus()).Value;

        private TestStatus GetStatus()
        {
            switch (_result.ResultState.Status)
            {
                case nui.TestStatus.Failed:
                    return TestStatus.Failed;
                case nui.TestStatus.Inconclusive:
                    return TestStatus.Inconclusive;
                case nui.TestStatus.Passed:
                    return TestStatus.Passed;
                case nui.TestStatus.Skipped:
                    return TestStatus.Skipped;
                case nui.TestStatus.Warning:
                    return TestStatus.Warning;
                default:
                    throw new NotSupportedException();
            }
        }

        public ITest Test => _test ?? (_test = new TestWrapper(_result.Test));

        public IEnumerable<ITestResult> Children => _result.Children?.Select(r => new TestResultWrapper(r));

        public TimeSpan Duration => TimeSpan.FromSeconds(_result.Duration);

        public DateTime EndTime => _result.EndTime;

        public bool HasChildren => _result.HasChildren;

        public string Message => _result.Message;

        public string Name => _result.Name;

        public string Output => _result.Output;

        public DateTime StartTime => _result.StartTime;

        public object NativeObject => _result;

        public IEnumerable<ITestAssertion> Assersions
        {
            get
            {
                if (_result.AssertionResults.Count == 0)
                    return new[] {
                        new TestAssertion {
                            Message = _result.Message,
                            Status = (TestStatus)_result.ResultState.Status,
                            StackTrace = _result.StackTrace
                        }
                    };
                else
                    return _result.AssertionResults.Select(r => new TestAssertionWrapper(r));
            }
        }
    }
}
