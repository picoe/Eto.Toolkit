namespace Eto.UnitTest
{
    class EmptyFilter : ITestFilter
    {
        public bool IsExplicitMatch(ITest test) => true;

        public bool Pass(ITest test) => true;
    }
}
