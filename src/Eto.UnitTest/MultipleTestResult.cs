using System;
using System.Collections.Generic;
using System.Linq;

namespace Eto.UnitTest
{
    public class MultipleTestResult : ITestResult
    {
        public List<ITestResult> Results { get; private set; }

        public MultipleTestResult(ITest test)
        {
            Results = new List<ITestResult>();
            Test = test;
        }

        public int AssertCount => Results.Sum(r => r.AssertCount);

        public IEnumerable<ITestResult> Children => Results.SelectMany(r => r.Children);

        public TimeSpan Duration => TimeSpan.FromTicks(Results.Sum(r => r.Duration.Ticks));

        public DateTime EndTime => Results.Max(r => r.EndTime);

        public int FailCount => Results.Sum(r => r.FailCount);

        //public int ErrorCount => Results.Sum(r => r.ErrorCount);

        public string FullName => string.Empty;

        public bool HasChildren => Results.Any(r => r.HasChildren);

        public int InconclusiveCount => Results.Sum(r => r.InconclusiveCount);

        public string Message => string.Join("\n", Results.Select(r => r.Message));

        public string Name => string.Join(", ", Results.Select(r => r.Name));

        public string Output => string.Join("\n", Results.Select(r => r.Output));

        public int PassCount => Results.Sum(r => r.PassCount);

        public int SkipCount => Results.Sum(r => r.SkipCount);

        public DateTime StartTime => Results.Min(r => r.StartTime);

        public ITest Test { get; }

        public int WarningCount => Results.Sum(r => r.WarningCount);

        public TestStatus Status => Results.Max(r => r.Status);

        public object NativeObject => null;

        public IEnumerable<ITestAssertion> Assersions => Results.SelectMany(r => r.Assersions);
    }
}
