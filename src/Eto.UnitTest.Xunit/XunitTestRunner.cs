using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using xua = Xunit.Abstractions;
using xu = Xunit;
using Eto.UnitTest;
using System.Linq;
using System.Reflection;
using Eto.Forms;
using Eto.UnitTest.Runners;

namespace Eto.UnitTest.Xunit
{
    public class XunitTestRunner : ProgressTestRunner, xu.IMessageSinkWithTypes
    {
        xu.XunitFrontController controller;
        TaskCompletionSource<bool> _tcs;
        TaskCompletionSource<ITestResult> _tcsRun;
        TaskCompletionSource<int> _tcsFilter;
        TestCollectionWrapper _testWrapper;
        ITestSource _source;
        Assembly _assembly;
        ITestFilter _filter;
        bool _cancel;
        HashSet<TestSuiteWrapper> _testSuiteStarted;
        Dictionary<TestSuiteWrapper, TestStatus> _testSuiteStatus;

        List<xua.ITestCase> _testsToRun = new List<xua.ITestCase>();

        public override ITest TestSuite => _testWrapper;

        public override Task<IEnumerable<string>> GetCategories(ITestFilter filter)
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }

        public override Task<int> GetTestCount(ITestFilter filter)
        {
            _tcsFilter = new TaskCompletionSource<int>();
            _filter = filter;
            messageHandler = FilterTestCases;
            _testsToRun.Clear();
            var discoveryOptions = xu.TestFrameworkOptions.ForDiscovery();
            controller.Find(true, this, discoveryOptions);
            return _tcsFilter.Task;
        }

        public override Task Load(ITestSource source)
        {
            _source = source;
            _tcs = new TaskCompletionSource<bool>();
            controller = new xu.XunitFrontController(xu.AppDomainSupport.Denied, source.Assembly.Location, diagnosticMessageSink: xu.MessageSinkAdapter.Wrap(this));
            _assembly = source.Assembly;
            _testWrapper = new TestCollectionWrapper(_assembly);

            var discoveryOptions = xu.TestFrameworkOptions.ForDiscovery();
            messageHandler = DiscoverMessageHandler;
            controller.Find(true, this, discoveryOptions);
            return _tcs.Task;
        }

        void DiscoverMessageHandler(xua.IMessageSinkMessage message)
        {
            try
            {
                if (message is xua.ITestCaseDiscoveryMessage testCaseDiscovery)
                {
                    Wrap(testCaseDiscovery.TestCase, true);
                }
                else if (message is xua.IDiscoveryCompleteMessage discoveryComplete)
                {
                    _tcs.SetResult(true);
                }
                else if (message is xua.IErrorMessage errorMessage)
                {
                    Console.WriteLine($"Error ocurred {errorMessage.ToString()}");
                }
                else
                {
                    Console.WriteLine($"Unhandled message of type {message.GetType()}");
                }
            }
            catch (Exception ex)
            {
                _tcs.SetException(ex);
            }
        }

        public override void StopTests()
        {
            _cancel = true;
        }

        Action<xua.IMessageSinkMessage> messageHandler;

        protected override async Task<ITestResult> RunInternalAsync(ITestFilter filter)
        {
            _tcsRun = new TaskCompletionSource<ITestResult>();

            var config = new xu.TestAssemblyConfiguration();

            _cancel = false;
            _testsToRun.Clear();
            _testSuiteStarted = new HashSet<TestSuiteWrapper>();
            _testSuiteStatus = new Dictionary<TestSuiteWrapper, TestStatus>();
            messageHandler = FilterTestCases;
            _filter = filter;
            _tcsFilter = new TaskCompletionSource<int>();
            controller.Find(false, this, xu.TestFrameworkOptions.ForDiscovery(config));
            await _tcsFilter.Task;

            messageHandler = RunMessageHandler;
            controller.RunTests(_testsToRun, this, xu.TestFrameworkOptions.ForExecution(config));

            return await _tcsRun.Task;
        }

        private void FilterTestCases(xua.IMessageSinkMessage message)
        {
            try
            {
                if (message is xua.ITestCaseDiscoveryMessage testDiscovered)
                {
                    if (_filter?.Pass(Wrap(testDiscovered.TestCase)) != false)
                        _testsToRun.Add(testDiscovered.TestCase);
                }
                else if (message is xua.IDiscoveryCompleteMessage discoveryComplete)
                {
                    _tcsFilter.SetResult(_testsToRun.Count);
                }
                else if (message is xua.IErrorMessage errorMessage)
                {
                    Console.WriteLine($"Error ocurred {errorMessage.ToString()}");
                }
                else
                {
                    Console.WriteLine($"Unhandled message of type {message.GetType()}");
                }
            }
            catch (Exception ex)
            {
                _tcsFilter?.SetException(ex);
                _tcsRun?.SetException(ex);
                _tcs?.SetException(ex);
            }
        }


        TestCaseWrapper Wrap(xua.ITestCase test, bool addToSuite = false)
        {
            var suite = _testWrapper.GetSuite(test.DisplayName, false);
            var testWrapper = new TestCaseWrapper(test) { Parent = suite };
            if (addToSuite)
                suite.Children.Add(testWrapper);
            return testWrapper;
        }

        TestSuiteWrapper Wrap(xua.ITestClass test)
        {
            return _testWrapper.GetSuite(test.Class.Name, true);
        }

        void SetStatus(TestSuiteWrapper suite, TestStatus status)
        {
            if (suite == null)
                return;
            if (_testSuiteStatus.TryGetValue(suite, out var currentStatus))
            {
                status = (TestStatus)Math.Max((int)currentStatus, (int)status);
            }
            _testSuiteStatus[suite] = status;
            if (_testSuiteStarted.Contains(suite) && suite.RunningTests.Count == 0)
            {
                _testSuiteStarted.Remove(suite);
                var parent = suite.Parent as TestSuiteWrapper;
                if (parent != null)
                {
                    parent.RunningTests.Remove(suite);
                }
                OnTestFinished(new UnitTestResultEventArgs(new TestResultWrapper(suite, status)));
                SetStatus(parent, status);
            }
        }

        bool SetStarted(TestSuiteWrapper suite)
        {
            if (suite == null)
                return false;
            if (!_testSuiteStarted.Contains(suite))
            {
                _testSuiteStarted.Add(suite);
                var parent = suite.Parent as TestSuiteWrapper;
                if (parent != null)
                {
                    SetStarted(parent);
                    parent.RunningTests.Add(suite);
                }
                OnTestStarted(new UnitTestTestEventArgs(suite));
                return true;
            }
            return false;
        }

        void RunMessageHandler(xua.IMessageSinkMessage message)
        {
            try
            {
                if (message is xua.ITestAssemblyFinished assemblyFinished)
                {
                    _tcsRun.SetResult(new TestResultWrapper(assemblyFinished));
                }
                else if (message is xua.ITestClassStarting classStarting)
                {
                    var suite = Wrap(classStarting.TestClass);
                    SetStarted(suite);
                    OnTestStarted(new UnitTestTestEventArgs(suite));
                }
                else if (message is xua.ITestClassFinished classFinished)
                {
                    var suite = Wrap(classFinished.TestClass);
                    var result = new TestResultWrapper(classFinished, suite);
                    SetStatus(suite, result.Status);
                    OnTestFinished(new UnitTestResultEventArgs(result));
                }
                else if (message is xua.ITestStarting testStarting)
                {
                    OnTestStarted(new UnitTestTestEventArgs(Wrap(testStarting.TestCase)));
                }
                else if (message is xua.ITestFailed testFailed)
                {
                    OnTestFinished(new UnitTestResultEventArgs(new TestResultWrapper(testFailed, Wrap(testFailed.TestCase))));
                }
                else if (message is xua.ITestPassed testPassed)
                {
                    OnTestFinished(new UnitTestResultEventArgs(new TestResultWrapper(testPassed, Wrap(testPassed.TestCase))));
                }
                else if (message is xua.ITestSkipped testSkipped)
                {
                    OnTestFinished(new UnitTestResultEventArgs(new TestResultWrapper(testSkipped, Wrap(testSkipped.TestCase))));
                }
                else if (message is xua.IErrorMessage errorMessage)
                {
                    Console.WriteLine($"Error ocurred {errorMessage.ToString()}");
                }
                else
                {
                    Console.WriteLine($"Unhandled message of type {message.GetType()}");
                }
            }
            catch (Exception ex)
            {
                _tcsRun.SetException(ex);
            }
        }

        bool xu.IMessageSinkWithTypes.OnMessageWithTypes(xua.IMessageSinkMessage message, HashSet<string> messageTypes)
        {
            messageHandler?.Invoke(message);
            return !_cancel;
        }
    }
}
