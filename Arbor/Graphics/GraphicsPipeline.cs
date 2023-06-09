using System.Runtime.InteropServices;
using Arbor.Caching;
using Arbor.Graphics.Commands;
using Arbor.Graphics.Shaders;
using Arbor.Graphics.Shaders.Vertices;
using Arbor.Utils;
using Veldrid;
using Veldrid.SPIRV;
using Shader = Arbor.Graphics.Shaders.Shader;

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
    
    internal void Initialize()
    {
        commandList = device.ResourceFactory.CreateCommandList();
        createDefaultPipeline();
    }

    #region Drawing API

    public void Start()
    {
        drawStack.Push(new DrawStart(this));
    }

    public void BindShader(ShaderPair pair)
    {
        var compiledShaders = pair.GetCompiledShaders(this);
        var vertexLayouts = pair.CreateVertexLayouts();
        var resourceLayouts = pair.CreateResourceLayouts().Select(factory.CreateResourceLayout).ToArray();

        var builder = new GraphicsPipelineDescriptionBuilder(defaultPipelineDescription);
        builder.PushShaderSet(vertexLayouts.ToArray(), new[] { compiledShaders.Vertex, compiledShaders.Fragment! });
        builder.PushResourceLayouts(resourceLayouts);

        var shaderPipeline = createPipeline(builder.Build());
        pipeline.Value = shaderPipeline;
        drawStack.Push(new DrawSetPipeline(this, GetPipeline()));
    }

    public void UnbindShader()
    {
        pipeline.Invalidate();
        drawStack.Push(new DrawSetPipeline(this, GetPipeline()));
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
    
    public void Flush()
    {
        while (drawStack.TryPop(out var command))
        {
            command.Execute(commandList);
        }

        device.SubmitCommands(commandList);
        device.SwapBuffers();
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

    public ResourceSet CreateResourceSet(ResourceLayoutElementDescription[] descriptions, BindableResource[] resources)
    {
        var layout = factory.CreateResourceLayout(new ResourceLayoutDescription(descriptions));
        return factory.CreateResourceSet(new ResourceSetDescription(layout, resources));
    }

    public IEnumerable<Veldrid.Shader> CompileShaders(Shader vertex, Shader fragment)
    {
        return factory.CreateFromSpirv(vertex.CreateShaderDescription(), fragment.CreateShaderDescription());
    }

    public IEnumerable<Veldrid.Shader> CompileShaders(Shader compute)
    {
        return new[] { factory.CreateFromSpirv(compute.CreateShaderDescription()) };
    }

    #endregion

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

        defaultPipelineDescription = builder.Build();
        return createPipeline(defaultPipelineDescription);
    }

    private Pipeline createPipeline(GraphicsPipelineDescription description)
    {
        return device.ResourceFactory.CreateGraphicsPipeline(description);
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
