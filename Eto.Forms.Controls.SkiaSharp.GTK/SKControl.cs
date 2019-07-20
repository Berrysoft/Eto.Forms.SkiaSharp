using System;
using Cairo;
using Eto.GtkSharp.Forms;
using Gdk;
using Gtk;
using SkiaSharp;

namespace Eto.Forms.Controls.SkiaSharp.GTK
{
    public class SKControlHandler : GtkControl<EventBox, SKControl, Control.ICallback>, SKControl.ISKControl
    {

        private SKControl_GTK nativecontrol;

        public SKControlHandler()
        {
            nativecontrol = new SKControl_GTK();
            Control = nativecontrol;
        }

        public override Eto.Drawing.Color BackgroundColor { get; set; }

        public Action<SKSurface> PaintSurfaceAction
        {
            get => nativecontrol.PaintSurface;
            set => nativecontrol.PaintSurface = value;
        }

    }

    public class SKControl_GTK : EventBox
    {

        public Action<SKSurface> PaintSurface;

        public SKControl_GTK()
        {
            AddEvents((int)EventMask.PointerMotionMask);
        }

        protected override bool OnDrawn(Context cr)
        {
            var res = base.OnDrawn(cr);
            if (res)
            {
                var rect = Allocation;
                if (rect.Width > 0 && rect.Height > 0)
                {
                    SKColorType ctype = SKColorType.Bgra8888;
                    using (var bitmap = new SKBitmap(rect.Width, rect.Height, ctype, SKAlphaType.Premul))
                    {
                        if (bitmap == null) { throw new InvalidOperationException("Bitmap is null"); }
                        using (var skSurface = SKSurface.Create(new SKImageInfo(bitmap.Info.Width, bitmap.Info.Height, ctype, SKAlphaType.Premul), bitmap.GetPixels(out IntPtr len), bitmap.Info.RowBytes))
                        {
                            if (skSurface == null) { throw new InvalidOperationException("skSurface is null"); }
                            if (PaintSurface != null) PaintSurface.Invoke(skSurface);
                            skSurface.Canvas.Flush();
                            using (Surface surface = new ImageSurface(bitmap.GetPixels(out len), Format.Argb32, bitmap.Width, bitmap.Height, bitmap.Width * 4))
                            {
                                surface.MarkDirty();
                                cr.SetSourceSurface(surface, 0, 0);
                                cr.Paint();
                            }
                        }
                    }
                }
            }
            return res;
        }
    }
}
