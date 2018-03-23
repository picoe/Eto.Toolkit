using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Forms;
using Eto;
using Eto.CodeEditor;
using Eto.CodeEditor.XamMac2;
using AppKit;
using ScintillaNET;
using Foundation;
using System.IO;
using ObjCRuntime;
using System.Runtime.InteropServices;

[assembly: ExportHandler(typeof(CodeEditor), typeof(CodeEditorHandler))]

namespace Eto.CodeEditor.XamMac2
{
    public class CodeEditorHandler : Eto.Mac.Forms.MacView<ScintillaView, CodeEditor, CodeEditor.ICallback>, CodeEditor.IHandler
    {
        public CodeEditorHandler()
        {
            Control = new ScintillaView();

            FontName = "Menlo";
            FontSize = 14;
            LineNumberColumnWidth = 40;
        }

        public string Text
        {
            get => Control.Text;
            set
            {
                Control.Text = value ?? string.Empty;
            }
        }

        public override NSView ContainerControl => Control;

        public override bool Enabled { get; set; }

        public void SetKeywords(int set, string keywords)
        {
            Control.SetKeywords(set, keywords);
        }

        public void SetProgrammingLanguage(ProgrammingLanguage language, string[] keywordSets)
        {
            int which = ScintillaNET.NativeMethods.SCLEX_CPP;
            switch (language)
            {
                case ProgrammingLanguage.CSharp:
                case ProgrammingLanguage.GLSL:
                    which = ScintillaNET.NativeMethods.SCLEX_CPP;
                    break;
                case ProgrammingLanguage.VB:
                    which = ScintillaNET.NativeMethods.SCLEX_VB;
                    break;
                case ProgrammingLanguage.Python:
                    which = ScintillaNET.NativeMethods.SCLEX_PYTHON;
                    break;
            }
            Control.SetGeneralProperty(ScintillaNET.NativeMethods.SCI_SETLEXER, which, 0);

            if (keywordSets != null)
            {
                for (int i = 0; i < keywordSets.Length; i++)
                {
                    SetKeywords(i, keywordSets[i]);
                }
            }
        }


        public string FontName
        {
            get
            {
                return Control.GetStringProperty(ScintillaNET.NativeMethods.SCI_STYLEGETFONT, ScintillaNET.NativeMethods.STYLE_DEFAULT);
            }
            set
            {
                Control.SetStringProperty(ScintillaNET.NativeMethods.SCI_STYLEGETFONT, ScintillaNET.NativeMethods.STYLE_DEFAULT, value);
            }
        }

        public int FontSize
        {
            get
            {
                return (int)Control.GetGeneralProperty(ScintillaNET.NativeMethods.SCI_STYLEGETSIZE);
            }
            set
            {
                Control.SetGeneralProperty(ScintillaNET.NativeMethods.SCI_STYLESETSIZE, ScintillaNET.NativeMethods.STYLE_DEFAULT, value);
            }
        }

        public int LineNumberColumnWidth
        {
            get
            {
                return (int)Control.GetGeneralProperty(NativeMethods.SCI_GETMARGINWIDTHN);
            }
            set
            {
                Control.SetGeneralProperty(NativeMethods.SCI_SETMARGINWIDTHN, 0, value);
            }
        }

        public void SetColor(Section section, Eto.Drawing.Color foreground, Eto.Drawing.Color background)
        {
            string fg = foreground.ToHex(false);
            string bg = background.ToHex(false);
            //NSColor fg = NSColor.FromRgba(foreground.R, foreground.G, foreground.B, foreground.A);
            //NSColor bg = NSColor.FromRgba(background.R, background.G, background.B, background.A);
            if (section == Section.Comment)
            {
                if (foreground != Eto.Drawing.Colors.Transparent)
                {
                    Control.SetColorProperty(NativeMethods.SCI_STYLESETFORE, NativeMethods.SCE_PROPS_COMMENT, fg);
                    Control.SetColorProperty(NativeMethods.SCI_STYLESETFORE, NativeMethods.SCE_C_COMMENTLINE, fg);
                }
                if (background != Eto.Drawing.Colors.Transparent)
                {
                    Control.SetColorProperty(NativeMethods.SCI_STYLESETBACK, NativeMethods.SCE_PROPS_COMMENT, bg);
                    Control.SetColorProperty(NativeMethods.SCI_STYLESETBACK, NativeMethods.SCE_C_COMMENTLINE, bg);
                }
            }

            if (section == Section.Keyword)
            {
                if (foreground != Eto.Drawing.Colors.Transparent)
                {
                    Control.SetColorProperty(NativeMethods.SCI_STYLESETFORE, NativeMethods.SCE_C_WORD, fg);
                    Control.SetColorProperty(NativeMethods.SCI_STYLESETFORE, NativeMethods.SCE_C_WORD2, fg);
                }
                if (background != Eto.Drawing.Colors.Transparent)
                {
                    Control.SetColorProperty(NativeMethods.SCI_STYLESETBACK, NativeMethods.SCE_C_WORD, bg);
                    Control.SetColorProperty(NativeMethods.SCI_STYLESETBACK, NativeMethods.SCE_C_WORD2, bg);
                }
            }

            if (section == Section.LineNumber)
            {
                if (foreground != Eto.Drawing.Colors.Transparent)
                {
                    Control.SetColorProperty(NativeMethods.SCI_STYLESETFORE, NativeMethods.STYLE_LINENUMBER, fg);
                }
                if (background != Eto.Drawing.Colors.Transparent)
                {
                    Control.SetColorProperty(NativeMethods.SCI_STYLESETBACK, NativeMethods.STYLE_LINENUMBER, bg);
                }
            }
        }


    }
}
