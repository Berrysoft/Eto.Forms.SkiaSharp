﻿using System;
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

        protected override bool OnDamageEvent(EventExpose evnt)
        {

            var rect = Allocation;

            if (rect.Width > 0 && rect.Height > 0)
            {
                var area = evnt.Area;
                SKColorType ctype = SKColorType.Bgra8888;
                using (Cairo.Context cr = Gdk.CairoHelper.Create(GdkWindow))
                {
                    if (cr == null) { Console.WriteLine("Cairo Context is null"); }
                    using (var bitmap = new SKBitmap(rect.Width, rect.Height, ctype, SKAlphaType.Premul))
                    {
                        if (bitmap == null) { Console.WriteLine("Bitmap is null"); }
                        IntPtr len;
                        using (var skSurface = SKSurface.Create(bitmap.Info.Width, bitmap.Info.Height, ctype, SKAlphaType.Premul, bitmap.GetPixels(out len), bitmap.Info.RowBytes))
                        {
                            if (skSurface == null) { Console.WriteLine("skSurface is null"); }
                            if (PaintSurface != null) PaintSurface.Invoke(skSurface);
                            skSurface.Canvas.Flush();
                            using (Cairo.Surface surface = new Cairo.ImageSurface(bitmap.GetPixels(out len), Cairo.Format.Argb32, bitmap.Width, bitmap.Height, bitmap.Width * 4))
                            {
                                surface.MarkDirty();
                                cr.SetSourceSurface(surface, 0, 0);
                                cr.Paint();
                            }
                        }
                    }
                }

            }

            return true;
        }

    }



}
