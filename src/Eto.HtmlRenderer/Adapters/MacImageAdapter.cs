#if USE_MACIMAGE
using System;
using sd = System.Drawing;
using sdi = System.Drawing.Imaging;
using System.IO;
using Eto.Drawing;
using System.Collections.Generic;
using System.Linq;
using TheArtOfDev.HtmlRenderer.Adapters;
#if __UNIFIED__
using CoreGraphics;
using AppKit;
using Foundation;
#else
using MonoMac.CoreGraphics;
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
#endif

namespace TheArtOfDev.HtmlRenderer.Eto.Adapters
{
    class MacImageFrame : RImageFrame, IDisposable
    {
        TimeSpan _delay;

        public MacImageFrame(TimeSpan delay)
        {
            _delay = delay;
        }

        public Bitmap Bitmap { get; set; }

        public override TimeSpan FrameDelay => _delay;

        public void Dispose()
        {
            Bitmap?.Dispose();
            Bitmap = null;
        }
    }

    public class MacImageAdapter : RImage, IImageAdapter
    {
        List<RImageFrame> _frames;
        int _currentFrame;
        NSBitmapImageRep _rep;
        NSImage _img;

        MacImageAdapter(NSImage img, NSBitmapImageRep rep, List<RImageFrame> frames)
        {
            _img = img;
            _rep = rep;
            _frames = frames;
        }

        public Image Image => GetBitmap();

        public override double Width => _img.Size.Width;

        public override double Height => _img.Size.Height;

        static NSString NSImageFrameCount = new NSString("NSImageFrameCount");
        static NSObject NSImageCurrentFrame = new NSString("NSImageFrameCount");
        static IntPtr selValueForPropertyHandle = Selector.GetHandle("valueForProperty:");
        static IntPtr selSetPropertyWithValueHandle = Selector.GetHandle("setProperty:withValue:");

        static NSObject GetValueForProperty(NSBitmapImageRep rep, NSString property)
        {
            return Runtime.GetNSObject(Messaging.IntPtr_objc_msgSend_IntPtr(rep.Handle, selValueForPropertyHandle, property.Handle));
        }

        static void SetValueForProperty(NSBitmapImageRep rep, NSString property, NSObject value)
        {
            Messaging.void_objc_msgSend_IntPtr_IntPtr(rep.Handle, selSetPropertyWithValueHandle, property.Handle, value.Handle);
        }

        public static MacImageAdapter TryGet(Stream stream)
        {
            var img = NSImage.FromStream(stream);
            var bitmapRep = img.Representations()[0] as NSBitmapImageRep;
            if (bitmapRep == null)
                return null;

            var frames = GetValueForProperty(bitmapRep, NSImageFrameCount) as NSNumber;
            if (frames == null || frames.Int32Value <= 1)
                return null;
            var images = new List<RImageFrame>();
            for (var i = 0; i < frames.Int32Value; i++)
            {
                SetValueForProperty(bitmapRep, NSBitmapImageRep.CurrentFrame, new NSNumber(i));
                var delay = GetValueForProperty(bitmapRep, NSBitmapImageRep.CurrentFrameDuration) as NSNumber;
                images.Add(new MacImageFrame(TimeSpan.FromSeconds(delay.DoubleValue)));
            }
            return new MacImageAdapter(img, bitmapRep, images);
        }

        Bitmap GetBitmap()
        {
            var frame = (MacImageFrame)_frames[_currentFrame];
            if (frame.Bitmap != null)
                return frame.Bitmap;

            SetValueForProperty(_rep, NSBitmapImageRep.CurrentFrame, new NSNumber(_currentFrame));

            var frameData = _rep.RepresentationUsingTypeProperties(NSBitmapImageFileType.Png, null);
            NSImage frameImage = new NSImage(frameData);
            frame.Bitmap = new Bitmap(new global::Eto.Mac.Drawing.BitmapHandler(frameImage));
            return frame.Bitmap;
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
                    (frame as MacImageFrame)?.Dispose();
                }
                _frames = null;
            }
        }
    }
}
#endif
