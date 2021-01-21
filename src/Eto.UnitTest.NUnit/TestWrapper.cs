using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using nui = NUnit.Framework.Interfaces;
using nufint = NUnit.Framework.Internal;

namespace Eto.UnitTest.NUnit
{
    [Serializable]
    class TestWrapper : ITest
    {
        readonly nui.ITest _test;

        public TestWrapper(nui.ITest test)
        {
            _test = test;
        }

        protected TestWrapper(SerializationInfo info, StreamingContext context)
        {
        }

        public bool IsAssembly => _test is nufint.TestAssembly;

        public bool IsSuite => _test.IsSuite;

        public bool IsParameterized => _test is nufint.ParameterizedMethodSuite;

        public string Name => _test.Name;

        public ITest Parent => _test.Parent != null ? new TestWrapper(_test.Parent) : null;

        public bool HasChildren => _test.HasChildren;

        public IEnumerable<ITest> Tests => _test.Tests.Select(r => new TestWrapper(r));

        public IEnumerable<string> Categories
        {
            get
            {
                var categories = _test.Properties["Category"];
                if (categories == null || categories.Count == 0)
                    return Enumerable.Empty<string>();
                return categories.OfType<string>();
            }
        }

        public string FullName => _test.FullName;

        public object NativeObject => _test;

        public Type Type => _test.TypeInfo.Type;

        public Assembly Assembly => (_test is nufint.TestAssembly testAssembly) ? testAssembly.Assembly : _test.TypeInfo?.Assembly;

        public override int GetHashCode() => _test.GetHashCode();

        public override bool Equals(object obj)
        {
            return obj is TestWrapper wrapper && wrapper._test.Equals(_test);
        }
    }
}
