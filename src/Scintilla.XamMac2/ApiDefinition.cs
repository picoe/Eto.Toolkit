using System;
using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
//using Scintilla;


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
}
