using System.Collections.Generic;
using System.Linq;

namespace Eto.UnitTest.Filters
{
    public class CategoryFilter : BaseFilter
    {
        public List<string> Categories { get; }

        public CategoryFilter()
        {
            Categories = new List<string>();
        }

        public CategoryFilter(IEnumerable<string> categories)
        {
            Categories = categories.ToList();
        }

        public bool MatchAll { get; set; }

        public bool AllowNone { get; set; }

        protected override bool Matches(ITest test, bool parent)
        {
            var categories = test.Categories.ToList();
            if (categories == null || categories.Count == 0)
                return Categories.Count == 0;

            return MatchAll ? Categories.All(categories.Contains) : Categories.Any(categories.Contains);
        }
    }
}
