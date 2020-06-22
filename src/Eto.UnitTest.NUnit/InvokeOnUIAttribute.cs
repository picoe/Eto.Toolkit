using System;
using System.Runtime.ExceptionServices;
using Eto.Forms;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace Eto.UnitTest.NUnit
{

    /// <summary>
    /// Runs the test on the UI thread
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class InvokeOnUIAttribute : Attribute, IWrapSetUpTearDown
    {
        /// <summary>
        /// Wraps the specified command
        /// </summary>
        /// <param name="command">command to wrap</param>
        /// <returns>A new command that wraps the specified command in a UI invoke</returns>
        public TestCommand Wrap(TestCommand command) => new RunOnUICommand(command);

        class RunOnUICommand : DelegatingTestCommand
        {
            public RunOnUICommand(TestCommand innerCommand)
                : base(innerCommand)
            {
            }

            public override TestResult Execute(TestExecutionContext context)
            {
                Exception exception = null;

                var result = Application.Instance.Invoke(() =>
                {
                    try
                    {
                        context.EstablishExecutionEnvironment();
                        return innerCommand.Execute(context);
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        return null;
                    }
                });

                if (exception != null)
                {
                    ExceptionDispatchInfo.Capture(exception).Throw();
                }

                return result;
            }
        }
    }
}
