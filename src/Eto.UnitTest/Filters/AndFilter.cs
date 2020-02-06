using System.Collections.Generic;
using System.Linq;

namespace Eto.UnitTest
{
    class AndFilter : ITestFilter
    {
        public List<ITestFilter> Filters { get; }

        public AndFilter()
        {
            Filters = new List<ITestFilter>();
        }

        public AndFilter(params ITestFilter[] filters)
        {
            Filters = filters.ToList();
        }

        public AndFilter(IEnumerable<ITestFilter> filters)
        {
            Filters = filters.ToList();
        }

        public bool IsExplicitMatch(ITest test)
        {
            for (int i = 0; i < Filters.Count; i++)
            {
                if (!Filters[i].IsExplicitMatch(test))
                    return false;
            }
            return true;
        }

        public bool Pass(ITest test)
        {
            for (int i = 0; i < Filters.Count; i++)
            {
                if (!Filters[i].Pass(test))
                    return false;
            }
            return true;
        }
    }
}
