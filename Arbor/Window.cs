using Arbor.Caching;
using Arbor.Graphics;
using Arbor.Graphics.Shaders.Uniforms;
using Arbor.Statistics;
using Arbor.Timing;
using GlmSharp;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Arbor;

public class Window : IDisposable
{
    internal static ImGuiRenderer Igr = null!;
    
    private readonly WindowCreateInfo createInfo;
    private Game runningGame = null!;

    private ThrottledFrameClock clock = null!;
    
    internal DevicePipeline Pipeline = null!;
    
    public string Title => createInfo.WindowTitle;

    #region Constructor

    public Window(string title, int width, int height)
        : this(new WindowCreateInfo(100, 100, width, height, WindowState.Normal, title))
    {
    }

    public Window(WindowCreateInfo info)
    {
        createInfo = info;
    }

    #endregion

    public unsafe void Run(Game game)
    {
        runningGame = game;
        runningGame.Window = this;

        createWindow();
        
        Igr = new ImGuiRenderer(device, device.SwapchainFramebuffer.OutputDescription, window.Width, window.Height);
        window.Resized += () => Igr.WindowResized(window.Width, window.Height);

        var mode = new SDL_DisplayMode();
        Sdl2Native.SDL_GetDesktopDisplayMode(0, &mode);

        clock = new ThrottledFrameClock();
        clock.MaximumUpdateHz = mode.refresh_rate * 2;

        runningGame.FramedClock = clock;
        runningGame.LoadInternal();

        // TODO: Run in separate threads for input, audio, update, and draw.
        while (window.Exists)
        {
            var snapshot = window.PumpEvents();
            
            clock.ProcessFrame();
            runningGame.UpdateInternal(clock);
            Igr.Update((float)clock.TimeInfo.Elapsed, snapshot);
            draw();
        }
    }

    #region Initialization

    private Sdl2Window window = null!;
    private GraphicsDevice device = null!;

    private void createWindow()
    {
        var options = new GraphicsDeviceOptions { PreferStandardClipSpaceYDirection = true };
        VeldridStartup.CreateWindowAndGraphicsDevice(createInfo, options, out window, out device);

        window.Resized += () =>
        {
            device.ResizeMainWindow((uint) window.Width, (uint) window.Height);
            invalidatePixelMatrix();
        };

        Pipeline = new DevicePipeline(device);
        invalidatePixelMatrix();
    }

    #endregion

    #region Drawing

    private mat4 pixelMatrix;
    private readonly Cached pixelMatrixBufferCache = new Cached();
    private DrawPipeline drawPipeline => Pipeline.DrawPipeline;

    private void draw()
    {
        FrameStatistics.Clear();
        FrameStatistics.COUNTERS[(int) StatisticsCounterType.DrawCalls] = 0;
        drawPipeline.Start();

        if (!pixelMatrixBufferCache.IsValid)
        {
            drawPipeline.SetGlobalUniform(GlobalProperties.ModelMatrix, mat4.Identity);
            drawPipeline.SetGlobalUniform(GlobalProperties.PixelMatrix, pixelMatrix);
            pixelMatrixBufferCache.Validate();
        }

        // TODO: Replace this with just components
        runningGame.DrawInternal(drawPipeline);
        drawPipeline.End();
        drawPipeline.Flush();
    }

    private void invalidatePixelMatrix()
    {
        pixelMatrix = mat4.Ortho(0, window.Width, window.Height, 0, -1, 1);
        pixelMatrixBufferCache.Invalidate();
    }

    #endregion

    public void Dispose()
    {
        runningGame.Dispose();
        Pipeline.Dispose();

        GC.SuppressFinalize(this);
    }
}
