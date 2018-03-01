using System;
using System.Reflection;
using Eto.Forms;

namespace Eto.CodeEditor
{
    [Handler(typeof(IHandler))]
    public class CodeEditor : Control
    {
        new IHandler Handler => (IHandler)base.Handler;

        public string Text
        {
            get => Handler.Text;
            set => Handler.Text = value;
        }

        public void SetKeywords(int set, string keywords)
        {
            Handler.SetKeywords(set, keywords);
        }

        public Lexer Lexer
        {
            get => Handler.Lexer;
            set => Handler.Lexer = value;
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

        public void SetColor(Section section, Eto.Drawing.Color foreground, Eto.Drawing.Color background)
        {
            Handler.SetColor(section, foreground, background);
        }

        public new interface IHandler : Control.IHandler
        {
            string Text { get; set; }
            void SetKeywords(int set, string keywords);
            Lexer Lexer { get; set; }
            string FontName { get; set; }
            int FontSize { get; set; }
            void SetColor(Section section, Eto.Drawing.Color foreground, Eto.Drawing.Color background);
        }
    }

    public enum Section
    {
        Comment,
        Keyword
    }

    public enum Lexer
    {
        Cpp,
        VB,
        Python
    }
}
