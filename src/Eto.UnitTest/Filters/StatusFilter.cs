using System;
using System.Linq;

namespace Eto.UnitTest
{
    class StatusFilter : ITestFilter
    {
        Func<ITest, ITestResult> _lookupResult;

        public string Name => Status.ToString();

        public TestStatus Status { get; }

        public StatusFilter(Func<ITest, ITestResult> lookupResult, TestStatus status)
        {
            Status = status;
            _lookupResult = lookupResult;
        }

        public override string ToString() => Name;

        public bool Pass(ITest test)
        {
            if (test.IsSuite)
                return test.GetChildren(false).Any(Pass);
            return IsExplicitMatch(test);
        }

        public bool IsExplicitMatch(ITest test) => _lookupResult(test)?.Status == Status;

    }
}
