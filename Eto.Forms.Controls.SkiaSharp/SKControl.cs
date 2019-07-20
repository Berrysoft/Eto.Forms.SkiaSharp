using System;
using SkiaSharp;

namespace Eto.Forms.Controls.SkiaSharp
{
    [Handler(typeof(ISKControl))]
    public class SKControl : Control
    {
        new ISKControl Handler => (ISKControl)base.Handler;

        public Action<SKSurface> PaintSurfaceAction
        {
            get => Handler.PaintSurfaceAction;
            set => Handler.PaintSurfaceAction = value;
        }

        public interface ISKControl : IHandler
        {
            Action<SKSurface> PaintSurfaceAction { get; set; }
        }
    }
}
