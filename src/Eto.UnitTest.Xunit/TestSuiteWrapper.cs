using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Eto.UnitTest.Xunit
{
    class TestSuiteWrapper : ITest
    {
        string _fullName;
        public List<ITest> Children { get; } = new List<ITest>();

        public virtual bool IsSuite => true;

        public virtual bool IsAssembly => false;

        public bool IsParameterized { get; set; }

        public string Name { get; set; }

        public ITest Parent { get; set; }

        public bool HasChildren => true;

        public IEnumerable<ITest> Tests => Children;

        public IEnumerable<string> Categories => Enumerable.Empty<string>();

        public List<TestSuiteWrapper> RunningTests { get; } = new List<TestSuiteWrapper>();

        public Type Type => null;

        public Assembly Assembly { get; set; }

        public string FullName => _fullName ?? (_fullName = Parent != null ? $"{Parent.FullName}.{Name}" : Name);

        public object NativeObject => null;
    }
}
