using System;
using Eto.Mac.Forms;
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using SkiaSharp;
using SkiaSharp.Views.GlesInterop;
using Views.Mac;

namespace Eto.Forms.Controls.SkiaSharp.Mac
{
    public class SKGLControlHandler : MacView<NSView, SKGLControl, Control.ICallback>, SKGLControl.ISKGLControl
    {
        private SKGLControl_Mac nativecontrol;

        public SKGLControlHandler()
        {
            nativecontrol = new SKGLControl_Mac();
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

    public class SKGLControl_Mac : SKGLView, IMacControl
    {
        public new Action<SKSurface> PaintSurface;

        private NSTrackingArea trackarea;

        public SKGLControl_Mac() : base()
        {
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

            var size = ConvertSizeToBacking(Bounds.Size);
            renderTarget = new GRBackendRenderTarget((int)size.Width, (int)size.Height, renderTarget.SampleCount, renderTarget.StencilBits, renderTarget.GetGlFramebufferInfo());

            Gles.glClear(Gles.GL_STENCIL_BUFFER_BIT);

            using (var surface = SKSurface.Create(context, renderTarget, SKColorType.Rgba8888))
            {
                if (PaintSurface != null) PaintSurface.Invoke(surface);

                surface.Canvas.Flush();
            }

            // flush the SkiaSharp contents to GL
            context.Flush();

            OpenGLContext.FlushBuffer();
        }

        public WeakReference WeakHandler { get; set; }
    }
}
