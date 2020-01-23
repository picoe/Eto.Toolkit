using System;

namespace Eto.UnitTest
{
    public sealed class UnitTestResultEventArgs : EventArgs
	{
		public ITestResult Result { get; private set; }

		public UnitTestResultEventArgs(ITestResult result)
		{
			Result = result;
		}
	}
}
