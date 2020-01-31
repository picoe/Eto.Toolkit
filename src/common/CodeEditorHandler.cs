using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto;
using Eto.Forms;
using Eto.CodeEditor;
using ScintillaNET;
using Eto.Drawing;

[assembly: ExportHandler(typeof(CodeEditor), typeof(CodeEditorHandler))]

namespace Eto.CodeEditor
{
    public partial class CodeEditorHandler  //: Eto.Wpf.Forms.WindowsFormsHostHandler<Scintilla.ScintillaControl, CodeEditor, CodeEditor.ICallback>, CodeEditor.IHandler
    {
        #region IHandler impl
        public string Text
        {
            get => scintilla.Text;
            set => scintilla.Text = value;
        }

        public void SetProgrammingLanguage(ProgrammingLanguage language, string[] keywordSets)
        {
            scintilla.SetProgrammingLanguage(language, keywordSets);
        }

        public string FontName
        {
            get => scintilla.FontName;
            set => scintilla.FontName = value;
        }

        public void SetColor(Section section, Eto.Drawing.Color foreground, Eto.Drawing.Color background)
        {
            scintilla.SetColor(section, foreground, background);
        }

        public int CurrentPosition
        {
            get => scintilla.CurrentPosition;
            set => scintilla.CurrentPosition = value;
        }

        public int CurrentPositionInLine => scintilla.CurrentPositionInLine;

        public int CurrentLineNumber => scintilla.CurrentLineNumber;

        public string GetLineText(int lineNumber)
        {
            return scintilla.GetLineText(lineNumber);
        }

        public unsafe void InsertText(int position, string text) 
        {
            scintilla.InsertText(position, text);
        }

        public unsafe int ReplaceTarget(string text, int start, int end)
        {
            return scintilla.ReplaceTarget(text, start, end);
        }

        public void DeleteRange(int position, int length) 
        {
            scintilla.DeleteRange(position, length);
        }

        public int WordStartPosition(int position, bool onlyWordCharacters)
        {
            return scintilla.WordStartPosition(position, onlyWordCharacters);
        }

        public unsafe void AutoCompleteShow(int lenEntered, string list)
        {
            scintilla.AutoCompleteShow(lenEntered, list);
        }
        #endregion
    }
}
