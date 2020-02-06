using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eto.UnitTest
{
    public interface ITestRunner
    {
        bool IsRunning { get; }
        ITest TestSuite { get; }

        event EventHandler<UnitTestLogEventArgs> Log;
        event EventHandler<UnitTestProgressEventArgs> Progress;
        event EventHandler<UnitTestResultEventArgs> TestFinished;
        event EventHandler<UnitTestTestEventArgs> TestStarted;
        event EventHandler<EventArgs> IsRunningChanged;

        void StopTests();

        Task Load(ITestSource source);

        Task<int> GetTestCount(ITestFilter filter);
        Task<IEnumerable<string>> GetCategories(ITestFilter filter);
        Task<ITestResult> RunAsync(ITestFilter filter);
    }
}
