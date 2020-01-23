using System;
using System.Text;
using System.Threading.Tasks;

namespace Eto.UnitTest.Runners
{
    public class LoggingTestRunner : WrappedTestRunner
    {
        string lastTestStarted;
        object lastTestLock = new object();
        public bool ShowOutput { get; set; }
        public bool ShowOnlyFailed { get; set; } = true;
        public bool ShowTotalAtStart { get; set; }


        public LoggingTestRunner(ITestRunner inner)
            : base(inner)
        {
        }

        protected override void OnTestStarted(UnitTestTestEventArgs e)
        {
            base.OnTestStarted(e);
            var test = e.Test;

            if (!ShowOnlyFailed && !test.IsSuite)
            {
                lock (lastTestLock)
                {
                    lastTestStarted = test.FullName;
                    WriteLog(test.FullName);
                }
            }
        }

        void WriteTest(ITest test)
        {
            var currentTest = test.FullName;
            if (lastTestStarted != currentTest)
            {
                WriteLog(currentTest);
                lastTestStarted = currentTest;
            }
        }

        protected override void OnTestFinished(UnitTestResultEventArgs e)
        {
            base.OnTestFinished(e);
            var result = e.Result;

            if (!result.Test.IsSuite)
            {
                lock (lastTestLock)
                {

                    if (ShowOutput && !string.IsNullOrEmpty(result.Output))
                    {
                        WriteTest(result.Test);
                        WriteLog(result.Output);
                    }

                    if (result.Status != TestStatus.Passed && result.Status != TestStatus.Skipped)
                    {
                        WriteTest(result.Test);
                        var assertions = result.Assersions;
                        if (assertions != null)
                        {
                            foreach (var assertion in assertions)
                            {
                                if (assertion.Status == TestStatus.Passed)
                                    continue;
                                if (!string.IsNullOrEmpty(assertion.StackTrace))
                                    WriteLog($"{assertion.Status}: {assertion.Message}\n{assertion.StackTrace}");
                                else
                                    WriteLog($"{assertion.Status}: {assertion.Message}");
                            }
                        }
                        else
                        {
                            WriteLog($"{result.Status}: {result.Message}");
                        }
                    }
                }
            }
        }

        public override async Task<ITestResult> RunAsync(ITestFilter filter)
        {
            WriteLog("Starting tests...");

            if (ShowTotalAtStart)
            {
                var testCount = await GetTestCount(filter);

                WriteLog($"Total test count: {testCount}");
            }

            ITestResult result;
            try
            {
                result = await base.RunAsync(filter);
            }
            catch (Exception ex)
            {
                WriteLog($"Error running tests: {ex}");
                return null;
            }

            WriteLog(result.FailCount > 0 ? "FAILED" : "PASSED");
            var sb = new StringBuilder();
            void AppendText(string str)
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append(str);
            }
            if (result.PassCount > 0)
                AppendText($"Passed: {result.PassCount}");
            if (result.FailCount > 0)
                AppendText($"Failed: {result.FailCount}");
            if (result.SkipCount > 0)
                AppendText($"Skipped: {result.FailCount}");
            //if (result.ErrorCount > 0)
            //    AppendText($"Errors: {result.ErrorCount}");
            if (result.WarningCount > 0)
                AppendText($"Warnings: {result.WarningCount}");
            if (result.InconclusiveCount > 0)
                AppendText($"Inconclusive: {result.InconclusiveCount}");

            WriteLog($"\t{sb}");
            WriteLog($"\tDuration: {result.Duration}");
            return result;
        }
    }
}
