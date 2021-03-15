using System;
using NUnit.Framework;
using Eto.UnitTest.NUnit;
using Eto.Drawing;
using System.Linq;

namespace Eto.CodeEditor.TestApp
{
    // todo: need to find a better way to set these up so user interaction is not needed
    [TestFixture]
    class TestsThatRequireUserInteraction
    {
        public static CodeEditor editor { get; set; }

        [Test, InvokeOnUI]
        [TestCase(13, 5)]
        [TestCase(20, 5)]
        public void HighlightRange_visually_inspect(int pos, int len)
        {
            editor.HighlightRange(pos, len);
            Assert.IsTrue(true);
        }

        [Test, InvokeOnUI]
        public void HighlightColor_visually_inspect()
        {
            editor.HighlightColor = Colors.Fuchsia;
            Assert.IsTrue(true);
        }

        [Test, InvokeOnUI]
        public void ClearHighlights_visually_inspect()
        {
            editor.ClearHighlights();
            Assert.IsTrue(true);
        }

        [Test, InvokeOnUI]
        [TestCase("aäa", 2)]
        public void CurrentPositionInLineCursorAfter2byteCharacter(string txt, int expected)
        {
            // place cursor after 'ä'
            Assert.AreEqual(txt, editor.Text);
            var cpil = editor.CurrentPositionInLine;
            Assert.AreEqual(expected, cpil);
        }

        [Test, InvokeOnUI]
        [TestCase(" abc", 1)]
        public void CurrentPositionInLineDoesntIgnorLeadingWhitespace(string txt, int expected)
        {
            // put cursor before the 'a'
            Assert.AreEqual(txt, editor.Text);
            var cpil = editor.CurrentPositionInLine;
            Assert.AreEqual(expected, cpil);
        }

        [Test, InvokeOnUI]
        [TestCase("a🦏b", 2, "put cursor behind the rhino")]
        public void CurrentPositionInLineMiscAfter4byteGrapheme(string txt, int expected, string msg)
        {
            var pos = editor.CurrentPositionInLine;
            Assert.AreEqual(expected, pos);
        }

        [Test, InvokeOnUI]
        [TestCase("aꦒb", 2, "javanese ga has 3 bytes")]
        public void CurrentPositionInLineAfter3byteGrapheme(string txt, int expected, string msg)
        {
            var pos = editor.CurrentPositionInLine;
            Assert.AreEqual(expected, pos);
        }

        [Test, InvokeOnUI]
        [TestCase("abc\r\ndef", 3, "place cursor at the end of the first line")]
        public void CurrentPositionInLineMiscEol(string txt, int expected, string msg)
        {
            var pos = editor.CurrentPositionInLine;
            Assert.AreEqual(expected, pos);
        }

        [Test, InvokeOnUI]
        [TestCase("abc\r\ndef\r\n", 3, "place cursor at the end of the last line")]
        public void CurrentPositionInLineMiscEolEof(string txt, int expected, string msg)
        {
            var pos = editor.CurrentPositionInLine;
            Assert.AreEqual(expected, pos);
        }

        [Test, InvokeOnUI]
        [TestCase("abc\tdef", 7, "place cursor at the end of the line")]
        public void CurrentPositionInLineMiscTabIsOneByte(string txt, int expected, string msg)
        {
            var pos = editor.CurrentPositionInLine;
            Assert.AreEqual(expected, pos);
        }

        [Test, InvokeOnUI]
        [TestCase("", 0, "empty document")]
        public void CurrentPositionInLineMiscZeroInEmptyDoc(string txt, int expected, string msg)
        {
            Assert.AreEqual(txt, editor.Text);
            var pos = editor.CurrentPositionInLine;
            Assert.AreEqual(expected, pos);
        }

        [Test, InvokeOnUI]
        public void FindAndSelectRange()
        {
            var searchText = "syntax";
            editor.Text = "import rhinoscriptsyntax as rs";
            var hit = editor.SearchInAll(searchText).First();
            editor.SelectRange(hit, searchText.Length);
            Eto.Forms.MessageBox.Show("check that the word 'syntax' is selected");
        }
    }
}
