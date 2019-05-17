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
        static CodeEditorHandler()
        {
            var path = Path.Combine(NSBundle.MainBundle.PrivateFrameworksPath, "Scintilla.framework", "Scintilla");
            Dlfcn.dlopen(path, 4);
        }

        public CodeEditorHandler()
        {
            Control = new ScintillaView();

            FontName = "Menlo";
            FontSize = 14;
            LineNumberColumnWidth = 40;
            TabWidth = 4;
            ReplaceTabsWithSpaces = true;
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

        public bool IsWhitespaceVisible => Control.GetGeneralProperty(NativeMethods.SCI_GETVIEWWS) == 1;

        public void ShowWhitespace()
        {
            Control.SetGeneralProperty(NativeMethods.SCI_SETVIEWWS, 1);
        }

        public void HideWhitespace()
        {
            Control.SetGeneralProperty(NativeMethods.SCI_SETVIEWWS, 0);
        }

        public void ShowWhitespaceWithColor(Eto.Drawing.Color color)
        {
            ShowWhitespace();
            Control.SetColorProperty(NativeMethods.SCI_SETWHITESPACEBACK, NativeMethods.SCWS_VISIBLEALWAYS, color.ToHex());
        }

        public void Rnd()
        {
            //Control.SetGeneralProperty(NativeMethods.SCI_SETUSETABS, 0); // don't use tabs
            int i = (int)Control.GetGeneralProperty(NativeMethods.SCI_GETUSETABS);
            Console.WriteLine($"usetabs {i == 1}");
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

        public int TabWidth
        {
            get
            {
                return (int)Control.GetGeneralProperty(NativeMethods.SCI_GETTABWIDTH);
            }
            set
            {
                Control.SetGeneralProperty(NativeMethods.SCI_SETTABWIDTH, value);
            }
        }

        public bool ReplaceTabsWithSpaces
        {
            get
            {
                return (int)Control.GetGeneralProperty(NativeMethods.SCI_GETUSETABS) == 0;
            }
            set
            {
                Control.SetGeneralProperty(NativeMethods.SCI_SETUSETABS, value ? 0 : 1);
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
        private const int ErrorIndex = 20;
        private const int WarningIndex = 21;
        public void SetupIndicatorStyles()
        {
            SetupIndicator(ErrorIndex, NativeMethods.INDIC_SQUIGGLE, Eto.Drawing.Colors.Red);
            SetupIndicator(WarningIndex, NativeMethods.INDIC_SQUIGGLE, Eto.Drawing.Colors.Gold);
        }

        void SetupIndicator(uint index, int style, Eto.Drawing.Color forecolor)
        {
            Control.Message(NativeMethods.SCI_INDICSETSTYLE, new IntPtr(index), new IntPtr(style));
            int abgr = forecolor.Rb | forecolor.Gb << 8 | forecolor.Bb << 16;
            Control.Message(NativeMethods.SCI_INDICSETFORE, new IntPtr(index), new IntPtr(abgr));
            Control.Message(NativeMethods.SCI_INDICSETALPHA, new IntPtr(index), new IntPtr(0));
            Control.Message(NativeMethods.SCI_INDICSETUNDER, new IntPtr(index), new IntPtr(1));
        }

        public void ClearAllErrorIndicators()
        {
            Control.Message(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(ErrorIndex), IntPtr.Zero);
            int length = Text.Length;
            Control.Message(NativeMethods.SCI_INDICATORCLEARRANGE, IntPtr.Zero, new IntPtr(length));
        }

        public void ClearAllWarningIndicators()
        {
            Control.Message(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(WarningIndex), IntPtr.Zero);
            int length = Text.Length;
            Control.Message(NativeMethods.SCI_INDICATORCLEARRANGE, IntPtr.Zero, new IntPtr(length));
        }

        public void AddErrorIndicator(int position, int length)
        {
            Control.Message(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(ErrorIndex), IntPtr.Zero);
            Control.Message(NativeMethods.SCI_INDICATORFILLRANGE, new IntPtr(position), new IntPtr(length));
        }

        public void AddWarningIndicator(int position, int length)
        {
            Control.Message(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(WarningIndex), IntPtr.Zero);
            Control.Message(NativeMethods.SCI_INDICATORFILLRANGE, new IntPtr(position), new IntPtr(length));
        }

        public void ClearAllTypeNameIndicators() { }
        public void AddTypeNameIndicator(int position, int length) { }


        public event EventHandler TextChanged
        {
            add { }
            remove { }
        }
    }
}
