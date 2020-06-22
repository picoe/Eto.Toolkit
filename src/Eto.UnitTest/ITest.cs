using System;
using System.Reflection;
using System.Collections.Generic;

namespace Eto.UnitTest
{
    public interface ITest : INativeObject
    {
        bool IsSuite { get; }
        bool IsAssembly { get; }
        bool IsParameterized { get; }
        string Name { get; }
        ITest Parent { get; }
        bool HasChildren { get; }
        IEnumerable<ITest> Tests { get; }
        IEnumerable<string> Categories { get; }
        Type Type { get; }
        Assembly Assembly { get; }
        string FullName { get; }
    }
}
