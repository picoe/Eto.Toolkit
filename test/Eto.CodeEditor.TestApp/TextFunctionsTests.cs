using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using Eto.UnitTest.NUnit;

namespace Eto.CodeEditor.TestApp
{
    [TestFixture]
    class TextFunctionsTests
    {
        [Test]
        public void GraphemesReturnsEmptySequenceWhenTextIsNull()
        {
            var e = TextFunctions.Graphemes(null);
            Assert.IsNotNull(e);
            Assert.IsEmpty(e);
        }

        [Test]
        public void GraphemesReturnsSequenceOfOneWhenTextIsEmptyButNotNull()
        {
            var e = TextFunctions.Graphemes("");
            Assert.IsNotNull(e);
            Assert.AreEqual(1, e.Count());
            var i = e.First();
            Assert.AreEqual(0, i.Item1);
            Assert.AreEqual(0, i.Item2);
        }

        [Test]
        public void GraphemeIndicesMatchByteIndicesWhenAllCharactersAreOneByte()
        {
            // just a subset
            foreach (var t in TextFunctions.Graphemes("abcdefghijklmnopqrstuvwxyz1234567890"))
                Assert.AreEqual(t.Item1, t.Item2);
        }

        [Test]
        [TestCase(0)] // this should always be the case with an empty string
        [TestCase(1)] // return Zero even if byte index is out of range
        public void GraphemeIndexOfEmptyStringIsZero(int byteIdx)
        {
            var gidx = TextFunctions.GraphemeIndexFromByteIndex("", byteIdx);
            Assert.AreEqual(0, gidx);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(-1)]
        [TestCase(999)]
        public void GraphemeIndexOfNullStringIsMinusOne(int byteIdx)
        {
            // it doesn't matter wath the byteIdx is when the text is null
            var gidx = TextFunctions.GraphemeIndexFromByteIndex(null, byteIdx);
            Assert.AreEqual(-1, gidx);
        }

        [Test]
        [TestCase("a", 1)]
        [TestCase("Щ", 2)] // U+0429 CYRILLIC CAPITAL LETTER SHCHA ('Щ')
        [TestCase("ꦒ", 3)] // Scalar: U+A992 JAVANESE LETTER GA ('ꦒ')
        [TestCase("𐓌", 4)] // U+104CC OSAGE CAPITAL LETTER TSHA ('𐓌')
        [TestCase("🦏", 4)] // U+1F98F Even rhinos are not that big!

        public void MultipleByteCharactersTakenIntoAccount(string text, int byteIdx)
        {
            var gidx = TextFunctions.GraphemeIndexFromByteIndex(text, byteIdx);
            Assert.AreEqual(1, gidx);
        }

        [Test]
        [TestCase(4, 1, "match")]
        [TestCase(3, 1, "falls in the middle of a cluster. Get grapheme index directly passed it.")]
        [TestCase(-100, 0, "way below lower boundary")]
        [TestCase(1000, 9, "way above upper boundary")]
        public void WhenByteIndexIsInTheMiddleOfAMultiByteCharacter(int byteIdx, int expected, string msg)
        {
            /*
            grapheme idx: 0,   byte idx:  0 
            grapheme idx: 1,   byte idx:  4 𐓏
            grapheme idx: 2,   byte idx:  8 𐓘
            grapheme idx: 3,   byte idx: 12 𐓻
            grapheme idx: 4,   byte idx: 16 𐓘
            grapheme idx: 5,   byte idx: 20 𐓻
            grapheme idx: 6,   byte idx: 24 𐓟
            grapheme idx: 7,   byte idx: 25  
            grapheme idx: 8,   byte idx: 29 𐒻
            grapheme idx: 9,   byte idx: 33 𐓟
            */
            string osage = "𐓏𐓘𐓻𐓘𐓻𐓟 𐒻𐓟";
            int graphemeIdx = TextFunctions.GraphemeIndexFromByteIndex(osage, byteIdx);
            Assert.AreEqual(expected, graphemeIdx);
        }

    }
}
