using System;
using System.IO;
using System.Reflection;

namespace Eto.UnitTest
{
    public interface ITestSource
    {
        Assembly Assembly { get; }
        Assembly GetReflectionAssembly();
    }

    [Serializable]
    public class TestSource : ITestSource
	{
        [NonSerialized]
        Assembly _reflectionAssembly;
        Assembly _assembly;

		public string Path { get; }
        public Assembly Assembly => _assembly ?? Assembly.LoadFrom(Path);
		public TestSource(string path)
		{
			Path = path;
		}

		public TestSource(Assembly assembly)
		{
			_assembly = assembly;
            Path = assembly.Location;
		}

        public Assembly GetReflectionAssembly() => _assembly ?? _reflectionAssembly ?? (_reflectionAssembly = Assembly.ReflectionOnlyLoad(File.ReadAllBytes(Path)));

		public static implicit operator TestSource(Assembly assembly) => new TestSource(assembly);

		public static implicit operator TestSource(string assemblyName) => new TestSource(assemblyName);
	}
}
