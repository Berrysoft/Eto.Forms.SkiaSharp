using System;
using Eto.Forms.Controls.SkiaSharp.Shared;
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
        private GRBackendRenderTargetDesc renderTarget;

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
                    Console.WriteLine("Error creating OpenGL ES interface. Check if you have OpenGL ES correctly installed and configured or change the PFD Renderer to 'Software (CPU)' on the Global Settings panel.", "Error Creating OpenGL ES interface");
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
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
                    Console.WriteLine("Error creating OpenGL ES render target. Check if you have OpenGL ES correctly installed and configured or change the PFD Renderer to 'Software (CPU)' on the Global Settings panel.\nError message:\n" + ex.ToString());
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }

            }

            if (grContext != null)
            {
                // update to the latest dimensions
                renderTarget.Width = rect.Width;
                renderTarget.Height = rect.Height;

                // create the surface
                using (var surface = SKSurface.Create(grContext, renderTarget))
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

        public static GRBackendRenderTargetDesc CreateRenderTarget()
        {

            int framebuffer, stencil, samples;
            Gles.glGetIntegerv(Gles.GL_FRAMEBUFFER_BINDING, out framebuffer);
            Gles.glGetIntegerv(Gles.GL_STENCIL_BITS, out stencil);
            Gles.glGetIntegerv(Gles.GL_SAMPLES, out samples);

            int bufferWidth = 0;
            int bufferHeight = 0;

            return new GRBackendRenderTargetDesc
            {
                Width = bufferWidth,
                Height = bufferHeight,
                Config = GRPixelConfig.Rgba8888,
                Origin = GRSurfaceOrigin.BottomLeft,
                SampleCount = samples,
                StencilBits = stencil,
                RenderTargetHandle = (IntPtr)framebuffer,
            };
        }

    }

}
