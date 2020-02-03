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

[assembly: ExportHandler(typeof(CodeEditor), typeof(CodeEditorHandler))]

namespace Eto.CodeEditor
{
    public partial class CodeEditorHandler : Eto.Wpf.Forms.WindowsFormsHostHandler<Scintilla.ScintillaControl, CodeEditor, CodeEditor.ICallback>, CodeEditor.IHandler
    {

        private Scintilla.ScintillaControl scintilla;

        public CodeEditorHandler()
        {
            //string path = ScintillaControl.UnpackNativeScintilla();
            //ScintillaNET.Scintilla.SetModulePath(path);
            scintilla = new Scintilla.ScintillaControl(); // new ScintillaNET.Scintilla();
            WinFormsControl = scintilla;

            /*scintilla*/this.CharAdded += WinFormsControl_CharAdded;
            scintilla.TextChanged += WinFormsControl_TextChanged;
            //InsertCheck += WinFormsControl_InsertCheck;
            //scintilla.AutoCMaxHeight = 10;
            scintilla.DirectMessage(NativeMethods.SCI_AUTOCSETMAXHEIGHT, new IntPtr(10));
            //scintilla.AutomaticFold = AutomaticFold.Click;
            scintilla.DirectMessage(NativeMethods.SCI_SETAUTOMATICFOLD, new IntPtr(NativeMethods.SC_AUTOMATICFOLD_CLICK));
            
            FontName = "Consolas";
            FontSize = 11;
            LineNumberColumnWidth = 40;
        }

        //private void WinFormsControl_InsertCheck(object sender, /*ScintillaNET.*/InsertCheckEventArgs e)
        //{
        //    InsertCheck?.Invoke(this, new Eto.CodeEditor.InsertCheckEventArgs(e.Text));
        //}

        //public void ChangeInsertion(string text)
        //{
        //    // this method shouldn't be part of the handler interface as it's not needed on windows.
        //    // on windows the handler set `e.Text = "some text"` and the setter calls the native ChangeInsertion
        //    throw new NotImplementedException("InsertCheck handler needs to be reworked.");
        //}

        private void WinFormsControl_CharAdded(object sender, /*ScintillaNET.*/CharAddedEventArgs e)
        {
            CharAdded?.Invoke(this, new Eto.CodeEditor.CharAddedEventArgs((char)e.Char));
        }

        private void WinFormsControl_TextChanged(object sender, EventArgs e)
        {
            TextChanged?.Invoke(this, new Eto.CodeEditor.TextChangedEventArgs());

        }

        //public event EventHandler<Eto.CodeEditor.CharAddedEventArgs> CharAdded;
        ////public event EventHandler<Eto.CodeEditor.TextChangedEventArgs> TextChanged;
        //public event EventHandler<EventArgs> TextChanged;
        //public event EventHandler<Eto.CodeEditor.InsertCheckEventArgs> InsertCheck;

        Encoding Encoding
        {
            get
            {
                int codePage = scintilla.DirectMessage(NativeMethods.SCI_GETCODEPAGE, IntPtr.Zero, IntPtr.Zero).ToInt32();
                return (codePage == 0) ? Encoding.Default : Encoding.GetEncoding(codePage);
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
