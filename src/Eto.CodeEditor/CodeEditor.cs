using System;
using System.Reflection;
using Eto.Forms;

namespace Eto.CodeEditor
{
    [Handler(typeof(IHandler))]
    public class CodeEditor : Control
    {
        static string[] GetKeywords(ProgrammingLanguage language)
        {
            switch (language)
            {
                case ProgrammingLanguage.CSharp:
                    return new string[]
                    {
                    "abstract as base break case catch checked continue default delegate do else event explicit extern false finally fixed for foreach goto if implicit in interface internal is lock namespace new null object operator out override params private protected public readonly ref return sealed sizeof stackalloc switch this throw true try typeof unchecked unsafe using virtual while",
                    "bool byte char class const decimal double enum float int long sbyte short static string struct uint ulong ushort void"
                    };
                default:
                    return new string[0];
            }
        }

        readonly ProgrammingLanguage _language;
        public CodeEditor(ProgrammingLanguage language)
        {
            _language = language;
            Handler.SetProgrammingLanguage( language, GetKeywords(language) );

            SetColor(Section.Comment, Drawing.Colors.Gray, Drawing.Colors.White);
            SetColor(Section.Keyword, Drawing.Colors.SeaGreen, Drawing.Colors.White);
            SetColor(Section.LineNumber, Drawing.Colors.Gray, Drawing.Colors.White);
        }

        new IHandler Handler => (IHandler)base.Handler;

        public string Text
        {
            get => Handler.Text;
            set => Handler.Text = value;
        }

        public ProgrammingLanguage Language
        {
            get => _language;
        }

        public string FontName
        {
            get => Handler.FontName;
            set => Handler.FontName = value;
        }

        public int FontSize
        {
            get => Handler.FontSize;
            set => Handler.FontSize = value;
        }

        public int LineNumberColumnWidth
        {
            get => Handler.LineNumberColumnWidth;
            set => Handler.LineNumberColumnWidth = value;
        }

        public void SetColor(Section section, Eto.Drawing.Color foreground, Eto.Drawing.Color background)
        {
            Handler.SetColor(section, foreground, background);
        }

        public new interface IHandler : Control.IHandler
        {
            string Text { get; set; }
            void SetProgrammingLanguage(ProgrammingLanguage language, string[] keywordSets);
            string FontName { get; set; }
            int FontSize { get; set; }
            int LineNumberColumnWidth { get; set; }
            void SetColor(Section section, Eto.Drawing.Color foreground, Eto.Drawing.Color background);
        }
    }

    public enum Section
    {
        Comment,
        Keyword,
        LineNumber
    }

    public enum ProgrammingLanguage
    {
        None,
        CSharp,
        GLSL,
        VB,
        Python
    }
}
