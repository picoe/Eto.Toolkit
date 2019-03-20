#if USE_SDIMAGE
using System;
using sd = System.Drawing;
using sdi = System.Drawing.Imaging;
using System.IO;
using Eto.Drawing;
using System.Collections.Generic;
using System.Linq;
using TheArtOfDev.HtmlRenderer.Adapters;

namespace TheArtOfDev.HtmlRenderer.Eto.Adapters
{
    class SystemDrawingImageFrame : RImageFrame, IDisposable
    {
        Bitmap _bitmap;
        TimeSpan _delay;
        sd.Image _sdimage;

        public SystemDrawingImageFrame(TimeSpan delay, sd.Image image)
        {
            _delay = delay;
            _sdimage = image;
        }

        public Bitmap Bitmap
        {
            get {
                if (_bitmap != null)
                    return _bitmap;

                using (var stream = new MemoryStream())
                {
                    _sdimage.Save(stream, sdi.ImageFormat.Png);
                    stream.Position = 0;
                    _bitmap = new Bitmap(stream);
                };

                return _bitmap;
            }
        }

        public override TimeSpan FrameDelay => _delay;

        public void Dispose()
        {
            _sdimage?.Dispose();
            _bitmap?.Dispose();
            _sdimage = null;
            _bitmap = null;
        }
    }

    public class SystemDrawingImageAdapter : RImage, IImageAdapter
    {
        List<RImageFrame> _frames;
        int _currentFrame;
        Size _size;

        SystemDrawingImageAdapter(Size size, IEnumerable<RImageFrame> frames)
        {
            _size = size;
            _frames = frames.ToList();
        }

        public Image Image => ((SystemDrawingImageFrame)_frames[_currentFrame]).Bitmap;

        public override double Width => _size.Width;

        public override double Height => _size.Height;

        public static SystemDrawingImageAdapter TryGet(Stream stream)
        {
            using (var img = sd.Image.FromStream(stream))
            {
                if (img.FrameDimensionsList.Length == 0)
                    return null;
                var dimension = new sdi.FrameDimension(img.FrameDimensionsList[0]);
                int frames = img.GetFrameCount(dimension);
                if (frames <= 1)
                    return null;

                byte[] delayBytes = img.PropertyItems.FirstOrDefault(r => r.Id == 0x5100)?.Value;
                if (delayBytes == null || delayBytes.Length < 4)
                    return null;

                // In mono we have to get the delay for each frame, unlike windows which returns the entire block
                var useFrameDelay = delayBytes.Length != frames * 4;

                //var loop = BitConverter.ToInt16(img.GetPropertyItem(0x5101).Value, 0) != 1;
                var transparentIndex = img.PropertyItems.FirstOrDefault(r => r.Id == 0x5104)?.Value[0];


                var images = new List<SystemDrawingImageFrame>();
                for (int frame = 0; frame < frames; frame++)
                {
                    img.SelectActiveFrame(dimension, frame);

                    int delayIndex;
                    if (useFrameDelay)
                    {
                        delayBytes = img.GetPropertyItem(0x5100).Value;
                        if (delayBytes.Length < 4)
                            return null;
                        delayIndex = 0;
                    }
                    else
                    {
                        delayIndex = frame * 4;
                    }

                    int delayInMilliseconds = BitConverter.ToInt32(delayBytes, delayIndex) * 10;

                    var frameImage = new sd.Bitmap(img);

                    if (global::Eto.EtoEnvironment.Platform.IsMono)
                    {
                        // bug in mono, windows preserves the transparency
                        if (transparentIndex != null)
                            frameImage.MakeTransparent(frameImage.Palette.Entries[transparentIndex.Value]);
                        else
                            frameImage.MakeTransparent();
                    }
                    images.Add(new SystemDrawingImageFrame(TimeSpan.FromMilliseconds(delayInMilliseconds), frameImage));
                }
                return new SystemDrawingImageAdapter(new Size(img.Width, img.Height), images);
            }
        }

        public override IList<RImageFrame> Frames => _frames;

        public override void SetActiveFrame(int frame)
        {
            _currentFrame = frame;
        }

        public override void Dispose()
        {
            if (_frames != null)
            {
                foreach (var frame in _frames)
                {
                    (frame as SystemDrawingImageFrame)?.Dispose();
                }
                _frames = null;
            }
        }
    }
}
#endif
