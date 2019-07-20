using System;

namespace SkiaSharp.Views.Mac
{
    public class SKPaintGLSurfaceEventArgs : EventArgs
    {
        public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget)
        {
            Surface = surface;
            RenderTarget = renderTarget;
        }

        public SKSurface Surface { get; }

        public GRBackendRenderTarget RenderTarget { get; }
    }
}