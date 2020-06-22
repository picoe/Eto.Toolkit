using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Eto.UnitTest
{
    class MultipleTest : ITest
    {
        List<ITest> _tests;
        public MultipleTest(IEnumerable<ITest> tests)
        {
            _tests = tests.ToList();
        }

        public bool IsSuite => true;

        public bool IsAssembly => false;

        public bool IsParameterized => false;

        public string Name => null;

        public ITest Parent => null;

        public bool HasChildren => true;

        public IEnumerable<ITest> Tests => _tests;

        public IEnumerable<string> Categories => _tests.SelectMany(r => r.Categories).Distinct();

        public Type Type => null;

        public Assembly Assembly => null;

        public string FullName => null;

        public object NativeObject => null;
    }
}
