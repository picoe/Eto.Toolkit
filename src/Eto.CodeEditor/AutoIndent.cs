using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Eto.CodeEditor
{
    public static class AutoIndent
    {
        public static void IndentationCheck(char trigger, CodeEditor editor)
        {
            // CRLF line endings generate 2 char_added events. Ignore the 2nd one
            if (previousCharWasCR && trigger == '\n')
            {
                previousCharWasCR = false;
                return;
            }
            previousCharWasCR = trigger == '\r';

            if (!(IsTriggerNewLineChar(trigger) || (IsCLikeLanguage(editor.Language) && trigger == '}')))
                return;
            if (IsTriggerNewLineChar(trigger))
            {
                var language = editor.Language;
                var previousLineNumber = editor.CurrentLineNumber - 1;
                var previousLineIndentation = editor.GetLineIndentation(previousLineNumber);
                var previousLineLastChar = editor.GetLineLastChar(previousLineNumber);
                var oneIndentSize = editor.TabWidth;
                var newIndent = GetNewLineIndentation(language, previousLineIndentation, previousLineLastChar, oneIndentSize);
                if (newIndent != 0)
                {
                    var pos = editor.CurrentPosition;
                    editor.SetLineIndentation(editor.CurrentLineNumber, newIndent);
                    // on macOS SetLineIndentation doesn't move the cursor
                    if (editor.CurrentPosition == pos)
                        editor.CurrentPosition += newIndent;
                }
            }
            else if (editor.CurrentLineNumber > 0
                && IsCLikeLanguage(editor.Language) && trigger == '}'
                && editor.GetLineText(editor.CurrentLineNumber).Trim() == "}")
            {
                // a good start but matching the indent of the open brace line is better
                Func<string, string> ws = s => Regex.Match(s ?? "", @"^\s*").Value;
                var prevLineLeadingWhitespace = ws(editor.GetLineText(editor.CurrentLineNumber - 1));
                var newLeadingWhitespace = prevLineLeadingWhitespace.Length < editor.TabWidth
                    ? prevLineLeadingWhitespace
                    : prevLineLeadingWhitespace.Substring(0, prevLineLeadingWhitespace.Length - editor.TabWidth);
                var lineLeadingWhitespace = ws(editor.GetLineText(editor.CurrentLineNumber));
                editor.ReplaceFirstOccuranceInLine(lineLeadingWhitespace, newLeadingWhitespace, editor.CurrentLineNumber);
            }
        }

        public static int GetNewLineIndentation(ProgrammingLanguage language, int previousLineIndentation, char previousLineLastChar, int oneCharIndentSize) =>
            language == ProgrammingLanguage.Python && previousLineLastChar == ':' || IsCLikeLanguage(language) && previousLineLastChar == '{'
                    ? previousLineIndentation + oneCharIndentSize
                    : previousLineIndentation;

        private static bool IsCLikeLanguage(ProgrammingLanguage language) => language == ProgrammingLanguage.CSharp || language == ProgrammingLanguage.GLSL;

        private static bool IsTriggerNewLineChar(char trigger) => trigger == '\r' || trigger == '\n';

        private static bool previousCharWasCR;
    }
}
