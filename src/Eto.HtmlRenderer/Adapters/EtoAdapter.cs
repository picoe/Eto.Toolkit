// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using Eto.Drawing;
using System.IO;
using Eto.Forms;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Eto.Utilities;
using System;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Eto.Adapters
{
    /// <summary>
    /// Adapter for Eto platforms.
    /// </summary>
    internal sealed class EtoAdapter : RAdapter
    {
        #region Fields and Consts

        /// <summary>
        /// Singleton instance of global adapter.
        /// </summary>
        private static readonly EtoAdapter _instance = new EtoAdapter();

        #endregion


        /// <summary>
        /// Init installed font families and set default font families mapping.
        /// </summary>
        private EtoAdapter()
        {
            // try/catch for platforms like Gtk where some fonts can't be found

            try { AddFontFamilyMapping("monospace", Fonts.Monospace(10).FamilyName); } catch { }
            try { AddFontFamilyMapping("serif", Fonts.Serif(10).FamilyName); } catch { }
            try { AddFontFamilyMapping("sans-serif", Fonts.Sans(10).FamilyName); } catch { }
            try { AddFontFamilyMapping("sans", Fonts.Sans(10).FamilyName); } catch { }
            try { AddFontFamilyMapping("cursive", Fonts.Cursive(10).FamilyName); } catch { }
            try { AddFontFamilyMapping("fantasy", Fonts.Fantasy(10).FamilyName); } catch { }

            var defaultFont = SystemFonts.Default()?.FamilyName;
            if (defaultFont != null)
            {
                // these fonts are hard-coded in HtmlRenderer.. yuck.
                AddFontFamilyMapping("Tahoma", defaultFont);
                AddFontFamilyMapping("Segoe UI", defaultFont);
            }

            foreach (var family in Fonts.AvailableFontFamilies)
            {
                AddFontFamily(new FontFamilyAdapter(family));
            }
        }

        /// <summary>
        /// Singleton instance of global adapter.
        /// </summary>
        public static EtoAdapter Instance
        {
            get { return _instance; }
        }

        protected override RColor GetColorInt(string colorName)
        {
            if (Color.TryParse(colorName, out var color))
                return Utils.Convert(color);
            return RColor.Empty;
        }

        protected override RPen CreatePen(RColor color)
        {
            return new PenAdapter(new Pen(Utils.Convert(color)));
        }

        protected override RBrush CreateSolidBrush(RColor color)
        {
            Brush solidBrush;
            if (color == RColor.White)
                solidBrush = Brushes.White;
            else if (color == RColor.Black)
                solidBrush = Brushes.Black;
            else if (color.A < 1)
                solidBrush = Brushes.Transparent;
            else
                solidBrush = new SolidBrush(Utils.Convert(color));

            return new BrushAdapter(solidBrush, false);
        }

        protected override RBrush CreateLinearGradientBrush(RRect rect, RColor color1, RColor color2, double angle)
        {
            return new BrushAdapter(new LinearGradientBrush(Utils.Convert(rect), Utils.Convert(color1), Utils.Convert(color2), (float)angle), true);
        }

        protected override RImage ConvertImageInt(object image)
        {
            if (image is Stream stream)
            {
                MemoryStream ms = null;
                try
                {
                    long originalPosition;
                    if (!stream.CanSeek)
                    {
                        ms = new MemoryStream();
                        stream.CopyTo(ms);
                        originalPosition = 0;
                        stream = ms;
                    }
                    else
                    {
                        originalPosition = stream.Position;
                    }

                    var img = SixLabors.ImageSharp.Image.Load(stream, out var format);
                    if (img.Frames.Count > 1 && img.Frames.Any(r => r.MetaData.FrameDelay > 0))
                    {
                        return new ImageSharpImageAdapter(img);
                    }
                    img.Dispose();

                    stream.Position = originalPosition;
                    return new ImageAdapter(new Bitmap(stream));
                }
                finally
                {
                    ms?.Dispose();
                }
            }
            if (image is Image bitmap)
                return new ImageAdapter(bitmap);
            return null;
        }

        protected override RImage ImageFromStreamInt(Stream memoryStream)
        {
            return new ImageAdapter(new Bitmap(memoryStream));
        }

        protected override RFont CreateFontInt(string family, double size, RFontStyle style)
        {
            var fontStyle = (FontStyle)((int)style);
            return new FontAdapter(new Font(family, (float)size, fontStyle));
        }

        protected override RFont CreateFontInt(RFontFamily family, double size, RFontStyle style)
        {
            var fontStyle = (FontStyle)((int)style);
            return new FontAdapter(new Font(((FontFamilyAdapter)family).FontFamily, (float)size, fontStyle));
        }

        protected override object GetClipboardDataObjectInt(string html, string plainText)
        {
            return ClipboardHelper.CreateDataObject(html, plainText);
        }

        protected override void SetToClipboardInt(string text)
        {
            ClipboardHelper.CopyToClipboard(text);
        }

        protected override void SetToClipboardInt(string html, string plainText)
        {
            ClipboardHelper.CopyToClipboard(html, plainText);
        }

        protected override void SetToClipboardInt(RImage image)
        {
            new Clipboard().Image = ((IImageAdapter)image).Image;
        }

        protected override RContextMenu CreateContextMenuInt()
        {
            return new ContextMenuAdapter();
        }

        protected override RTimer CreateTimerInt() => new TimerAdapter();

        protected override void SaveToFileInt(RImage image, string name, string extension, RControl control = null)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filters.Add(new FileFilter("Images", ".png", ".bmp", ".jpg", ".jpeg", ".tiff"));
                saveDialog.FileName = name;
                //saveDialog.filter.DefaultExt = extension;

                var dialogResult = control == null ? saveDialog.ShowDialog(null) : saveDialog.ShowDialog(((ControlAdapter)control).Control);
                if (dialogResult == DialogResult.Ok)
                {
                    var ext = Path.GetExtension(saveDialog.FileName);
                    ImageFormat format = ImageFormat.Png;
                    switch (ext.ToLowerInvariant())
                    {
                        case ".png":
                            format = ImageFormat.Png;
                            break;
                        case ".bmp":
                            format = ImageFormat.Bitmap;
                            break;
                        case ".tiff":
                            format = ImageFormat.Tiff;
                            break;
                        case ".jpeg":
                        case ".jpg":
                            format = ImageFormat.Jpeg;
                            break;
                    }
                    var realImage = ((IImageAdapter)image).Image;
                    var bitmap = realImage as Bitmap;
                    if (bitmap == null && realImage is Icon icon)
                    {
                        bitmap = icon.GetFrame(1).Bitmap;
                    }
                    bitmap?.Save(saveDialog.FileName, format);
                }
            }
        }
    }
}