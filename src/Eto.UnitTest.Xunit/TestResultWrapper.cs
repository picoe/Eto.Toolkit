using System;
using System.Collections.Generic;
using xua = Xunit.Abstractions;
using System.Linq;

namespace Eto.UnitTest.Xunit
{
    class TestAssertionWrapper : ITestAssertion
    {
        public string Message { get; set; }

        public TestStatus Status { get; set; }

        public string StackTrace { get; set; }

        public object NativeObject => null;
    }

    class TestResultWrapper : ITestResult
    {
        List<TestAssertionWrapper> _testAssertions;
        public TestResultWrapper(ITest test, TestStatus status)
        {
            Test = test;
            Status = status;
        }

        public TestResultWrapper(xua.ITestAssemblyFinished assemblyFinished)
        {
            FailCount = assemblyFinished.TestsFailed;
            SkipCount = assemblyFinished.TestsSkipped;
            PassCount = assemblyFinished.TestsRun - FailCount - SkipCount;
            Status = FailCount > 0 ? TestStatus.Failed : SkipCount > 0 ? TestStatus.Skipped : TestStatus.Passed;
            Duration = TimeSpan.FromSeconds((double)assemblyFinished.ExecutionTime);
        }

        public TestResultWrapper(xua.ITestClassFinished classFinished, ITest test)
        {
            Test = test;
            FailCount = classFinished.TestsFailed;
            SkipCount = classFinished.TestsSkipped;
            PassCount = classFinished.TestsRun - FailCount - SkipCount;
            Status = FailCount > 0 ? TestStatus.Failed : SkipCount > 0 ? TestStatus.Skipped : TestStatus.Passed;
            Duration = TimeSpan.FromSeconds((double)classFinished.ExecutionTime);
            NativeObject = classFinished;
        }

        public TestResultWrapper(xua.ITestCollectionFinished collectionFinished, ITest test)
        {
            Test = test;
            FailCount = collectionFinished.TestsFailed;
            SkipCount = collectionFinished.TestsSkipped;
            PassCount = collectionFinished.TestsRun - FailCount - SkipCount;
            Status = FailCount > 0 ? TestStatus.Failed : SkipCount > 0 ? TestStatus.Skipped : TestStatus.Passed;
            Duration = TimeSpan.FromSeconds((double)collectionFinished.ExecutionTime);
            NativeObject = collectionFinished;
        }
        public TestResultWrapper(xua.ITestFailed failed, ITest test)
        {
            Test = test;
            FailCount = 1;
            Set(failed);
            Status = SetFailureInfo(failed);
            //if (Status == TestStatus.Error)
            //    ErrorCount = 1;
        }
        public TestResultWrapper(xua.ITestSkipped skipped, ITest test)
        {
            Test = test;
            SkipCount = 1;
            Status = TestStatus.Skipped;
            Set(skipped);
        }

        public TestResultWrapper(xua.ITestPassed passed, ITest test)
        {
            Test = test;
            PassCount = 1;
            Status = TestStatus.Passed;
            Set(passed);
        }

        void Set(xua.ITestResultMessage message)
        {
            NativeObject = message;
            Duration = TimeSpan.FromSeconds((double)message.ExecutionTime);
            Output = message.Output;
        }

        TestStatus SetFailureInfo(xua.IFailureInformation failure)
        {
            var status = TestStatus.Failed;
            _testAssertions = new List<TestAssertionWrapper>();
            for (int i = 0; i < failure.Messages.Length; i++)
            {
                var assertion = new TestAssertionWrapper();
                var exceptionType = failure.ExceptionTypes[i];
                TestStatus assertionStatus = TestStatus.Failed;
                /*
                if (!exceptionType.StartsWith("Xunit.Sdk", StringComparison.Ordinal))
                    assertionStatus = TestStatus.Error;
                */

                status = (TestStatus)Math.Max((int)assertionStatus, (int)status);
                assertion.Message = failure.Messages[i];
                assertion.StackTrace = failure.StackTraces[i];
                assertion.Status = assertionStatus;
                _testAssertions.Add(assertion);
            }
            return status;
        }

        public int AssertCount { get; set; }

        public int FailCount { get; set; }

        public int ErrorCount { get; set; }

        public int WarningCount { get; set; }

        public int PassCount { get; set; }

        public int InconclusiveCount { get; set; }

        public int SkipCount { get; set; }

        public TestStatus Status { get; set; }

        public ITest Test { get; set; }

        public IEnumerable<ITestAssertion> Assersions => _testAssertions ?? Enumerable.Empty<ITestAssertion>();

        public IEnumerable<ITestResult> Children { get; set; } = Enumerable.Empty<ITestResult>();

        public TimeSpan Duration { get; set; }

        public DateTime EndTime { get; set; }

        public bool HasChildren { get; set; }

        public string Message { get; set; }

        public string Name { get; set; }

        public string Output { get; set; }

        public DateTime StartTime { get; set; }

        public object NativeObject { get; set; }
    }
}
