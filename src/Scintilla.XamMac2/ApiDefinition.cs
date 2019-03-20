using System;
using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
//using Scintilla;

namespace ScintillaNET
{

    // @protocol ScintillaNotificationProtocol
    [Protocol]
    interface ScintillaNotificationProtocol
    {
        // @required -(void)notification:(SCNotification *)notification;
        [Abstract]
        [Export("notification:")]
        void Notification(IntPtr notification);
    }

    // @interface ScintillaView : NSView <InfoBarCommunicator, ScintillaNotificationProtocol>
    [BaseType(typeof(NSView))]
    interface ScintillaView
    {
        // @property (assign, nonatomic) id<ScintillaNotificationProtocol> delegate;
        [NullAllowed, Export("delegate", ArgumentSemantic.Assign)]
        NSObject WeakDelegate { get; set; }

        // @property (readonly, nonatomic) NSScrollView * scrollView;
        [Export("scrollView")]
        NSScrollView ScrollView { get; }

        // -(NSString *)string;
        // -(void)setString:(NSString *)aString;
        [Export("string")]
        string Text { get; set; }


        // -(void)setGeneralProperty:(int)property parameter:(long)parameter value:(long)value;
        [Export("setGeneralProperty:parameter:value:")]
        void SetGeneralProperty(int property, nint parameter, nint value);

        // -(void)setGeneralProperty:(int)property value:(long)value;
        [Export("setGeneralProperty:value:")]
        void SetGeneralProperty(int property, nint value);

        // -(long)getGeneralProperty:(int)property;
        [Export("getGeneralProperty:")]
        nint GetGeneralProperty(int property);

        // -(long)getGeneralProperty:(int)property parameter:(long)parameter;
        [Export("getGeneralProperty:parameter:")]
        nint GetGeneralProperty(int property, nint parameter);

        // -(long)getGeneralProperty:(int)property parameter:(long)parameter extra:(long)extra;
        [Export("getGeneralProperty:parameter:extra:")]
        nint GetGeneralProperty(int property, nint parameter, nint extra);

        // -(void)setColorProperty:(int)property parameter:(long)parameter value:(NSColor *)value;
        [Export("setColorProperty:parameter:value:")]
        void SetColorProperty(int property, nint parameter, NSColor value);

        // -(void)setColorProperty:(int)property parameter:(long)parameter fromHTML:(NSString *)fromHTML;
        [Export("setColorProperty:parameter:fromHTML:")]
        void SetColorProperty(int property, nint parameter, string fromHTML);

        // -(NSColor *)getColorProperty:(int)property parameter:(long)parameter;
        [Export("getColorProperty:parameter:")]
        NSColor GetColorProperty(int property, nint parameter);

        // -(void)setReferenceProperty:(int)property parameter:(long)parameter value:(const void *)value;
        [Export("setReferenceProperty:parameter:value:")]
        void SetReferenceProperty(int property, nint parameter, IntPtr value);

        // -(void)setStringProperty:(int)property parameter:(long)parameter value:(NSString *)value;
        [Export("setStringProperty:parameter:value:")]
        void SetStringProperty(int property, nint parameter, string value);

        // -(NSString *)getStringProperty:(int)property parameter:(long)parameter;
        [Export("getStringProperty:parameter:")]
        string GetStringProperty(int property, nint parameter);

        // -(sptr_t)message:(unsigned int)message wParam:(uptr_t)wParam lParam:(sptr_t)lParam;
        [Export("message:wParam:lParam:")]
        IntPtr Message(uint message, IntPtr wParam, IntPtr lParam);

    }

}