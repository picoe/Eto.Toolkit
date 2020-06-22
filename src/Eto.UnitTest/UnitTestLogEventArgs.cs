using System;

namespace Eto.UnitTest
{
    public class UnitTestLogEventArgs : EventArgs
	{
		public string Message { get; }

		public UnitTestLogEventArgs(string message)
		{
			Message = message;
		}
	}
}
