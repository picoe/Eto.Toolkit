using System.Reflection;

namespace Eto.UnitTest
{
    class SingleTestFilter : ITestFilter
    {
        public ITest Test { get; set; }

        public Assembly Assembly { get; set; }

        public bool IsExplicitMatch(ITest test)
        {
            if (Assembly != null)
            {
                if (!test.IsSuite && test.Assembly != Assembly)
                    return false;
                if (test.IsAssembly && test.Assembly != Assembly)
                    return false;
            }
            return test.FullName == Test.FullName;
        }

        public bool Pass(ITest test)
        {
            if (Assembly != null)
            {
                if (!test.IsSuite && test.Assembly != Assembly)
                    return false;

                if (test.IsAssembly && test.Assembly != Assembly)
                    return false;
            }


            var parent = Test;
            // check if it is a parent of the test
            while (parent != null)
            {
                if (test.FullName == parent.FullName)
                    return true;
                parent = parent.Parent;
            }
            // execute all children of the test
            parent = test;
            while (parent != null)
            {
                if (parent.FullName == Test.FullName)
                    return true;
                parent = parent.Parent;
            }
            return false;
        }
    }
}
