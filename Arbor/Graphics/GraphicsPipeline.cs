using System.Runtime.InteropServices;
using System.Text;
using Arbor.Caching;
using Arbor.Graphics.Commands;
using Arbor.Graphics.Utils;
using Arbor.Graphics.Vertices;
using Veldrid;
using Veldrid.SPIRV;

namespace Arbor.Graphics;

public class GraphicsPipeline : IDisposable
{
    private readonly GraphicsDevice device;
    private readonly DrawStack drawStack = new();

    private CommandList commandList = null!;
    private GraphicsPipelineDescription defaultPipelineDescription;
    private readonly Cached<Pipeline> pipeline = new();

    private readonly List<IVertexBuffer> aliveVertexBuffers = new();

    private ResourceFactory factory => device.ResourceFactory;

    public GraphicsPipeline(GraphicsDevice device)
    {
        this.device = device;
    }

    public void Flush()
    {
        while (drawStack.TryPop(out var command))
        {
            command.Execute(commandList);
        }

        device.SubmitCommands(commandList);
        device.SwapBuffers();
    }

    #region Drawing API

    public void Start()
    {
        drawStack.Push(new DrawStart(this));
    }

    public void DrawVertexBuffer(IVertexBuffer buffer)
    {
        aliveVertexBuffers.Add(buffer);
        drawStack.Push(new DrawVertexBuffer(this, buffer));
    }

    public void End()
    {
        drawStack.Push(new DrawEnd(this));
    }

    #endregion
    
    #region Device API

    public Framebuffer GetSwapchainFramebuffer()
    {
        return device.SwapchainFramebuffer;
    }

    public DeviceBuffer CreateBuffer<T>(T[] values, BufferUsage usage, uint? size = null)
        where T : unmanaged
    {
        var bufferSize = (uint) (values.Length * Marshal.SizeOf<T>());

        if (size.HasValue)
            bufferSize = size.Value;

        var buffer = factory.CreateBuffer(new BufferDescription(bufferSize, usage));
        device.UpdateBuffer(buffer, 0, values);

        return buffer;
    }

    #endregion

    internal void Initialize()
    {
        commandList = device.ResourceFactory.CreateCommandList();
        createDefaultPipeline();
    }

    #region Veldrid Pipeline

    public Pipeline GetPipeline()
    {
        if (pipeline.IsValid)
            return pipeline.Value;

        return pipeline.Value = createDefaultPipeline();
    }

    private Pipeline createDefaultPipeline()
    {
        var builder = new GraphicsPipelineDescriptionBuilder();

        builder.SetDepthStencilState(new DepthStencilStateDescription
            {
                DepthTestEnabled = true,
                DepthWriteEnabled = true,
                DepthComparison = ComparisonKind.LessEqual
            })
           .SetRasterizerState(new RasterizerStateDescription
            {
                CullMode = FaceCullMode.Back,
                FillMode = PolygonFillMode.Solid,
                FrontFace = FrontFace.Clockwise,
                DepthClipEnabled = true,
                ScissorTestEnabled = false
            })
           .SetPrimitiveTopology(PrimitiveTopology.TriangleStrip)
           .SetBlendState(BlendStateDescription.SingleAlphaBlend)
           .SetResourceLayouts()
           .SetShaderSet()
           .SetOutput(device.SwapchainFramebuffer.OutputDescription);

        // TEMPORARY //

        const string vertex_code = @"
#version 450

layout(location = 0) in vec2 Position;
layout(location = 1) in vec4 Color;

layout(location = 0) out vec4 fsin_Color;

void main()
{
    gl_Position = vec4(Position, 0, 1);
    fsin_Color = Color;
}";

        const string fragment_code = @"
#version 450

layout(location = 0) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

void main()
{
    fsout_Color = fsin_Color;
}";

        var vertexShaderDesc = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertex_code), "main");
        var fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragment_code), "main");
        var shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

        // TEMPORARY //

        builder.AddShaderSet(
            new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
            ),
            shaders
        );

        defaultPipelineDescription = builder.Build();
        return device.ResourceFactory.CreateGraphicsPipeline(defaultPipelineDescription);
    }

    #endregion

    public void Dispose()
    {
        foreach (var b in aliveVertexBuffers)
            b.Dispose();

        commandList.Dispose();
        device.Dispose();

        if (pipeline.IsValid)
            pipeline.Value.Dispose();
    }
}
