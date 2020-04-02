namespace Eto.UnitTest.Filters
{
    public class NotFilter : ITestFilter
    {
        public ITestFilter Filter { get; set; }

        public NotFilter(ITestFilter filter)
        {
            Filter = filter;
        }

        public NotFilter()
        {
        }

        public bool IsExplicitMatch(ITest test) => Filter?.IsExplicitMatch(test) != true;

        public bool Pass(ITest test) => Filter?.Pass(test) != true;
    }
}
