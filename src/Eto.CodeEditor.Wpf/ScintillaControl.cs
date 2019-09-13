using System;
using System.Windows.Forms;
using System.IO;

namespace Eto.CodeEditor.Wpf
{
    class ScintillaControl : Control
    {
        /// <summary>
        /// SciLexer.dll is embedded in this assembly. Unpack it to a temp directory and
        /// return the path that to the unpacked library
        /// </summary>
        /// <returns></returns>
        public static string UnpackNativeScintilla()
        {
            const string scilexerVersion = "4.2.0";
            string bitness = IntPtr.Size == 4 ? "x86" : "x64";
            string path = Path.Combine(Path.GetTempPath(), "Eto.CodeEditor.Wpf", scilexerVersion, bitness, "SciLexer.dll");
            if (!File.Exists(path))
            {
                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                string resourceName = $"Eto.CodeEditor.Wpf.scintilla.{bitness}.SciLexer.dll";
                using (var resourceStream = typeof(ScintillaControl).Assembly.GetManifestResourceStream(resourceName))
                using (var fileStream = File.Create(path))
                {
                    resourceStream.CopyTo(fileStream);
                }
            }
            return path;
        }
    }
}
