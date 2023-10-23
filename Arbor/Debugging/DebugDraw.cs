using Arbor.Graphics;
using Arbor.Graphics.Shaders;
using Arbor.Graphics.Shaders.Vertices;
using Arbor.Utils;
using GlmSharp;
using Veldrid;
using RectangleF = System.Drawing.RectangleF;

namespace Arbor.Debugging;

/// <summary>
/// Immediate mode debug drawing.
/// Useful for drawing debug shapes, lines, and text.
/// </summary>
public static class DebugDraw
{
    internal static DevicePipeline Device = null!;
    internal static DrawPipeline Draw => Device.DrawPipeline;
    
    private static readonly IShaderSet shader = ShaderSetHelper.CreateBasicShaderSet();

    /// <summary>
    /// Draws a line from <paramref name="start"/> to <paramref name="end"/> with the given <paramref name="colour"/>.
    /// </summary>
    /// <param name="start">The point where the line begins.</param>
    /// <param name="end">The point where the line ends.</param>
    /// <param name="colour">The colour of the line.</param>
    public static void Line(vec2 start, vec2 end, RgbaFloat colour)
    {
        Shape(colour, start, end);
    }

    /// <summary>
    /// Draws a rectangle with the given <paramref name="colour"/>.
    /// Points should be in clockwise order.
    /// </summary>
    /// <param name="p0">The first point.</param>
    /// <param name="p1">The second point.</param>
    /// <param name="p2">The third point.</param>
    /// <param name="p3">The fourth point.</param>
    /// <param name="colour">The colour of the lines.</param>
    public static void Rect(vec2 p0, vec2 p1, vec2 p2, vec2 p3, RgbaFloat colour)
    {
        Shape(colour, p0, p1, p2, p3, p0);
    }

    /// <summary>
    /// Draws a rectangle with the given <paramref name="colour"/>.
    /// </summary>
    /// <param name="rectangle">The <see cref="RectangleF"/> to draw lines from.</param>
    /// <param name="colour">The colour of the lines.</param>
    public static void Rect(RectangleF rectangle, RgbaFloat colour)
    {
        var topLeft = new vec2(rectangle.Left, rectangle.Top);
        var topRight = new vec2(rectangle.Right, rectangle.Top);
        var bottomLeft = new vec2(rectangle.Left, rectangle.Bottom);
        var bottomRight = new vec2(rectangle.Right, rectangle.Bottom);
        
        Rect(topLeft, topRight, bottomRight, bottomLeft, colour);
    }

    /// <summary>
    /// Draws a shape with the given <paramref name="colour"/>.
    /// Points should be in clockwise order.
    /// </summary>
    /// <param name="colour">The colour of the lines.</param>
    /// <param name="points">An array of points, contiguous from the last point.</param>
    public static void Shape(RgbaFloat colour, params vec2[] points)
    {
        Draw.QueueForDebug(cl =>
        {
            var pipeline = getDefaultPipeline(Device);
            
            var vertices = points.Select(p => new VertexPositionColour(p, colour)).ToArray();
            var vertexBuffer = Device.CreateBuffer(vertices, BufferUsage.VertexBuffer);
            
            bindShader(shader, cl);
            
            cl.SetVertexBuffer(0, vertexBuffer);
            cl.Draw((uint) vertices.Length);
            cl.SetPipeline(pipeline);
            
            vertexBuffer.Dispose();
        });
    }

    private static Pipeline getDefaultPipeline(DevicePipeline device)
    {
        var defaultDescription = Device.DrawPipeline.CreateDefaultPipeline();
        defaultDescription.PrimitiveTopology = PrimitiveTopology.LineStrip;
        return device.CreatePipeline(defaultDescription);
    }
    
    private static void bindShader(IShaderSet set, CommandList cl)
    {
        var defaultDescription = Device.DrawPipeline.CreateDefaultPipeline();
        defaultDescription.PrimitiveTopology = PrimitiveTopology.LineStrip;
        
        var compiledShaders = set.GetCompiledShaders(Device);
        var vertexLayouts = set.CreateVertexLayouts();
        var resourceLayouts = set.CreateResourceLayouts().Select(Device.Factory.CreateResourceLayout).ToArray();

        var shaderPipeline = Device.CreatePipeline(new GraphicsPipelineDescriptionBuilder(defaultDescription)
           .PushShaderSet(vertexLayouts.ToArray(), new[] { compiledShaders.Vertex, compiledShaders.Fragment! })
           .PushResourceLayouts(resourceLayouts)
           .Build());
        cl.SetPipeline(shaderPipeline);
    }
}
