using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Collections;
using Eto.UnitTest.Filters;

namespace Eto.UnitTest
{

    public static class TestFilter
    {
        public static ITestFilter Empty { get; } = new EmptyFilter();
    }
}
