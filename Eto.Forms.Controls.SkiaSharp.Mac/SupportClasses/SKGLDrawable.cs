using SkiaSharp.Views.GlesInterop;

namespace SkiaSharp.Views.Mac
{
    internal static class SKGLDrawable
    {
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