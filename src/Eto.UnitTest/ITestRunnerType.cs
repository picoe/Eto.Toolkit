using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Eto.UnitTest
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class TestRunnerTypeAttribute : Attribute
    {
        ITestRunnerType _runnerType;
        public Type Type { get; }

        public ITestRunnerType RunnerType => _runnerType ?? (_runnerType = (ITestRunnerType)Activator.CreateInstance(Type));

        public TestRunnerTypeAttribute(Type type)
        {
            Type = type;
        }
    }

    public abstract class TestRunnerType<T> : ITestRunnerType
        where T : ITestRunner, new()
    {
        public virtual string Name { get; }

        static bool HasReference(Assembly assembly, string referenceName)
        {
            foreach (var reference in assembly.GetReferencedAssemblies())
            {
                if (string.Equals(reference.Name, referenceName, StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        protected virtual string[] RequiredReferences => new string[0];

        public virtual bool CanExecute(ITestSource source)
        {
            var assembly = source.GetReflectionAssembly();
            foreach (var reference in RequiredReferences)
            {
                if (!HasReference(assembly, reference))
                    return false;
            }
            return true;
        }

        public ITestRunner CreateRunner() => new T();
    }


    public static class TestRunnerType
    {
        static List<ITestRunnerType> types;

        public static void AddAssembly(AssemblyName assemblyName)
        {
            AddAssembly(Assembly.Load(assemblyName));
        }

        public static void Add(ITestRunnerType type)
        {
            if (types == null)
                types = new List<ITestRunnerType>();
            types.Add(type);
        }

        public static void AddAssembly(Assembly assembly)
        {
            if (types == null)
                types = new List<ITestRunnerType>();
            types.AddRange(assembly.GetCustomAttributes<TestRunnerTypeAttribute>().Select(r => r.RunnerType));
        }

        public static ITestRunnerType Find(ITestSource source)
        {
            if (types == null)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    AddAssembly(assembly);
                }
            }

            foreach (var type in types)
            {
                if (type.CanExecute(source))
                {
                    return type;
                }
            }
            return null;
        }
    }

    public interface ITestRunnerType
    {
        string Name { get; }
        bool CanExecute(ITestSource source);
        ITestRunner CreateRunner();
    }
}
