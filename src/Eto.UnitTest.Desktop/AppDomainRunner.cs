using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eto.UnitTest.App
{
    class AppDomainTestRunnerWrapper : MarshalByRefObject
    {
        ITestRunner _runner;
        public bool IsRunning => _runner.IsRunning;

        public ITest Test => _runner.TestSuite;

        public event EventHandler<UnitTestLogEventArgs> Log;
        public event EventHandler<UnitTestProgressEventArgs> Progress;
        public event EventHandler<UnitTestResultEventArgs> TestFinished;
        public event EventHandler<UnitTestTestEventArgs> TestStarted;
        public event EventHandler<EventArgs> IsRunningChanged;

        public async Task<IEnumerable<string>> GetCategories(ITestFilter filter)
        {
            var categories = await _runner.GetCategories(filter);
            return categories.ToList();
        }

        public Task<int> GetTestCount(ITestFilter filter)
        {
            return _runner.GetTestCount(filter);
        }

        public Task<ITestResult> RunAsync(ITestFilter filter)
        {
            return _runner.RunAsync(filter);
        }

        public void StopTests() => _runner.StopTests();

        public void CreateRunner(Type type)
        {
            var runnerType = Activator.CreateInstance(type) as ITestRunnerType;
            _runner = runnerType.CreateRunner();
        }

        public Task Load(ITestSource source) => _runner.Load(source);
    }

    public class AppDomainTestRunner : ITestRunner, IDisposable
    {
        AppDomainTestRunnerWrapper _wrapper;
        public bool IsRunning => _wrapper.IsRunning;

        public ITest TestSuite => _wrapper.Test;

        public event EventHandler<UnitTestLogEventArgs> Log;
        public event EventHandler<UnitTestProgressEventArgs> Progress;
        public event EventHandler<UnitTestResultEventArgs> TestFinished;
        public event EventHandler<UnitTestTestEventArgs> TestStarted;
        public event EventHandler<EventArgs> IsRunningChanged;

        public AppDomainTestRunner()
        {
            _wrapper = Domain.CreateInstanceFromAndUnwrap(typeof(AppDomainTestRunnerWrapper).Assembly.CodeBase, typeof(AppDomainTestRunnerWrapper).FullName) as AppDomainTestRunnerWrapper;
        }

        public void Initialize(Type runnerType)
        {
            _wrapper.CreateRunner(runnerType);
        }

        AppDomain _domain;

        AppDomain Domain
        {
            get
            {
                if (_domain != null)
                    return _domain;

                var cur = AppDomain.CurrentDomain;
                var si = cur.SetupInformation;
                //var path = AssemblyPath;
                var setup = new AppDomainSetup
                {
                    ApplicationName = si.ApplicationName ?? "Eto.UnitTest",
                    CachePath = si.CachePath,
                    ShadowCopyFiles = "true",//si.ShadowCopyFiles,
                    ShadowCopyDirectories = si.ShadowCopyDirectories,
                    PrivateBinPath = si.PrivateBinPath,
                    //DynamicBase = si.DynamicBase,
                    TargetFrameworkName = si.TargetFrameworkName,
                    ConfigurationFile = si.ConfigurationFile,
                    ApplicationBase = si.ApplicationBase,
                    LoaderOptimization = LoaderOptimization.MultiDomain,//= si.LoaderOptimization,
                                                                        //SandboxInterop = si.SandboxInterop
                };
                //setup.SetCompatibilitySwitches(new[] { "NetFx40_LegacySecurityPolicy" });
                _domain = AppDomain.CreateDomain("UnitTests");//, cur.Evidence, setup);
                cur.DomainUnload += (sender, e) => UnloadDomain();
                return _domain;
            }
        }

        public Task<IEnumerable<string>> GetCategories(ITestFilter filter)
        {
            return _wrapper.GetCategories(filter);
        }

        public Task<int> GetTestCount(ITestFilter filter)
        {
            return _wrapper.GetTestCount(filter);
        }

        public Task<ITestResult> RunAsync(ITestFilter filter)
        {
            return _wrapper.RunAsync(filter);
        }

        public void StopTests()
        {
            _wrapper.StopTests();
        }

        void UnloadDomain()
        {
            if (_domain == null)
                return;

            AppDomain.Unload(_domain);
            _domain = null;
        }

        public void Dispose() => UnloadDomain();

        public Task Load(ITestSource source) => _wrapper.Load(source);
    }

}
