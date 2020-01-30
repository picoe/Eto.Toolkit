using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto;
using Eto.Forms;
using Eto.CodeEditor;
using Eto.CodeEditor.Wpf;
using ScintillaNET;
using Eto.Drawing;
using System.Windows.Forms;
using Eto.Wpf.Forms;
using Eto.CodeEditor.Mac;

[assembly: ExportHandler(typeof(CodeEditor), typeof(CodeEditorHandler))]

namespace Eto.CodeEditor.Wpf
{
    public class CodeEditorHandler : Eto.Wpf.Forms.WindowsFormsHostHandler<ScintillaControl, CodeEditor, CodeEditor.ICallback>, CodeEditor.IHandler
    {
        private const int BREAKPOINT_MARKER = 3; // arbitrary number
        private const int BREAK_MARKER = 4; // arbitrary number

        private const int BREAKPOINTS_MARGIN = 1;
        private const int LINENUMBERS_MARGIN = 2;

        private ScintillaControl scintilla;

        public CodeEditorHandler()
        {
            //string path = ScintillaControl.UnpackNativeScintilla();
            //ScintillaNET.Scintilla.SetModulePath(path);
            scintilla = new ScintillaControl(); // new ScintillaNET.Scintilla();
            WinFormsControl = scintilla;

            /*scintilla*/this.CharAdded += WinFormsControl_CharAdded;
            scintilla.TextChanged += WinFormsControl_TextChanged;
            /*scintilla.*/this.InsertCheck += WinFormsControl_InsertCheck;
            //scintilla.AutoCMaxHeight = 10;
            scintilla.DirectMessage(NativeMethods.SCI_AUTOCSETMAXHEIGHT, new IntPtr(10));
            //scintilla.AutomaticFold = AutomaticFold.Click;
            scintilla.DirectMessage(NativeMethods.SCI_SETAUTOMATICFOLD, new IntPtr(NativeMethods.SC_AUTOMATICFOLD_CLICK));
            
            FontName = "Consolas";
            FontSize = 11;
            LineNumberColumnWidth = 40;

            Action<int, int, int> SetGeneralProperty = (c, i, j) => scintilla.DirectMessage(c, new IntPtr(i), new IntPtr(j));
            // breakpoints margin
            SetGeneralProperty(NativeMethods.SCI_SETMARGINSENSITIVEN, BREAKPOINTS_MARGIN, 1);
            SetGeneralProperty(NativeMethods.SCI_SETMARGINTYPEN, BREAKPOINTS_MARGIN, NativeMethods.SC_MARGIN_SYMBOL);
            SetGeneralProperty(NativeMethods.SCI_SETMARGINMASKN, BREAKPOINTS_MARGIN, int.MaxValue); // ScintillaNet -> public const uint MaskAll = unchecked((uint)-1);
            SetGeneralProperty(NativeMethods.SCI_MARKERDEFINE, BREAKPOINTS_MARGIN, NativeMethods.SC_MARK_FULLRECT);
            IsBreakpointsMarginVisible = false;

            // line numbers margin
            SetGeneralProperty(NativeMethods.SCI_SETMARGINSENSITIVEN, LINENUMBERS_MARGIN, 0);
            SetGeneralProperty(NativeMethods.SCI_SETMARGINTYPEN, LINENUMBERS_MARGIN, NativeMethods.SC_MARGIN_NUMBER);

            // breakpoint marker
            SetGeneralProperty(NativeMethods.SCI_MARKERDEFINE, BREAKPOINT_MARKER, NativeMethods.SC_MARK_CIRCLE); // default
            var red = 255; // 0xFF0000; // */ 16711680;
            SetGeneralProperty(NativeMethods.SCI_MARKERSETFORE, BREAKPOINT_MARKER, red);
            SetGeneralProperty(NativeMethods.SCI_MARKERSETBACK, BREAKPOINT_MARKER, red);

            // break marker
            SetGeneralProperty(NativeMethods.SCI_MARKERDEFINE, BREAK_MARKER, NativeMethods.SC_MARK_ARROW);
            var yellow = 0x00FFFF; // */ 16776960;
            SetGeneralProperty(NativeMethods.SCI_MARKERSETFORE, BREAK_MARKER, 0xFFFFFF); //black
            SetGeneralProperty(NativeMethods.SCI_MARKERSETBACK, BREAK_MARKER, yellow);

        }

        private void WinFormsControl_InsertCheck(object sender, /*ScintillaNET.*/InsertCheckEventArgs e)
        {
            InsertCheck?.Invoke(this, new Eto.CodeEditor.InsertCheckEventArgs(e.Text));
        }

        public void ChangeInsertion(string text)
        {
            // this method shouldn't be part of the handler interface as it's not needed on windows.
            // on windows the handler set `e.Text = "some text"` and the setter calls the native ChangeInsertion
            throw new NotImplementedException("InsertCheck handler needs to be reworked.");
        }

        private int MarkerNext(int lineNumber) => 
            scintilla.DirectMessage(NativeMethods.SCI_MARKERNEXT, new IntPtr(lineNumber), new IntPtr(1 << BREAKPOINT_MARKER)).ToInt32();
        public IEnumerable<int> Breakpoints
        {
            get
            {
                int lineIndex = MarkerNext(0);
                while (lineIndex != -1)
                {
                    // increment lineIndex before returning it because line numbers start at 1 on the client
                    lineIndex++;
                    yield return lineIndex;
                    // start searching on the next (incremented) index
                    lineIndex = MarkerNext(lineIndex);
                }
            }
        }

        private void WinFormsControl_CharAdded(object sender, /*ScintillaNET.*/CharAddedEventArgs e)
        {
            CharAdded?.Invoke(this, new Eto.CodeEditor.CharAddedEventArgs((char)e.Char));
        }

        private void WinFormsControl_TextChanged(object sender, EventArgs e)
        {
            TextChanged?.Invoke(this, new Eto.CodeEditor.TextChangedEventArgs());

        }

        public void SetProgrammingLanguage(ProgrammingLanguage language, string[] keywordSets)
        {
            Language = language;
            if(keywordSets!=null)
            {
                for( int i=0; i<keywordSets.Length; i++ )
                {
                    SetKeywords(i, keywordSets[i]);
                }
            }
        }

        public string Text
        {
            get => scintilla.Text;
            set => scintilla.Text = value;
        }

        public unsafe void SetKeywords(int set, string keywords)
        {
            //scintilla.SetKeywords(set, keywords);
            set = Helpers.Clamp(set, 0, NativeMethods.KEYWORDSET_MAX);
            var bytes = Helpers.GetBytes(keywords ?? string.Empty, Encoding.ASCII, zeroTerminated: true);

            fixed (byte* bp = bytes)
                scintilla.DirectMessage(NativeMethods.SCI_SETKEYWORDS, new IntPtr(set), new IntPtr(bp));
        }

        ProgrammingLanguage _language = ProgrammingLanguage.None;
        public ProgrammingLanguage Language
        {
            get { return _language; }
            set
            {
                _language = value;
                int which = ScintillaNET.NativeMethods.SCLEX_CPP;
                switch (_language)
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
                scintilla.DirectMessage(ScintillaNET.NativeMethods.SCI_SETLEXER, new IntPtr(which));
            }
        }

        public string FontName
        {
            //get { return scintilla.Styles[ScintillaNET.Style.Default].Font; }
            //set { scintilla.Styles[ScintillaNET.Style.Default].Font = value; }
            get
            {
                var length = scintilla.DirectMessage(NativeMethods.SCI_STYLEGETFONT, new IntPtr(ScintillaNET.NativeMethods.STYLE_DEFAULT), IntPtr.Zero).ToInt32();
                var font = new byte[length];
                unsafe
                {
                    fixed (byte* bp = font)
                        scintilla.DirectMessage(NativeMethods.SCI_STYLEGETFONT, new IntPtr(ScintillaNET.NativeMethods.STYLE_DEFAULT), new IntPtr(bp));
                }

                var name = Encoding.UTF8.GetString(font, 0, length);
                return name;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    value = "Verdana";

                // Scintilla expects UTF-8
                var font = Helpers.GetBytes(value, Encoding.UTF8, true);
                unsafe
                {
                    fixed (byte* bp = font)
                        scintilla.DirectMessage(NativeMethods.SCI_STYLESETFONT, new IntPtr(ScintillaNET.NativeMethods.STYLE_DEFAULT), new IntPtr(bp));
                }
            }
        }

        public int FontSize
        {
            //get { return scintilla.Styles[ScintillaNET.Style.Default].Size; }
            //set { scintilla.Styles[ScintillaNET.Style.Default].Size = value; }
            get
            {
                return scintilla.DirectMessage(NativeMethods.SCI_STYLEGETSIZE, new IntPtr(ScintillaNET.NativeMethods.STYLE_DEFAULT), IntPtr.Zero).ToInt32();
            }
            set
            {
                scintilla.DirectMessage(NativeMethods.SCI_STYLESETSIZE, new IntPtr(ScintillaNET.NativeMethods.STYLE_DEFAULT), new IntPtr(value));
            }
        }

        public void SetColor(Section section, Eto.Drawing.Color foreground, Eto.Drawing.Color background)
        {
            //var fg = System.Drawing.Color.FromArgb(forecolor.Rb, forecolor.Gb, forecolor.Bb);
            //var bg = System.Drawing.Color.FromArgb(backcolor.Rb, backcolor.Gb, backcolor.Bb);

            //if ( section == Section.Default)
            //{
            //    scintilla.Styles[ScintillaNET.Style.Default].ForeColor = fg;
            //    scintilla.CaretForeColor = fg;
            //    scintilla.Styles[ScintillaNET.Style.Default].BackColor = bg;
            //    scintilla.StyleClearAll();
            //}
            //if (section == Section.Comment)
            //{
            //    foreach (var id in CommentStyleIds(Language))
            //    {
            //        scintilla.Styles[id].ForeColor = fg;
            //        scintilla.Styles[id].BackColor = bg;
            //    }
            //}
            //if (section == Section.Keyword1)
            //{
            //    foreach (var id in Keyword1Ids(Language))
            //    {
            //        scintilla.Styles[id].ForeColor = fg;
            //        scintilla.Styles[id].BackColor = bg;
            //    }
            //}
            //if (section == Section.Keyword2)
            //{
            //    foreach (var id in Keyword2Ids(Language))
            //    {
            //        scintilla.Styles[id].ForeColor = fg;
            //        scintilla.Styles[id].BackColor = bg;
            //    }
            //}
            //if (section == Section.Strings)
            //{
            //    foreach (var id in StringStyleIds(Language))
            //    {
            //        scintilla.Styles[id].ForeColor = fg;
            //        scintilla.Styles[id].BackColor = bg;
            //    }
            //}
            //if (section == Section.LineNumber)
            //{
            //    scintilla.Styles[ScintillaNET.Style.LineNumber].ForeColor = fg;
            //    scintilla.Styles[ScintillaNET.Style.LineNumber].BackColor = bg;
            //}
            //if( section == Section.DefName && Language == ProgrammingLanguage.Python)
            //{
            //    scintilla.Styles[ScintillaNET.Style.Python.DefName].ForeColor = fg;
            //    scintilla.Styles[ScintillaNET.Style.Python.DefName].BackColor = bg;
            //}
            //if(section == Section.Preprocessor)
            //{
            //    foreach (var id in PreprocessorIds(Language))
            //    {
            //        scintilla.Styles[id].ForeColor = fg;
            //        scintilla.Styles[id].BackColor = bg;
            //    }

            //}

            //string fg = foreground.ToHex(false);
            //string bg = background.ToHex(false);
            int fg = ColorTranslator.ToWin32(System.Drawing.Color.FromArgb(foreground.ToArgb()));
            int bg = ColorTranslator.ToWin32(System.Drawing.Color.FromArgb(background.ToArgb()));

            if (section == Section.Default)
            {
                scintilla.DirectMessage(NativeMethods.SCI_STYLESETFORE, new IntPtr(NativeMethods.STYLE_DEFAULT), new IntPtr(fg));
                int argb = foreground.ToArgb();
                scintilla.DirectMessage(NativeMethods.SCI_SETCARETFORE, new IntPtr(argb), new IntPtr(0));
                scintilla.DirectMessage(NativeMethods.SCI_STYLESETBACK, new IntPtr(NativeMethods.STYLE_DEFAULT), new IntPtr(bg));
                scintilla.DirectMessage(NativeMethods.SCI_STYLECLEARALL, new IntPtr(0), new IntPtr(0));
            }
            if (section == Section.Comment)
            {
                foreach (var id in CommentStyleIds(Language))
                {
                    scintilla.DirectMessage(NativeMethods.SCI_STYLESETFORE, new IntPtr(id), new IntPtr(fg));
                    scintilla.DirectMessage(NativeMethods.SCI_STYLESETBACK, new IntPtr(id), new IntPtr(bg));
                }
            }
            if (section == Section.Keyword1)
            {
                foreach (var id in Keyword1Ids(Language))
                {
                    scintilla.DirectMessage(NativeMethods.SCI_STYLESETFORE, new IntPtr(id), new IntPtr(fg));
                    scintilla.DirectMessage(NativeMethods.SCI_STYLESETBACK, new IntPtr(id), new IntPtr(bg));
                }
            }
            if (section == Section.Keyword2)
            {
                foreach (var id in Keyword2Ids(Language))
                {
                    scintilla.DirectMessage(NativeMethods.SCI_STYLESETFORE, new IntPtr(id), new IntPtr(fg));
                    scintilla.DirectMessage(NativeMethods.SCI_STYLESETBACK, new IntPtr(id), new IntPtr(bg));
                }
            }
            if (section == Section.Strings)
            {
                foreach (var id in StringStyleIds(Language))
                {
                    scintilla.DirectMessage(NativeMethods.SCI_STYLESETFORE, new IntPtr(id), new IntPtr(fg));
                    scintilla.DirectMessage(NativeMethods.SCI_STYLESETBACK, new IntPtr(id), new IntPtr(bg));
                }
            }
            if (section == Section.LineNumber)
            {
                scintilla.DirectMessage(NativeMethods.SCI_STYLESETFORE, new IntPtr(NativeMethods.STYLE_LINENUMBER), new IntPtr(fg));
                scintilla.DirectMessage(NativeMethods.SCI_STYLESETBACK, new IntPtr(NativeMethods.STYLE_LINENUMBER), new IntPtr(bg));
            }
            if (section == Section.DefName && Language == ProgrammingLanguage.Python)
            {
                scintilla.DirectMessage(NativeMethods.SCI_STYLESETFORE, new IntPtr(NativeMethods.SCE_P_DEFNAME), new IntPtr(fg));
                scintilla.DirectMessage(NativeMethods.SCI_STYLESETBACK, new IntPtr(NativeMethods.SCE_P_DEFNAME), new IntPtr(bg));
            }
            if (section == Section.Preprocessor)
            {
                foreach (var id in PreprocessorIds(Language))
                {
                    scintilla.DirectMessage(NativeMethods.SCI_STYLESETFORE, new IntPtr(id), new IntPtr(fg));
                    scintilla.DirectMessage(NativeMethods.SCI_STYLESETBACK, new IntPtr(id), new IntPtr(bg));
                }

            }
        }

        static int[] CommentStyleIds(ProgrammingLanguage language)
        {
            //if (language == ProgrammingLanguage.Python)
            //    return new int[] { ScintillaNET.Style.Python.CommentBlock, ScintillaNET.Style.Python.CommentLine};

            //if (language == ProgrammingLanguage.VB)
            //    return new int[] { ScintillaNET.Style.Vb.Comment, ScintillaNET.Style.Vb.CommentBlock,
            //        ScintillaNET.Style.Vb.DocBlock, ScintillaNET.Style.Vb.Preprocessor};

            //return new int[] {ScintillaNET.Style.Cpp.Comment, ScintillaNET.Style.Cpp.CommentLine,
            //    ScintillaNET.Style.Cpp.CommentDoc, ScintillaNET.Style.Cpp.CommentLineDoc};
            if (language == ProgrammingLanguage.Python)
                return new int[] { NativeMethods.SCE_P_COMMENTBLOCK, NativeMethods.SCE_P_COMMENTLINE };

            if (language == ProgrammingLanguage.VB)
                return new int[] { NativeMethods.SCE_B_COMMENT, NativeMethods.SCE_B_COMMENTBLOCK,
                    NativeMethods.SCE_B_DOCBLOCK};

            return new int[] { NativeMethods.SCE_C_COMMENT, NativeMethods.SCE_C_COMMENTLINE,
                NativeMethods.SCE_C_COMMENTDOC, NativeMethods.SCE_C_COMMENTLINEDOC };
        }

        static int[] StringStyleIds(ProgrammingLanguage language)
        {
            //if (language == ProgrammingLanguage.Python)
            //    return new int[] { ScintillaNET.Style.Python.Character, ScintillaNET.Style.Python.String,
            //        ScintillaNET.Style.Python.Triple, ScintillaNET.Style.Python.TripleDouble };

            //if (language == ProgrammingLanguage.VB)
            //    return new int[] { ScintillaNET.Style.Vb.String };
            //return new int[] {ScintillaNET.Style.Cpp.String, ScintillaNET.Style.Cpp.Character};

            if (language == ProgrammingLanguage.Python)
                return new int[] { NativeMethods.SCE_P_CHARACTER, NativeMethods.SCE_P_STRING,
                    NativeMethods.SCE_P_TRIPLE, NativeMethods.SCE_P_TRIPLEDOUBLE };

            if (language == ProgrammingLanguage.VB)
                return new int[] { NativeMethods.SCE_B_STRING };
            return new int[] { NativeMethods.SCE_C_STRING, NativeMethods.SCE_C_CHARACTER };
        }

        static int[] Keyword1Ids(ProgrammingLanguage language)
        {
            //if (language == ProgrammingLanguage.Python)
            //    return new int[] { ScintillaNET.Style.Python.Word };

            //if (language == ProgrammingLanguage.VB)
            //    return new int[] { ScintillaNET.Style.Vb.Keyword };

            //return new int[] { ScintillaNET.Style.Cpp.Word };

            if (language == ProgrammingLanguage.Python)
                return new int[] { NativeMethods.SCE_P_WORD };

            if (language == ProgrammingLanguage.VB)
                return new int[] { NativeMethods.SCE_B_KEYWORD };

            return new int[] { NativeMethods.SCE_C_WORD };
        }

        static int[] Keyword2Ids(ProgrammingLanguage language)
        {
            //if (language == ProgrammingLanguage.Python)
            //    return new int[] { ScintillaNET.Style.Python.Word2 };

            //if (language == ProgrammingLanguage.VB)
            //    return new int[] { ScintillaNET.Style.Vb.Keyword2, ScintillaNET.Style.Vb.Keyword3, ScintillaNET.Style.Vb.Keyword4 };

            //return new int[] { ScintillaNET.Style.Cpp.Word2 };

            if (language == ProgrammingLanguage.Python)
                return new int[] { NativeMethods.SCE_P_WORD2 };

            if (language == ProgrammingLanguage.VB)
                return new int[] { NativeMethods.SCE_B_KEYWORD2, NativeMethods.SCE_B_KEYWORD3, NativeMethods.SCE_B_KEYWORD4 };

            return new int[] { NativeMethods.SCE_C_WORD2 };
        }

        static int[] PreprocessorIds(ProgrammingLanguage language)
        {
            if (language == ProgrammingLanguage.Python)
                return new int[] { };

            if (language == ProgrammingLanguage.VB)
                return new int[] { /*ScintillaNET.Style.Vb.Preprocessor*/NativeMethods.SCE_B_PREPROCESSOR };

            return new int[] { /*ScintillaNET.Style.Cpp.Preprocessor*/NativeMethods.SCE_C_PREPROCESSOR };
        }

        private const int ErrorIndex = 20;
        private const int WarningIndex = 21;
        private const int TypeNameIndex = 22;

        public event EventHandler<Eto.CodeEditor.CharAddedEventArgs> CharAdded;
        //public event EventHandler<Eto.CodeEditor.TextChangedEventArgs> TextChanged;
        public event EventHandler<EventArgs> TextChanged;
        public event EventHandler<Eto.CodeEditor.InsertCheckEventArgs> InsertCheck;

        public void SetupIndicatorStyles()
        {
            //scintilla.Indicators[ErrorIndex].Style = IndicatorStyle.CompositionThick;
            scintilla.DirectMessage(NativeMethods.SCI_INDICSETSTYLE, new IntPtr(ErrorIndex), new IntPtr(NativeMethods.INDIC_COMPOSITIONTHICK));
            //scintilla.Indicators[ErrorIndex].ForeColor = System.Drawing.Color.Crimson;
            scintilla.DirectMessage(NativeMethods.SCI_INDICSETFORE, new IntPtr(ErrorIndex), new IntPtr(ColorTranslator.ToWin32(System.Drawing.Color.Crimson)));
            //scintilla.Indicators[ErrorIndex].Alpha = 255;
   
            //scintilla.Indicators[WarningIndex].Style = IndicatorStyle.CompositionThick;
            scintilla.DirectMessage(NativeMethods.SCI_INDICSETSTYLE, new IntPtr(WarningIndex), new IntPtr(NativeMethods.INDIC_COMPOSITIONTHICK));
            //scintilla.Indicators[WarningIndex].ForeColor = System.Drawing.Color.DarkOrange;
            scintilla.DirectMessage(NativeMethods.SCI_INDICSETFORE, new IntPtr(WarningIndex), new IntPtr(ColorTranslator.ToWin32(System.Drawing.Color.DarkOrange)));
            //scintilla.Indicators[WarningIndex].Alpha = 255;

            //scintilla.Indicators[TypeNameIndex].Style = IndicatorStyle.TextFore;
            scintilla.DirectMessage(NativeMethods.SCI_INDICSETSTYLE, new IntPtr(TypeNameIndex), new IntPtr(NativeMethods.INDIC_TEXTFORE));
            //scintilla.Indicators[TypeNameIndex].ForeColor = System.Drawing.Color.FromArgb(43, 145, 175);
            scintilla.DirectMessage(NativeMethods.SCI_INDICSETFORE, new IntPtr(TypeNameIndex), new IntPtr(ColorTranslator.ToWin32(System.Drawing.Color.FromArgb(43, 145, 175))));
        }
        public void ClearAllErrorIndicators()
        {
            //scintilla.IndicatorCurrent = ErrorIndex;
            scintilla.DirectMessage(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(ErrorIndex));
            //scintilla.IndicatorClearRange(0, scintilla.TextLength);
            scintilla.DirectMessage(NativeMethods.SCI_INDICATORCLEARRANGE, IntPtr.Zero, new IntPtr(Text.Length));
        }
        public void ClearAllWarningIndicators()
        {
            //scintilla.IndicatorCurrent = WarningIndex;
            scintilla.DirectMessage(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(WarningIndex));
            //scintilla.IndicatorClearRange(0, scintilla.TextLength);
            scintilla.DirectMessage(NativeMethods.SCI_INDICATORCLEARRANGE, IntPtr.Zero, new IntPtr(Text.Length));
        }
        public void ClearAllTypeNameIndicators()
        {
            //scintilla.IndicatorCurrent = TypeNameIndex;
            scintilla.DirectMessage(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(TypeNameIndex));
            //scintilla.IndicatorClearRange(0, scintilla.TextLength);
            scintilla.DirectMessage(NativeMethods.SCI_INDICATORCLEARRANGE, IntPtr.Zero, new IntPtr(Text.Length));
        }

        public void AddErrorIndicator(int position, int length)
        {
            //scintilla.IndicatorCurrent = ErrorIndex;
            scintilla.DirectMessage(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(ErrorIndex));
            //scintilla.IndicatorFillRange(position, length);
            scintilla.DirectMessage(NativeMethods.SCI_INDICATORFILLRANGE, new IntPtr(position), new IntPtr(length));
        }
        public void AddWarningIndicator(int position, int length)
        {
            //scintilla.IndicatorCurrent = WarningIndex;
            scintilla.DirectMessage(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(WarningIndex));
            //scintilla.IndicatorFillRange(position, length);
            scintilla.DirectMessage(NativeMethods.SCI_INDICATORFILLRANGE, new IntPtr(position), new IntPtr(length));
        }
        public void AddTypeNameIndicator(int position, int length)
        {
            //scintilla.IndicatorCurrent = TypeNameIndex;
            scintilla.DirectMessage(NativeMethods.SCI_SETINDICATORCURRENT, new IntPtr(TypeNameIndex));
            //scintilla.IndicatorFillRange(position, length);
            scintilla.DirectMessage(NativeMethods.SCI_INDICATORFILLRANGE, new IntPtr(position), new IntPtr(length));
        }

        public int LineNumberColumnWidth
        {
            get
            {
                //return scintilla.Margins[0].Width;
                return scintilla.DirectMessage(NativeMethods.SCI_GETMARGINWIDTHN, new IntPtr(LINENUMBERS_MARGIN), IntPtr.Zero).ToInt32();
            }
            set
            {
                //scintilla.Margins[0].Width = value;
                scintilla.DirectMessage(NativeMethods.SCI_SETMARGINWIDTHN, new IntPtr(LINENUMBERS_MARGIN), new IntPtr(value));
            }
        }

        public int TabWidth
        {
            get => scintilla.DirectMessage(NativeMethods.SCI_GETTABWIDTH).ToInt32();
            set => scintilla.DirectMessage(NativeMethods.SCI_SETTABWIDTH, new IntPtr(value));
        }
        
        public bool ReplaceTabsWithSpaces
        {
            get => scintilla.DirectMessage(NativeMethods.SCI_GETUSETABS) != IntPtr.Zero;
            set
            {
                var useTabs = (value ? new IntPtr(1) : IntPtr.Zero);
                scintilla.DirectMessage(NativeMethods.SCI_SETUSETABS, useTabs);
            }
        }

        public bool BackspaceUnindents
        {
          get => false;
          set
            {
                //pass
            }
        }
        public int CurrentPosition
        {
            get => scintilla.DirectMessage(NativeMethods.SCI_GETCURRENTPOS).ToInt32();
            set => scintilla.DirectMessage(NativeMethods.SCI_SETCURRENTPOS, new IntPtr(value));
        }

        public int CurrentPositionInLine => CurrentPosition - scintilla.DirectMessage(NativeMethods.SCI_POSITIONFROMLINE, new IntPtr(CurrentPosition)).ToInt32();

        public int CurrentLineNumber => scintilla.DirectMessage(NativeMethods.SCI_LINEFROMPOSITION, new IntPtr(CurrentPosition)).ToInt32();

        public bool IsWhitespaceVisible => scintilla.DirectMessage(NativeMethods.SCI_GETVIEWWS).ToInt32() != NativeMethods.SCWS_INVISIBLE;

        public void ShowWhitespace()
        {
            //scintilla.ViewWhitespace = WhitespaceMode.VisibleAlways;
            scintilla.DirectMessage(NativeMethods.SCI_SETVIEWWS, new IntPtr(NativeMethods.SCWS_VISIBLEALWAYS));
        }

        public void HideWhitespace()
        {
            //scintilla.ViewWhitespace = WhitespaceMode.Invisible;
            scintilla.DirectMessage(NativeMethods.SCI_SETVIEWWS, new IntPtr(0));
        }

        public void ShowWhitespaceWithColor(Eto.Drawing.Color color)
        {
            ShowWhitespace();
            //scintilla.SetWhitespaceBackColor(true, System.Drawing.Color.FromArgb(color.ToArgb()));
            var colour = ColorTranslator.ToWin32(System.Drawing.Color.FromArgb(color.ToArgb()));
            scintilla.DirectMessage(NativeMethods.SCI_SETWHITESPACEBACK, new IntPtr(1), new IntPtr(colour));
        }

        //public bool AreIndentationGuidesVisible => scintilla.IndentationGuides != IndentView.None;
        public bool AreIndentationGuidesVisible => scintilla.DirectMessage(NativeMethods.SCI_GETINDENTATIONGUIDES).ToInt32() != NativeMethods.SC_IV_NONE;

        public void ShowIndentationGuides()
        {
            //scintilla.IndentationGuides = IndentView.LookBoth;
            scintilla.DirectMessage(NativeMethods.SCI_SETINDENTATIONGUIDES, new IntPtr(NativeMethods.SC_IV_LOOKBOTH));
        }

        public void HideIndentationGuides()
        {
            //scintilla.IndentationGuides = IndentView.None;
            scintilla.DirectMessage(NativeMethods.SCI_SETINDENTATIONGUIDES, new IntPtr(NativeMethods.SC_IV_NONE));
        }

        public int GetLineIndentation(int lineNumber)
        {
            //var line = new Line(scintilla, lineNumber);
            //return line?.Indentation ?? 0;
            return scintilla.DirectMessage(NativeMethods.SCI_GETLINEINDENTATION, new IntPtr(lineNumber)).ToInt32();
        }

        public void SetLineIndentation(int lineNumber, int indentation)
        {
            //var line = new Line(scintilla, lineNumber);
            //if (line != null)
            //{
            //    line.Indentation = indentation;
            //    scintilla.GotoPosition(line.Position + indentation);
            //}
            scintilla.DirectMessage(NativeMethods.SCI_SETLINEINDENTATION, new IntPtr(lineNumber), new IntPtr(indentation));
        }

        public char GetLineLastChar(int lineNumber)
        {
            //var line = new Line(scintilla, lineNumber);
            //return line?.Text.Reverse().SkipWhile(c => c == '\n' || c == '\r').FirstOrDefault() ?? '\0';
            var lineEndPos = scintilla.DirectMessage(NativeMethods.SCI_GETLINEENDPOSITION, new IntPtr(lineNumber)).ToInt32();
            var lineStartPos = scintilla.DirectMessage(NativeMethods.SCI_POSITIONFROMLINE, new IntPtr(lineNumber)).ToInt32();
            char lineLastChar;
            do
            {
                lineLastChar = (char)scintilla.DirectMessage(NativeMethods.SCI_GETCHARAT, new IntPtr(lineEndPos--));
            }
            while (lineEndPos >= lineStartPos && (lineLastChar == '\n' || lineLastChar == '\r'));
            return lineLastChar;
        }


        Encoding Encoding
        {
            get
            {
                int codePage = scintilla.DirectMessage(NativeMethods.SCI_GETCODEPAGE, IntPtr.Zero, IntPtr.Zero).ToInt32();
                return (codePage == 0) ? Encoding.Default : Encoding.GetEncoding(codePage);
            }
        }

        public string GetLineText(int lineNumber)
        {
            //var line = new Line(scintilla, lineNumber);
            //return line?.Text ?? "";
            var start = scintilla.DirectMessage(NativeMethods.SCI_POSITIONFROMLINE, new IntPtr(lineNumber));
            var length = scintilla.DirectMessage(NativeMethods.SCI_LINELENGTH, new IntPtr(lineNumber));
            var ptr = scintilla.DirectMessage(NativeMethods.SCI_GETRANGEPOINTER, start, length);
            if (ptr == IntPtr.Zero)
                return string.Empty;
            var text = Mac.Helpers.GetString(ptr, length.ToInt32(), Encoding); // new string((sbyte*)ptr, 0, length.ToInt32(), scintilla.Encoding);
            return text;
        }

        public int GetLineLength(int lineNumber)
        {
            //var line = new Line(scintilla, lineNumber);
            //return line?.Length ?? 0;
            return scintilla.DirectMessage(NativeMethods.SCI_LINELENGTH, new IntPtr(lineNumber)).ToInt32();
        }

        public bool AutoCompleteActive { get { return scintilla.DirectMessage(NativeMethods.SCI_AUTOCACTIVE) != IntPtr.Zero; } }
        public unsafe void InsertText(int position, string text) 
        { 
            //scintilla.InsertText(position, text); 
            if (position < -1)
                throw new ArgumentOutOfRangeException(nameof(position), "Position must be greater or equal to -1");
            if (position != -1)
            {
                int textLength = scintilla.DirectMessage(NativeMethods.SCI_GETLENGTH, IntPtr.Zero, IntPtr.Zero).ToInt32();
                if (position > textLength)
                    throw new ArgumentOutOfRangeException(nameof(position), "Position cannot exceed document length");
            }

            fixed (byte* bp = Eto.CodeEditor.Mac.Helpers.GetBytes(text ?? string.Empty, Encoding, zeroTerminated: true))
                scintilla.DirectMessage(NativeMethods.SCI_INSERTTEXT, new IntPtr(position), new IntPtr(bp));
        }

        public void DeleteRange(int position, int length) 
        { 
            //scintilla.DeleteRange(position, length); 
            var textLength = scintilla.DirectMessage(NativeMethods.SCI_GETLENGTH).ToInt32();
            position = Mac.Helpers.Clamp(position, 0, textLength);
            length = Mac.Helpers.Clamp(length, 0, textLength - position);

            scintilla.DirectMessage(NativeMethods.SCI_DELETERANGE, new IntPtr(position), new IntPtr(length));
        }

        public void SetTargetRange(int start, int end)
        {
            var textLength = scintilla.DirectMessage(NativeMethods.SCI_GETLENGTH, IntPtr.Zero, IntPtr.Zero).ToInt32();
            start = Mac.Helpers.Clamp(start, 0, textLength);
            end = Mac.Helpers.Clamp(end, 0, textLength);

            scintilla.DirectMessage(NativeMethods.SCI_SETTARGETRANGE, new IntPtr(start), new IntPtr(end));
        }

        public unsafe int ReplaceTarget(string text, int start, int end)
        {
            //scintilla.SetTargetRange(start, end);
            //return scintilla.ReplaceTarget(text);

            SetTargetRange(start, end);
            if (text == null)
                text = string.Empty;

            var bytes = Mac.Helpers.GetBytes(text, Encoding, false);
            fixed (byte* bp = bytes)
                scintilla.DirectMessage(NativeMethods.SCI_REPLACETARGET, new IntPtr(bytes.Length), new IntPtr(bp));

            return text.Length;
        }

        public unsafe void ReplaceFirstOccuranceInLine(string oldText, string newText, int lineNumber)
        {
            //var line = scintilla.Lines[lineNumber];
            //scintilla.SetTargetRange(line.Position, line.EndPosition);

            //var pos = scintilla.SearchInTarget(oldText);
            //if (pos == -1)
            //  return;

            //scintilla.SetTargetRange(pos, pos + oldText.Length);

            //scintilla.ReplaceTarget(newText);

            var lineStartPos = scintilla.DirectMessage(NativeMethods.SCI_POSITIONFROMLINE, new IntPtr(CurrentLineNumber));
            var lineEndPos = scintilla.DirectMessage(NativeMethods.SCI_GETLINEENDPOSITION, new IntPtr(CurrentLineNumber));
            scintilla.DirectMessage(NativeMethods.SCI_SETTARGETRANGE, lineStartPos, lineEndPos);

            int bytePos = 0;
            var bytes = Mac.Helpers.GetBytes(oldText ?? string.Empty, Encoding, zeroTerminated: false);
            fixed (byte* bp = bytes)
                bytePos = scintilla.DirectMessage(NativeMethods.SCI_SEARCHINTARGET, new IntPtr(bytes.Length), new IntPtr(bp)).ToInt32();

            if (bytePos == -1)
                return;

            scintilla.DirectMessage(NativeMethods.SCI_SETTARGETRANGE, new IntPtr(bytePos), new IntPtr(bytePos + bytes.Length));

            bytes = Mac.Helpers.GetBytes(newText ?? string.Empty, Encoding, zeroTerminated:false);
            fixed (byte* bp = bytes)
                scintilla.DirectMessage(NativeMethods.SCI_REPLACETARGET, new IntPtr(bytes.Length), new IntPtr(bp));
        }

        public int WordStartPosition(int position, bool onlyWordCharacters)
        {
            //return scintilla.WordStartPosition(position, onlyWordCharacters);
            var onlyWordChars = (onlyWordCharacters ? new IntPtr(1) : IntPtr.Zero);
            int textLength = scintilla.DirectMessage(NativeMethods.SCI_GETLENGTH, IntPtr.Zero, IntPtr.Zero).ToInt32();
            position = Eto.CodeEditor.Mac.Helpers.Clamp(position, 0, textLength);
            position = scintilla.DirectMessage(NativeMethods.SCI_WORDSTARTPOSITION, new IntPtr(position), onlyWordChars).ToInt32();
            return position;
        }

        public string GetTextRange(int position, int length)
        {
            //return scintilla.GetTextRange(position, length);
            string txt = Text;
            return txt.Substring(position, length);
        }

        public unsafe void AutoCompleteShow(int lenEntered, string list)
        {
            //scintilla.AutoCShow(lenEntered, list);
            if (string.IsNullOrEmpty(list))
                return;
            lenEntered = Eto.CodeEditor.Mac.Helpers.ClampMin(lenEntered, 0);
            if( lenEntered > 0 )
            {
                int endPos = scintilla.DirectMessage(NativeMethods.SCI_GETCURRENTPOS, IntPtr.Zero, IntPtr.Zero).ToInt32();
                int startPos = endPos;
                for (int i = 0; i < lenEntered; i++)
                    startPos = scintilla.DirectMessage(NativeMethods.SCI_POSITIONRELATIVE, new IntPtr(startPos), new IntPtr(-1)).ToInt32();
                lenEntered = (endPos - startPos);
            }

            var bytes = Eto.CodeEditor.Mac.Helpers.GetBytes(list, Encoding, zeroTerminated: true);
            fixed (byte* bp = bytes)
                scintilla.DirectMessage(NativeMethods.SCI_AUTOCSHOW, new IntPtr(lenEntered), new IntPtr(bp));
            // if the following property is not set, items after 'import' that start with an uppercase
            // closes the completion window. Ex: 'import R' closes the window even though 'Rhino' is
            // in the list.
            scintilla.DirectMessage(NativeMethods.SCI_AUTOCSETIGNORECASE, new IntPtr(1), IntPtr.Zero);
        }

        public void BreakOnLine(int lineNumber)
        {
            ClearBreak();
            scintilla.DirectMessage(NativeMethods.SCI_MARKERADD, new IntPtr(lineNumber), new IntPtr(BREAK_MARKER));
        }

        public event EventHandler<BreakpointsChangedEventArgs> BreakpointsChanged;

        public void ClearBreak() => scintilla.DirectMessage(NativeMethods.SCI_MARKERDELETEALL, new IntPtr(BREAK_MARKER), IntPtr.Zero);

        public void ClearBreakpoints()
        {
            //Control.SetGeneralProperty(NativeMethods.SCI_MARKERDELETEALL, BREAKPOINT_MARKER);
            scintilla.DirectMessage(NativeMethods.SCI_MARKERDELETEALL, new IntPtr(BREAKPOINT_MARKER), IntPtr.Zero);
            BreakpointsChanged?.Invoke(this, new BreakpointsChangedEventArgs(BreakpointChangeType.Clear));
        }
        public bool IsBreakpointsMarginVisible
        {
            get
            {
                var i = (scintilla.DirectMessage(NativeMethods.SCI_GETMARGINWIDTHN, new IntPtr(BREAKPOINTS_MARGIN), IntPtr.Zero)).ToInt32();
                return i != 0;
            }
            set
            {
                scintilla.DirectMessage(2242, new IntPtr(BREAKPOINTS_MARGIN), value ? new IntPtr(16) : IntPtr.Zero);
            }
        }

        public override Eto.Drawing.Color BackgroundColor
        {
          get
          {
            return Eto.Drawing.Colors.Transparent;
          }
          set
          {
            throw new NotImplementedException();
          }
        }
    }


}
