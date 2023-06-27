﻿using System.Diagnostics;
using Arbor.Caching;
using Arbor.Graphics;
using Arbor.Graphics.Shaders;
using Arbor.Graphics.Shaders.Basics;
using Arbor.Graphics.Shaders.Uniforms;
using Arbor.Graphics.Shaders.Vertices;
using GlmSharp;
using Veldrid;
using Veldrid.ImageSharp;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Arbor;

public class Window : IDisposable
{
    private readonly WindowCreateInfo createInfo;
    private DevicePipeline pipeline = null!;

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

    private VertexBuffer<VertexUvColour> buffer = null!;
    private IShaderSet shader = null!;

    private void createWindow()
    {
        var options = new GraphicsDeviceOptions { PreferStandardClipSpaceYDirection = true };
        VeldridStartup.CreateWindowAndGraphicsDevice(createInfo, options, out window, out device);

        window.Resized += () =>
        {
            device.ResizeMainWindow((uint) window.Width, (uint) window.Height);
            invalidatePixelMatrix();
        };

        pipeline = new DevicePipeline(device);

        buffer = pipeline.CreateVertexBuffer<VertexUvColour>();
        var color = RgbaFloat.White;

        const float size = 200;
        
        buffer.Add(new VertexUvColour(new vec2(0 + 20, 0 + 20), new vec2(0, 0), color));
        buffer.Add(new VertexUvColour(new vec2(size + 20, 0 + 20), new vec2(1, 0), color));
        buffer.Add(new VertexUvColour(new vec2(0 + 20, size + 20), new vec2(0, 1), color));
        buffer.Add(new VertexUvColour(new vec2(size + 20, size + 20), new vec2(1, 1), color));

        var imageSharpTexture = new ImageSharpTexture(@"D:\Projects\Projects\Arbor\Arbor.Resources\Textures\10-wKGO250UVi.png");
        shader = new ShaderSet(new TexturedVertexShader(), new TexturedFragmentShader(pipeline.CreateDeviceTextureView(imageSharpTexture), pipeline.GetDefaultSampler()));

        invalidatePixelMatrix();
    }

    #endregion

    #region Drawing

    private mat4 pixelMatrix;
    private readonly Cached pixelMatrixBufferCache = new();
    private DrawPipeline drawPipeline => pipeline.DrawPipeline;

    private void draw(double dt)
    {
        drawPipeline.Start();

        if (!pixelMatrixBufferCache.IsValid)
        {
            drawPipeline.SetGlobalUniform(GlobalProperties.PixelMatrix, pixelMatrix);
            pixelMatrixBufferCache.Validate();
        }

        drawPipeline.BindShader(shader);
        drawPipeline.DrawVertexBuffer(buffer);
        drawPipeline.UnbindShader();
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
        pipeline.Dispose();

        GC.SuppressFinalize(this);
    }
}
