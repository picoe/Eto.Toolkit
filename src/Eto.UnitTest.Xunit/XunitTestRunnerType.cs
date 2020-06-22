using Eto.UnitTest;

[assembly: TestRunnerType(typeof(Eto.UnitTest.Xunit.XunitTestRunnerType))]


namespace Eto.UnitTest.Xunit
{
    public class XunitTestRunnerType : TestRunnerType<XunitTestRunner>
    {
        public override string Name => "Xunit";

        protected override string[] RequiredReferences => new[] { "xunit.core" };

        public static void Register() => TestRunnerType.Add(new XunitTestRunnerType());
    }
}
