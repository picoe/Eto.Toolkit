using System;

namespace Eto.UnitTest
{
    public class UnitTestProgressEventArgs : EventArgs
	{
		public int TestCaseCount { get; private set; }
		public int CompletedCount { get; private set; }
		public int AssertCount { get; private set; }
		public int FailCount { get; private set; }
		public int PassCount { get; private set; }
		public int WarningCount { get; private set; }
		public int InconclusiveCount { get; private set; }
		public int SkipCount { get; private set; }
		public ITestResult CurrentResult { get; private set; }

		public UnitTestProgressEventArgs(int testCaseCount)
		{
			TestCaseCount = testCaseCount;
		}

		internal void SetCount(int count)
		{
			CompletedCount = count;
		}

		public void AddResult(ITestResult result)
		{
			CurrentResult = result;
			CompletedCount++;
			AssertCount += result.AssertCount;
			FailCount += result.FailCount;
			WarningCount += result.WarningCount;
			PassCount += result.PassCount;
			InconclusiveCount += result.InconclusiveCount;
			SkipCount += result.SkipCount;
		}
	}
}
