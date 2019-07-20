using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Gdk;
using Gtk;
using OpenTK.Graphics;
using OpenTK.Platform;

namespace OpenTK
{
    /// <summary>
    /// The <see cref="GLWidget"/> is a GTK widget for which an OpenGL context can be used to draw arbitrary graphics.
    /// </summary>
    public class GLWidget : DrawingArea
    {
        [DllImport("libX11", EntryPoint = "XGetVisualInfo")]
        static extern IntPtr XGetVisualInfoInternal(IntPtr display, IntPtr vinfo_mask, ref XVisualInfo template, out int nitems);

        private static int _GraphicsContextCount;
        private static bool _SharedContextInitialized = false;

        private IGraphicsContext _GraphicsContext;
        private IWindowInfo _WindowInfo;
        private bool _Initialized = false;

        /// <summary>
        /// Use a single buffer versus a double buffer.
        /// </summary>
        [Browsable(true)]
        public bool SingleBuffer { get; set; }

        /// <summary>
        /// Color Buffer Bits-Per-Pixel
        /// </summary>
        public int ColorBPP { get; set; }

        /// <summary>
        /// Accumulation Buffer Bits-Per-Pixel
        /// </summary>
        public int AccumulatorBPP { get; set; }

        /// <summary>
        /// Depth Buffer Bits-Per-Pixel
        /// </summary>
        public int DepthBPP { get; set; }

        /// <summary>
        /// Stencil Buffer Bits-Per-Pixel
        /// </summary>
        public int StencilBPP { get; set; }

        /// <summary>
        /// Number of samples
        /// </summary>
        public int Samples { get; set; }

        /// <summary>
        /// Indicates if steropic renderering is enabled
        /// </summary>
        public bool Stereo { get; set; }

        /// <summary>
        /// The major version of OpenGL to use.
        /// </summary>
        public int GlVersionMajor { get; set; }

        /// <summary>
        /// The minor version of OpenGL to use.
        /// </summary>
        public int GlVersionMinor { get; set; }

        /// <summary>
        /// The set <see cref="GraphicsContextFlags"/> for this widget.
        /// </summary>
        public GraphicsContextFlags GraphicsContextFlags { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GLWidget"/> class.
        /// </summary>
        public GLWidget()
            : this(GraphicsMode.Default)
        {
        }

        /// <summary>Constructs a new GLWidget using a given GraphicsMode</summary>
        public GLWidget(GraphicsMode graphicsMode)
            : this(graphicsMode, 1, 0, GraphicsContextFlags.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GLWidget"/> class.
        /// </summary>
        /// <param name="graphicsMode">The <see cref="GraphicsMode"/> which the widget should be constructed with.</param>
        /// <param name="glVersionMajor">The major OpenGL version to attempt to initialize.</param>
        /// <param name="glVersionMinor">The minor OpenGL version to attempt to initialize.</param>
        /// <param name="graphicsContextFlags">
        /// Any flags which should be used during initialization of the <see cref="GraphicsContext"/>.
        /// </param>
        public GLWidget(GraphicsMode graphicsMode, int glVersionMajor, int glVersionMinor, GraphicsContextFlags graphicsContextFlags)
        {
            this.DoubleBuffered = false;

            SingleBuffer = graphicsMode.Buffers == 1;
            ColorBPP = graphicsMode.ColorFormat.BitsPerPixel;
            AccumulatorBPP = graphicsMode.AccumulatorFormat.BitsPerPixel;
            DepthBPP = graphicsMode.Depth;
            StencilBPP = graphicsMode.Stencil;
            Samples = graphicsMode.Samples;
            Stereo = graphicsMode.Stereo;

            GlVersionMajor = glVersionMajor;
            GlVersionMinor = glVersionMinor;
            GraphicsContextFlags = graphicsContextFlags;
        }

        /// <summary>
        /// Destroys this <see cref="Widget"/>, disposing it and destroying it in the context of GTK.
        /// </summary>
        public override void Destroy()
        {
            GC.SuppressFinalize(this);
            Dispose(true);

            base.Destroy();
        }

        /// <summary>
        /// Disposes the current object, releasing any native resources it was using.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _GraphicsContext.MakeCurrent(_WindowInfo);
                OnShuttingDown();
                if (GraphicsContext.ShareContexts && (Interlocked.Decrement(ref _GraphicsContextCount) == 0))
                {
                    OnGraphicsContextShuttingDown();
                    _SharedContextInitialized = false;
                }
                _GraphicsContext.Dispose();
            }
        }

        /// <summary>
        /// Called when the first <see cref="GraphicsContext"/> is created in the case where
        /// GraphicsContext.ShareContexts == true;
        /// </summary>
        public static event EventHandler GraphicsContextInitialized;

        /// <summary>
        /// Invokes the <see cref="GraphicsContextInitialized"/> event.
        /// </summary>
        private static void OnGraphicsContextInitialized()
        {
            GraphicsContextInitialized?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the first <see cref="GraphicsContext"/> is being destroyed in the case where
        /// GraphicsContext.ShareContext == true;
        /// </summary>
        public static event EventHandler GraphicsContextShuttingDown;

        /// <summary>
        /// Invokes the <see cref="GraphicsContextShuttingDown"/> event.
        /// </summary>
        private static void OnGraphicsContextShuttingDown()
        {
            GraphicsContextShuttingDown?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Called when this <see cref="GLWidget"/> has finished initializing and has a valid
        /// <see cref="GraphicsContext"/>.
        /// </summary>
        public event EventHandler Initialized;

        /// <summary>
        /// Invokes the <see cref="Initialized"/> event.
        /// </summary>
        protected virtual void OnInitialized()
        {
            Initialized?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Invokes the <see cref="RenderFrame"/> event.
        /// </summary>
        protected virtual void OnRenderFrame() { }

        /// <summary>
        /// Called when this <see cref="GLWidget"/> is being disposed.
        /// </summary>
        public event EventHandler ShuttingDown;

        /// <summary>
        /// Invokes the <see cref="ShuttingDown"/> event.
        /// </summary>
        protected virtual void OnShuttingDown()
        {
            ShuttingDown?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the widget needs to be (fully or partially) redrawn.
        /// </summary>
        /// <param name="cr"></param>
        /// <returns></returns>
        protected override bool OnDrawn(Cairo.Context cr)
        {
            if (!_Initialized)
            {
                Initialize();
            }
            else
            {
                _GraphicsContext.MakeCurrent(_WindowInfo);
            }

            var result = base.OnDrawn(cr);

            OnRenderFrame();

            _GraphicsContext.SwapBuffers();

            return result;
        }

        /// <summary>
        /// Called whenever the widget is resized.
        /// </summary>
        /// <param name="evnt"></param>
        /// <returns></returns>
        protected override bool OnConfigureEvent(EventConfigure evnt)
        {
            bool result = base.OnConfigureEvent(evnt);

            _GraphicsContext?.Update(_WindowInfo);

            return result;
        }

        /// <summary>
        /// Initializes the <see cref="GLWidget"/> with its given values and creates a <see cref="GraphicsContext"/>.
        /// </summary>
        private void Initialize()
        {
            _Initialized = true;

            // If this looks uninitialized...  initialize.
            if (ColorBPP == 0)
            {
                ColorBPP = 32;

                if (DepthBPP == 0)
                {
                    DepthBPP = 16;
                }
            }

            GraphicsMode graphicsMode = new GraphicsMode();

            if (Configuration.RunningOnWindows)
            {
                Debug.WriteLine("OpenTK running on windows");
            }
            else if (Configuration.RunningOnMacOS)
            {
                Debug.WriteLine("OpenTK running on OSX");
            }
            else
            {
                Debug.WriteLine("OpenTK running on X11");
            }

            IntPtr widgetWindowHandle = Window.Handle;

            // IWindowInfo
            if (Configuration.RunningOnWindows)
            {
                _WindowInfo = Utilities.CreateWindowsWindowInfo(widgetWindowHandle);
            }
            else if (Configuration.RunningOnMacOS)
            {
                _WindowInfo = Utilities.CreateMacOSWindowInfo(widgetWindowHandle);
            }
            else
            {
                _WindowInfo = X11.XWindowInfoInitializer.Initialize(graphicsMode, this.Display.Handle, this.Screen.Display.NMonitors, widgetWindowHandle, this.Screen.RootWindow.Handle);
            }

            // GraphicsContext
            _GraphicsContext = new GraphicsContext(graphicsMode, _WindowInfo, GlVersionMajor, GlVersionMinor, GraphicsContextFlags);
            _GraphicsContext.MakeCurrent(_WindowInfo);

            if (GraphicsContext.ShareContexts)
            {
                Interlocked.Increment(ref _GraphicsContextCount);

                if (!_SharedContextInitialized)
                {
                    _SharedContextInitialized = true;
                    ((IGraphicsContextInternal)_GraphicsContext).LoadAll();
                    OnGraphicsContextInitialized();
                }
            }
            else
            {
                ((IGraphicsContextInternal)_GraphicsContext).LoadAll();
                OnGraphicsContextInitialized();
            }

            OnInitialized();
        }
    }
}