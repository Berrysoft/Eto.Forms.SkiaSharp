using System;
using SkiaSharp;

namespace Eto.Forms.Controls.SkiaSharp
{
    [Handler(typeof(ISKGLControl))]
    public class SKGLControl : Control
    {
        new ISKGLControl Handler => (ISKGLControl)base.Handler;

        public Action<SKSurface> PaintSurfaceAction
        {
            get => Handler.PaintSurfaceAction;
            set => Handler.PaintSurfaceAction = value;
        }

        public interface ISKGLControl : IHandler
        {
            Action<SKSurface> PaintSurfaceAction { get; set; }
        }
    }
}
