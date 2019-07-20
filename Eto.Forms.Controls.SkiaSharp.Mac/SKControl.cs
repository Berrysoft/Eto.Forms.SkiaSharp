using System;
using Eto.Mac.Forms;
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using SkiaSharp;
using SkiaSharp.Views.Mac;

namespace Eto.Forms.Controls.SkiaSharp.Mac
{
    public class SKControlHandler : MacView<NSView, SKControl, Control.ICallback>, SKControl.ISKControl
    {
        private SKControl_Mac nativecontrol;

        public SKControlHandler()
        {
            nativecontrol = new SKControl_Mac();
            Control = nativecontrol;
        }

        public override Eto.Drawing.Color BackgroundColor
        {
            get => Eto.Drawing.Colors.White;
            set { }
        }

        public override NSView ContainerControl => Control;

        public override bool Enabled { get; set; }
        public Action<SKSurface> PaintSurfaceAction
        {
            get => nativecontrol.PaintSurface;
            set => nativecontrol.PaintSurface = value;
        }
    }

    public class SKControl_Mac : NSView, IMacControl
    {
        public Action<SKSurface> PaintSurface;

        private NSTrackingArea trackarea;

        public float _lastTouchX;
        public float _lastTouchY;

        private SKDrawable drawable;

        public SKControl_Mac()
        {
            drawable = new SKDrawable();
            BecomeFirstResponder();
        }

        public override CGRect Bounds
        {
            get => base.Bounds;
            set
            {
                base.Bounds = value;
                UpdateTrackingAreas();
            }
        }

        public override CGRect Frame
        {
            get => base.Frame;
            set
            {
                base.Frame = value;
                UpdateTrackingAreas();
            }
        }

        public override void UpdateTrackingAreas()
        {
            if (trackarea != null) { RemoveTrackingArea(trackarea); }
            trackarea = new NSTrackingArea(Frame, NSTrackingAreaOptions.ActiveWhenFirstResponder | NSTrackingAreaOptions.MouseMoved | NSTrackingAreaOptions.InVisibleRect, this, null);
            AddTrackingArea(trackarea);
        }

        public override void DrawRect(CGRect dirtyRect)
        {
            base.DrawRect(dirtyRect);

            var ctx = NSGraphicsContext.CurrentContext.GraphicsPort;

            // create the skia context
            var surface = drawable.CreateSurface(Bounds, 1.0f, out SKImageInfo info);

            if (PaintSurface != null) PaintSurface.Invoke(surface);

            // draw the surface to the context
            drawable.DrawSurface(ctx, Bounds, info, surface);
        }

        public WeakReference WeakHandler { get; set; }
    }
}
