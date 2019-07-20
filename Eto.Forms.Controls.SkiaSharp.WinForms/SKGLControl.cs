using System;
using Eto.WinForms.Forms;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace Eto.Forms.Controls.SkiaSharp.WinForms
{
    public class SKGLControlHandler : WindowsControl<System.Windows.Forms.Control, SKGLControl, Control.ICallback>, SKGLControl.ISKGLControl
    {
        private SKGLControl_WinForms nativecontrol;

        public SKGLControlHandler()
        {
            nativecontrol = new SKGLControl_WinForms();
            Control = nativecontrol;
        }
        public Action<SKSurface> PaintSurfaceAction
        {
            get => nativecontrol.PaintSurface;
            set => nativecontrol.PaintSurface = value;
        }
    }

    public class SKGLControl_WinForms : global::SkiaSharp.Views.Desktop.SKGLControl
    {

        public new Action<SKSurface> PaintSurface;

        public bool WPFHost = false;

        public Action<MouseEventArgs> WPFMouseDown;
        public Action<MouseEventArgs> WPFMouseUp;
        public Action<MouseEventArgs> WPFMouseDoubleClick;

        private GRContext grContext;
        private GRBackendRenderTarget renderTarget;

        public SKGLControl_WinForms()
        {
            ResizeRedraw = true;
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            // create the contexts if not done already
            if (grContext == null)
            {
                var glInterface = GRGlInterface.CreateNativeGlInterface();
                grContext = GRContext.Create(GRBackend.OpenGL, glInterface);

                // get initial details
                renderTarget = CreateRenderTarget();
            }

            // update to the latest dimensions
            renderTarget = new GRBackendRenderTarget(Width, Height, renderTarget.SampleCount, renderTarget.StencilBits, renderTarget.GetGlFramebufferInfo());

            // create the surface
            using (var surface = SKSurface.Create(grContext, renderTarget, SKColorType.Rgba8888))
            {

                if (PaintSurface != null) PaintSurface.Invoke(surface);

                // start drawing
                OnPaintSurface(new SKPaintGLSurfaceEventArgs(surface, renderTarget));

                surface.Canvas.Flush();
            }

            // update the control
            SwapBuffers();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            // clean up
            if (grContext != null)
            {
                grContext.Dispose();
                grContext = null;
            }
        }

        public static GRBackendRenderTarget CreateRenderTarget()
        {
            Gles.glGetIntegerv(Gles.GL_FRAMEBUFFER_BINDING, out int framebuffer);
            Gles.glGetIntegerv(Gles.GL_STENCIL_BITS, out int stencil);
            Gles.glGetIntegerv(Gles.GL_SAMPLES, out int samples);

            int bufferWidth = 0;
            int bufferHeight = 0;

            return new GRBackendRenderTarget(bufferWidth, bufferHeight, samples, stencil, new GRGlFramebufferInfo((uint)framebuffer, GRPixelConfig.Rgba8888.ToGlSizedFormat()));
        }
    }
}
