using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Eto.UnitTest.NUnit;
using Eto.Drawing;
using System.Linq;

namespace Eto.CodeEditor.TestApp
{
    [TestFixture]
    class CodeEditorTests
    {
        public static CodeEditor editor { get; set; }

        [Test, InvokeOnUI]
        public void Rnd()
        {
            editor.HighlightRange(13, 5);
            Assert.IsTrue(true);
        }

        [Test, InvokeOnUI]
        public void Rnd2()
        {
            editor.HighlightColor = Colors.Fuchsia;
            Assert.IsTrue(true);
        }

        [Test, InvokeOnUI]
        public void Rnd3()
        {
            editor.HighlightRange(20, 5);
            Assert.IsTrue(true);
        }

        [Test, InvokeOnUI]
        public void Rnd4()
        {
            editor.ClearHighlights();
            Assert.IsTrue(true);
        }

        [Test, InvokeOnUI]
        public void SearchInAll()
        {
            editor.Text = "Be who you are and say what you feel, because those who mind don't matter, and those who matter don't mind";
            var foundPositions = editor.SearchInAll("who", true);
            Assert.AreEqual(3, foundPositions.Count);
        }

        [Test, InvokeOnUI]
        public void FontSticks()
        {
            Assert.IsNotNull(editor, "oops");
            var editorFont = editor.Font;
            Assert.IsNotNull(editorFont);
            var newFont = new Font(
                Fonts.AvailableFontFamilies.FirstOrDefault(ff => ff.Name == "Courier New"), 
                editorFont.Size, 
                editorFont.FontStyle, 
                editorFont.FontDecoration);
            Assert.IsNotNull(newFont, "Courier New is not installed on system");
            Assert.AreNotEqual(editorFont, newFont);
            var newFont2 = new Font(newFont.Family, newFont.Size, newFont.FontStyle, newFont.FontDecoration);
            Assert.AreEqual(newFont, newFont2);
            editor.Font = newFont;
            Assert.AreEqual(newFont, editor.Font);
            editor.Font = editorFont;
        }
    }
}
