using System;
using Eto.GtkSharp.Forms;
using Gdk;
using OpenTK;
using SkiaSharp;

namespace Eto.Forms.Controls.SkiaSharp.GTK
{
    public class SKGLControlHandler : GtkControl<GLWidget, SKGLControl, Control.ICallback>, SKGLControl.ISKGLControl
    {
        private SKGLControl_GTK nativecontrol;

        public SKGLControlHandler()
        {
            nativecontrol = new SKGLControl_GTK();
            Control = nativecontrol;
        }

        public override Eto.Drawing.Color BackgroundColor { get; set; }
        public Action<SKSurface> PaintSurfaceAction
        {
            get => nativecontrol.PaintSurface;
            set => nativecontrol.PaintSurface = value;
        }
    }

    public class SKGLControl_GTK : GLWidget
    {
        public Action<SKSurface> PaintSurface;

        private GRContext grContext;
        private GRBackendRenderTarget renderTarget;

        public SKGLControl_GTK() : base()
        {
            AddEvents((int)EventMask.PointerMotionMask);
        }

        protected override void OnRenderFrame()
        {
            var rect = Allocation;

            // create the contexts if not done already
            if (grContext == null)
            {
                var glInterface = GRGlInterface.CreateNativeGlInterface();

                if (glInterface == null)
                {
                    throw new InvalidOperationException("Error creating OpenGL ES interface. Check if you have OpenGL ES correctly installed and configured or change the PFD Renderer to 'Software (CPU)' on the Global Settings panel.");
                }
                else
                {
                    grContext = GRContext.Create(GRBackend.OpenGL, glInterface);
                }

                try
                {
                    renderTarget = CreateRenderTarget();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Error creating OpenGL ES render target. Check if you have OpenGL ES correctly installed and configured or change the PFD Renderer to 'Software (CPU)' on the Global Settings panel.", ex);
                }
            }

            if (grContext != null)
            {
                // update to the latest dimensions
                renderTarget = new GRBackendRenderTarget(rect.Width, rect.Height, renderTarget.SampleCount, renderTarget.StencilBits, renderTarget.GetGlFramebufferInfo());

                // create the surface
                using (var surface = SKSurface.Create(grContext, renderTarget, SKColorType.Rgba8888))
                {

                    if (PaintSurface != null) PaintSurface.Invoke(surface);

                    surface.Canvas.Flush();
                }
            }

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
