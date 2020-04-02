using System.Linq;

namespace Eto.UnitTest.Filters
{
    public abstract class BaseFilter : ITestFilter
    {
        public bool IsExplicitMatch(ITest test) => Matches(test);

        public bool ChildCanMatch { get; set; } = true;
        public bool ParentCanMatch { get; set; } = true;

        public bool Pass(ITest test)
        {
            var matches = Matches(test);

            if (test.IsSuite)
            {
                if (ChildCanMatch)
                    matches |= test.GetChildren().Any(Matches);
            }
            else if (ParentCanMatch)
                matches |= test.GetParents().Any(ParentMatch);

            return matches;
        }

        protected bool ParentMatch(ITest test) => Matches(test, true);

        protected bool Matches(ITest test) => Matches(test, false);

        protected abstract bool Matches(ITest test, bool parent);
    }
}
