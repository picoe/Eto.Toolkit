using System;
using sd = System.Drawing;
using System.IO;
using Eto.Drawing;
using System.Collections.Generic;
using System.Linq;
using TheArtOfDev.HtmlRenderer.Adapters;
using System.Runtime.InteropServices;
using TheArtOfDev.HtmlRenderer.Eto.Adapters;
#if __UNIFIED__
using CoreGraphics;
using AppKit;
using Foundation;
using ObjCRuntime;
#else
using MonoMac.CoreGraphics;
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
#endif

[assembly: Eto.ExportHandler(typeof(IImageAdapter), typeof(MacImageAdapter))]

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

#if __UNIFIED__
    static class Messaging
    {
        [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
        public static extern IntPtr IntPtr_objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);

        [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
        public static extern void void_objc_msgSend_IntPtr_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);
    }
#endif

    public class MacImageAdapter : RImage, IImageAdapter
    {
        List<RImageFrame> _frames;
        int _currentFrame;
        NSBitmapImageRep _rep;
        NSImage _img;

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

        public bool Load(Stream stream)
        {
            var img = NSImage.FromStream(stream);
            var bitmapRep = img.Representations()[0] as NSBitmapImageRep;
            if (bitmapRep == null)
                return false;

            var frames = GetValueForProperty(bitmapRep, NSImageFrameCount) as NSNumber;
            if (frames == null || frames.Int32Value <= 1)
                return false;
            var images = new List<RImageFrame>();
            for (var i = 0; i < frames.Int32Value; i++)
            {
                SetValueForProperty(bitmapRep, NSBitmapImageRep.CurrentFrame, new NSNumber(i));
                var delay = GetValueForProperty(bitmapRep, NSBitmapImageRep.CurrentFrameDuration) as NSNumber;
                images.Add(new MacImageFrame(TimeSpan.FromSeconds(delay.DoubleValue)));
            }
            _img = img;
            _rep = bitmapRep;
            _frames = images;
            return true;
        }
    }
}
