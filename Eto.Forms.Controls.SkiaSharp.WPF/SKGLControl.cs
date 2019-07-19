using System;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using Eto.Forms.Controls.SkiaSharp.WinForms;
using Eto.Wpf.Forms;
using SkiaSharp;

namespace Eto.Forms.Controls.SkiaSharp.WPF
{
    public class SKGLControlHandler : WpfFrameworkElement<FrameworkElement, SKGLControl, Control.ICallback>, SKGLControl.ISKGLControl
    {

        private SKGLControl_WPF nativecontrol;

        public SKGLControlHandler()
        {
            nativecontrol = new SKGLControl_WPF();

            // Create the winforms control

            nativecontrol.WinFormsControl = new SKGLControl_WinForms();

            Control = nativecontrol;
        }

        public override Eto.Drawing.Color BackgroundColor { get; set; }

        public Action<SKSurface> PaintSurfaceAction
        {
            get => nativecontrol.WinFormsControl.PaintSurface;
            set => nativecontrol.WinFormsControl.PaintSurface = value;
        }


    }

    public class SKGLControl_WPF : System.Windows.Controls.Grid
    {

        public SKGLControl_WinForms WinFormsControl;

        public SKGLControl_WPF()
        {
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            // Create the interop host control.
            WindowsFormsHost host = new WindowsFormsHost();

            WinFormsControl.WPFHost = true;

            // Assign the winforms control as the host control's child.
            host.Child = WinFormsControl;

            // Add the interop host control to the Grid
            // control's collection of child controls.
            Children.Add(host);

        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            WinFormsControl.Invalidate();
        }

    }

}
