using System.Collections.Generic;
using System.Linq;

namespace Eto.UnitTest.Filters
{
    public class OrFilter : ITestFilter
    {
        public List<ITestFilter> Filters { get; }

        public OrFilter()
        {
            Filters = new List<ITestFilter>();
        }

        public OrFilter(params ITestFilter[] filters)
        {
            Filters = filters.ToList();
        }

        public OrFilter(IEnumerable<ITestFilter> filters)
        {
            Filters = filters.ToList();
        }

        public bool IsExplicitMatch(ITest test)
        {
            for (int i = 0; i < Filters.Count; i++)
            {
                if (Filters[i].IsExplicitMatch(test))
                    return true;
            }
            return false;
        }

        public bool Pass(ITest test)
        {
            for (int i = 0; i < Filters.Count; i++)
            {
                if (Filters[i].Pass(test))
                    return true;
            }
            return false;
        }

    }
}
