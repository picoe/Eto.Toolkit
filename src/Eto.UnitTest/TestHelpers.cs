using System.Collections.Generic;

namespace Eto.UnitTest
{
    public static class TestHelpers
    {
        public static IEnumerable<ITest> GetChildren(this ITest test) => GetChildren(test, true);

        public static IEnumerable<ITest> GetChildren(this ITest test, bool recursive)
        {
            if (test.HasChildren)
            {
                foreach (var child in test.Tests)
                {
                    yield return child;
                    if (recursive)
                    {
                        foreach (var childTest in GetChildren(child, recursive))
                            yield return childTest;
                    }
                }
            }
        }

        public static IEnumerable<ITest> GetParents(this ITest test)
        {
            while (test != null)
            {
                yield return test;
                test = test.Parent;
            }
        }
    }
}
