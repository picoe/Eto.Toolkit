namespace Eto.UnitTest
{
    public interface ITestFilter
    {
        bool IsExplicitMatch(ITest test);
        bool Pass(ITest test);
    }
}
