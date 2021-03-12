using System;
using System.Collections.Generic;
using System.Linq;

namespace Eto.CodeEditor
{
    public static class TextFunctions
    {
        // Scintilla position functions return byte indices
        // element index (the char index) has jumps when graphemes are more than 2 bytes because .Net strings are sequences of 'char's (2 bytes)
        // therefore the loopIdx (Item1) of the tuple has the true sequential character position index.
        public static IEnumerable<Tuple<int, int, string>> Graphemes(string text)
        {
            if (text != null)
            {
                // a text element is a grapheme cluster
                var graphemes = System.Globalization.StringInfo.GetTextElementEnumerator(text);
                int byteIdxRunningTotal = 0;
                int graphemeIdxRunningTotal = 0;
                yield return Tuple.Create(graphemeIdxRunningTotal, byteIdxRunningTotal, ""); // before first grapheme
                while (graphemes.MoveNext())
                {
                    var s = graphemes.GetTextElement();
                    var b = System.Text.Encoding.UTF8.GetBytes(s);
                    graphemeIdxRunningTotal++;
                    byteIdxRunningTotal += b.Length;
                    yield return Tuple.Create(graphemeIdxRunningTotal, byteIdxRunningTotal, s);
                }
            }
        }

        public static int GraphemeIndexFromByteIndex(string text, int byteIndex) =>
            text == null
              ? -1
              : text == string.Empty || byteIndex == 0
                ? 0
                // grapheme index at byte index if any
                : Graphemes(text).Where(t => t.Item2 == byteIndex).Select(t => (int?)t.Item1).SingleOrDefault()
                  // The last cases should happen very rarely and indicates an invalid byte index, such as one that falls in the middle of a grapheme cluster
                  // grapheme index of next byte index if any
                  ?? Graphemes(text).Where(t => t.Item2 > byteIndex).Select(t => (int?)t.Item1).FirstOrDefault()
                  // grapheme index of previous byte index if any
                  ?? Graphemes(text).Where(t => t.Item2 < byteIndex).Select(t => (int?)t.Item1).LastOrDefault()
                  // nothing found
                  ?? -1;
    }
}
