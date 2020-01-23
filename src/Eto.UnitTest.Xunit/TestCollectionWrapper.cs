using System.Linq;
using System.Reflection;

namespace Eto.UnitTest.Xunit
{
    class TestCollectionWrapper : TestSuiteWrapper
    {
        public override bool IsAssembly => true;

        public TestCollectionWrapper(Assembly assembly)
        {
            Assembly = assembly;
            Name = assembly.GetName().Name;
        }

        public TestSuiteWrapper GetSuite(string fullName, bool isClass)
        {
            var names = fullName.Split('.');
            TestSuiteWrapper suite = this;
            var len = isClass ? names.Length : names.Length - 1;
            for (int i = 0; i < len; i++)
            {
                string name = names[i];
                var child = suite.Children.FirstOrDefault(r => r.Name == name) as TestSuiteWrapper;
                if (child == null)
                {
                    child = new TestSuiteWrapper { Name = name, Parent = suite, Assembly = Assembly };
                    suite.Children.Add(child);
                }
                suite = child;
            }
            return suite;
        }
    }
}
