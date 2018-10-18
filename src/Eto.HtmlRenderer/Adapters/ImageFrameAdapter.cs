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

using TheArtOfDev.HtmlRenderer.Adapters;
using System;

namespace TheArtOfDev.HtmlRenderer.Eto.Adapters
{
	class ImageFrameAdapter : RImageFrame
	{
        TimeSpan _frameDelay;

        public ImageFrameAdapter(TimeSpan frameDelay)
        {
            _frameDelay = frameDelay;
        }

        public override TimeSpan FrameDelay => _frameDelay;
	}
}
