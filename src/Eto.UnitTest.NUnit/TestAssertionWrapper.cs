using System;
using NUnit.Framework.Interfaces;

namespace Eto.UnitTest.NUnit
{
    class TestAssertion : ITestAssertion
    {
        public string Message { get; set; }

        public TestStatus Status { get; set; }

        public string StackTrace { get; set; }

        public object NativeObject => null;
    }

    class TestAssertionWrapper : ITestAssertion
    {
        AssertionResult _assertion;

        public TestAssertionWrapper(AssertionResult assertion)
        {
            _assertion = assertion;
        }

        public string Message => _assertion.Message;

        public TestStatus Status
        {
            get
            {
                switch (_assertion.Status)
                {
                    case AssertionStatus.Error:
                        return TestStatus.Failed;
                        //return TestStatus.Error;
                    case AssertionStatus.Failed:
                        return TestStatus.Failed;
                    case AssertionStatus.Inconclusive:
                        return TestStatus.Inconclusive;
                    case AssertionStatus.Passed:
                        return TestStatus.Passed;
                    case AssertionStatus.Warning:
                        return TestStatus.Warning;
                    default:
                        throw new NotSupportedException();
                }
            }

        }

        public string StackTrace => _assertion.StackTrace;

        public object NativeObject => _assertion;
    }
}