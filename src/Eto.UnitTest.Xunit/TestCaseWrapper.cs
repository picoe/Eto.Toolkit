using System;
using System.Collections.Generic;
using xua = Xunit.Abstractions;
using System.Linq;
using System.Reflection;

namespace Eto.UnitTest.Xunit
{
    class TestCaseWrapper : ITest
    {
        readonly xua.ITestCase _test;
        Type _type;

        public TestCaseWrapper(xua.ITestCase test)
        {
            _test = test;
        }

        public List<TestCaseWrapper> Children { get; set; }

        public bool IsSuite => false;

        public bool IsAssembly => false;

        public bool IsParameterized => false;

        public string Name => _test.TestMethod.Method.Name;

        public ITest Parent { get; set; }

        public bool HasChildren => Children?.Count > 0;

        public IEnumerable<ITest> Tests => Children ?? Enumerable.Empty<ITest>();

        public IEnumerable<string> Categories => new string[0];

        public Type Type => _type ?? (_type = _test.TestMethod.TestClass.Class.ToRuntimeType());

        public Assembly Assembly => Type.Assembly;

        public string FullName => _test.DisplayName;// _fullName ?? (_fullName = Parent != null ? $"{Parent.FullName}.{Name}" : Name);

        public object NativeObject => _test;

        public override int GetHashCode() => _test.UniqueID.GetHashCode();

        public override bool Equals(object obj)
        {
            return obj is TestCaseWrapper wrapper && wrapper._test.UniqueID.Equals(_test.UniqueID);
        }
    }
}
