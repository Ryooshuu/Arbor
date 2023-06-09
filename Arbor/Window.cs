using System.Diagnostics;
using System.Numerics;
using Arbor.Caching;
using Arbor.Graphics;
using Arbor.Graphics.Shaders;
using Arbor.Graphics.Shaders.Basic;
using Arbor.Graphics.Shaders.Uniforms;
using Arbor.Graphics.Shaders.Vertices;
using GlmSharp;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Arbor;

public class Window : IDisposable
{
    private readonly WindowCreateInfo createInfo;
    private GraphicsPipeline pipeline = null!;

    private Stopwatch stopwatch = null!;
    private double previouslyElapsed;

    private const int target_fps = 60;
    private const float fps_time = 1000f / target_fps;

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

    public void Run()
    {
        createWindow();
        pipeline.Initialize();

        stopwatch = Stopwatch.StartNew();
        previouslyElapsed = stopwatch.ElapsedMilliseconds;

        while (window.Exists)
        {
            var snapshot = window.PumpEvents();

            var newElapsed = stopwatch.ElapsedMilliseconds;
            var deltaMs = newElapsed - previouslyElapsed;

            if (deltaMs < fps_time)
                continue;

            previouslyElapsed = newElapsed;
            draw(deltaMs);
        }
    }

    #region Initialization

    private Sdl2Window window = null!;
    private GraphicsDevice device = null!;

    private VertexBuffer<VertexPositionColour> buffer = null!;
    private ShaderSet shader = null!;

    private void createWindow()
    {
        var options = new GraphicsDeviceOptions { PreferStandardClipSpaceYDirection = true };
        VeldridStartup.CreateWindowAndGraphicsDevice(createInfo, options, out window, out device);

        window.Resized += () =>
        {
            device.ResizeMainWindow((uint) window.Width, (uint) window.Height);
            invalidatePixelMatrix();
        };

        pipeline = new GraphicsPipeline(device);
        
        buffer = new VertexBuffer<VertexPositionColour>(pipeline);
        var color = RgbaFloat.Red;
        
        buffer.Add(new VertexPositionColour(new Vector2(0 + 20, 0 + 20), color));
        buffer.Add(new VertexPositionColour(new Vector2(50 + 20, 0 + 20), color));
        buffer.Add(new VertexPositionColour(new Vector2(0 + 20, 50 + 20), color));
        buffer.Add(new VertexPositionColour(new Vector2(50 + 20, 50 + 20), color));

        shader = new ShaderSet(new BasicVertexShader(), new BasicFragmentShader());
        
        invalidatePixelMatrix();
    }

    #endregion

    #region Drawing

    private mat4 pixelMatrix;
    private readonly Cached pixelMatrixBufferCache = new();

    private void draw(double dt)
    {
        pipeline.Start();
        
        if (!pixelMatrixBufferCache.IsValid)
        {
            pipeline.SetGlobalUniform(GlobalProperties.PixelMatrix, pixelMatrix);
            pixelMatrixBufferCache.Validate();
        }
        
        pipeline.BindShader(shader);
        pipeline.DrawVertexBuffer(buffer);
        pipeline.UnbindShader();
        pipeline.End();

        pipeline.Flush();
    }
    
    private void invalidatePixelMatrix()
    {
        pixelMatrix = mat4.Ortho(0, window.Width, window.Height, 0, -1, 1);
        pixelMatrixBufferCache.Invalidate();
    }

    #endregion

    public void Dispose()
    {
        pipeline.Dispose();

        GC.SuppressFinalize(this);
    }
}
