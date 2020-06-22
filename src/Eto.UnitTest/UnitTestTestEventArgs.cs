using System;

namespace Eto.UnitTest
{
    public sealed class UnitTestTestEventArgs : EventArgs
	{
		public ITest Test { get; private set; }

		public UnitTestTestEventArgs(ITest test)
		{
			Test = test;
		}
	}
}
