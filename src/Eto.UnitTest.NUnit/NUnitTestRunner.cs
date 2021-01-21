using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Eto.UnitTest;
using Eto.UnitTest.Runners;
using nua = NUnit.Framework.Api;
using nui = NUnit.Framework.Interfaces;
using nufint = NUnit.Framework.Internal;

[assembly: TestRunnerType(typeof(Eto.UnitTest.NUnit.NUnitTestRunnerType))]

namespace Eto.UnitTest.NUnit
{
    public class NUnitTestRunnerType : TestRunnerType<NUnitTestRunner>
    {
        public override string Name => "NUnit";

        public static void Register() => TestRunnerType.Add(new NUnitTestRunnerType());

        protected override string[] RequiredReferences => new[] { "nunit.framework" };
    }

    public class NUnitTestRunner : ProgressTestRunner, nui.ITestListener
    {
        TaskCompletionSource<ITestResult> _tcs;
        nua.ITestAssemblyBuilder _builder = new nua.DefaultTestAssemblyBuilder();
        nua.ITestAssemblyRunner _runner;

        public nua.ITestAssemblyRunner NUnitRunner => _runner;

        public override Task Load(ITestSource source)
        {
            var settings = new Dictionary<string, object>();

            _runner = new nua.NUnitTestAssemblyRunner(_builder);
            _runner.Load(source.Assembly, settings);
            return Task.CompletedTask;
        }

        public override ITest TestSuite => new TestWrapper(_runner.ExploreTests(nufint.TestFilter.Empty));

        public override Task<IEnumerable<string>> GetCategories(ITestFilter filter)
        {
            return Task.FromResult(TestSuite.GetChildren().Where(filter.Pass).SelectMany(r => r.Categories).Distinct());
        }

        public override Task<int> GetTestCount(ITestFilter filter)
        {
            return Task.FromResult(TestSuite.GetChildren().Count(r => !r.IsSuite && filter.Pass(r)));
        }

        protected override Task<ITestResult> RunInternalAsync(ITestFilter filter)
        {
            _tcs = new TaskCompletionSource<ITestResult>();
            new nufint.TestExecutionContext.AdhocContext().EstablishExecutionEnvironment();
            _runner.RunAsync(this, new TestFilterWrapper(filter));
            return _tcs.Task;
        }

        public override void StopTests()
        {
            lock (this)
            {
                _runner.StopRun(true);
                _tcs.SetResult(new TestResultWrapper(_runner.Result));
            }
        }

        void nui.ITestListener.TestStarted(nui.ITest test)
        {
            OnTestStarted(new UnitTestTestEventArgs(new TestWrapper(test)));
        }

        void nui.ITestListener.TestFinished(nui.ITestResult result)
        {
            var wrappedResult = new TestResultWrapper(result);
            OnTestFinished(new UnitTestResultEventArgs(wrappedResult));

            if (result.Test is nufint.TestAssembly)
            {
                _tcs.SetResult(new TestResultWrapper(_runner.Result));
            }
        }

        void nui.ITestListener.TestOutput(nui.TestOutput output)
        {
            WriteLog(output.ToString());
        }

        void nui.ITestListener.SendMessage(nui.TestMessage message)
        {
            WriteLog(message.ToString());
        }

    }
}
