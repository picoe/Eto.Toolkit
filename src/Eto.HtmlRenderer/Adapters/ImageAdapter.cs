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

using System.Collections.Generic;
using System.IO;
using Eto.Drawing;
using TheArtOfDev.HtmlRenderer.Adapters;

namespace TheArtOfDev.HtmlRenderer.Eto.Adapters
{

    /// <summary>
    /// Adapter for Eto Image object for core.
    /// TODO: Implement animation
    /// </summary>
    internal sealed class ImageAdapter : RImage, IImageAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public ImageAdapter(Image image)
        {
            Image = image;
        }

        public ImageAdapter()
        {
        }

        /// <summary>
        /// the underline Eto image.
        /// </summary>
        public Image Image { get; private set; }

        public override double Width => Image.Width;

        public override double Height => Image.Height;

        public override void Dispose() => Image.Dispose();

        public bool Load(Stream stream)
        {
            Image?.Dispose();
            Image = new Bitmap(stream);
            return true;
        }
    }
}