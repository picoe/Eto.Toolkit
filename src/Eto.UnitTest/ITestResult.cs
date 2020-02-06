using System;
using System.Collections.Generic;

namespace Eto.UnitTest
{
    public interface ITestAssertion : INativeObject
    {
        string Message { get; }
        TestStatus Status { get; }
        string StackTrace { get; }
    }

    public interface ITestResult : INativeObject
    {
        int AssertCount { get; }
		//int ErrorCount { get; }
		int FailCount { get; }
        int WarningCount { get; }
		int PassCount { get; }
        int InconclusiveCount { get; }
        int SkipCount { get; }
        TestStatus Status { get; }
        ITest Test { get; }
        IEnumerable<ITestAssertion> Assersions { get; }
        IEnumerable<ITestResult> Children { get; }
        TimeSpan Duration { get; }
        DateTime EndTime { get; }
        bool HasChildren { get; }
        string Message { get; }
        string Name { get; }
        string Output { get; }
        DateTime StartTime { get; }
    }
}
