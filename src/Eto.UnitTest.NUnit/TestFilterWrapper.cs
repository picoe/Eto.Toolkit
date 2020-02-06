using System;
using nui = NUnit.Framework.Interfaces;

namespace Eto.UnitTest.NUnit
{
    class TestFilterWrapper : nui.ITestFilter
    {
        ITestFilter _filter;

        public TestFilterWrapper(ITestFilter filter)
        {
            _filter = filter;
        }

        public bool IsExplicitMatch(nui.ITest test) => _filter.IsExplicitMatch(new TestWrapper(test));

        public bool Pass(nui.ITest test) => _filter.Pass(new TestWrapper(test));

        public nui.TNode ToXml(bool recursive) => throw new NotImplementedException();

        public nui.TNode AddToXml(nui.TNode parentNode, bool recursive) => throw new NotImplementedException();
    }
}
