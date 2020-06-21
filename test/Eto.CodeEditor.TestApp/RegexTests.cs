using System;
using NUnit.Framework;
using Eto.UnitTest.NUnit;
using System.Collections.Generic;
using System.Linq;

namespace Eto.CodeEditor.TestApp
{
    [TestFixture]
    public class RegexTests
    {
        public static CodeEditor editor { get; set; }

        [Test, InvokeOnUI]
        public void InvalidOrEmptyRegexPatternReturnsEmptyIList()
        {
            editor.Text = "some text";
            var r = editor.SearchInAll("p[y", false, false);
            Assert.IsTrue(r != null && r.Count == 0);

            r = editor.SearchInAll("", false, false);
            Assert.IsTrue(r != null && r.Count == 0);
        }

        [Test, InvokeOnUI]
        public void TypicalMatches()
        {
            editor.Text = "import rhinoscript as rs\nrs.AddCircle((0,0,0), 3.0)\nrs.AddSomething('something')\npyython pxxxthon pizzathon";
            var matches = editor.SearchInAll("p[a-z]*thon", false, true);
            Assert.AreEqual(3, matches.Count);

            var lst = new List<string> { "pyython", "pxxxthon", "pizzathon" };
            Assert.IsTrue(matches.All(m => lst.Contains(m.Item2)), matches.Select(m => m.Item2).Aggregate((a,b) => $"{a} {b}"));
        }
    }
}
