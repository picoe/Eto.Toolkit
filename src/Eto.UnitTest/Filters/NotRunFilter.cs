using System;
using System.Linq;

namespace Eto.UnitTest.Filters
{
    public class NotRunFilter : ITestFilter
    {
        Func<ITest, ITestResult> LookupResult { get; }

        public NotRunFilter(Func<ITest, ITestResult> lookupResult)
        {
            LookupResult = lookupResult;
        }

        public override string ToString() => "Not Run";

        public bool Pass(ITest test)
        {
            if (test.IsSuite)
                return test.GetChildren(false).Any(Pass);
            return IsExplicitMatch(test);
        }

        public bool IsExplicitMatch(ITest test) => LookupResult(test) == null;

    }
}
