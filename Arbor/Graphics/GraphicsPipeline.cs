using System.Runtime.InteropServices;
using Arbor.Caching;
using Arbor.Graphics.Commands;
using Arbor.Graphics.Shaders;
using Arbor.Graphics.Shaders.Uniforms;
using Arbor.Graphics.Shaders.Vertices;
using Arbor.Utils;
using Veldrid;
using Veldrid.SPIRV;
using Shader = Veldrid.Shader;

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
        GlobalPropertyManager.Init(this);
        createDefaultPipeline();
    }

    #region Drawing API

    public void Start()
    {
        drawStack.Push(new DrawStart(this));
    }

    public void BindShader(ShaderSet set)
    {
        var compiledShaders = set.GetCompiledShaders(this);
        var vertexLayouts = set.CreateVertexLayouts();
        var resourceLayouts = set.CreateResourceLayouts().Select(factory.CreateResourceLayout).ToArray();

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

    public void SetGlobalUniform<T>(GlobalProperties property, T value)
        where T : unmanaged
    {
        drawStack.Push(new DrawUpdateGlobalUniform<T>(this, property, value));
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
            command.Execute(commandList);

        device.SubmitCommands(commandList);
        device.SwapBuffers();
    }

    #endregion

    #region Device API

    public Framebuffer GetSwapchainFramebuffer()
        => device.SwapchainFramebuffer;

    public DeviceBuffer CreateBuffer<T>(T[] values, BufferUsage usage, uint? size = null)
        where T : unmanaged
    {
        var bufferSize = (uint) (values.Length * Marshal.SizeOf<T>());

        if (size.HasValue)
            bufferSize = size.Value;

        var buffer = factory.CreateBuffer(new BufferDescription(bufferSize, usage));
        if (values.Length > 0)
            device.UpdateBuffer(buffer, 0, values);

        return buffer;
    }

    public ResourceLayout CreateResourceLayout(ResourceLayoutElementDescription[] descriptions)
        => factory.CreateResourceLayout(new ResourceLayoutDescription(descriptions));

    public ResourceSet CreateResourceSet(ResourceLayout layout, BindableResource[] resources)
        => factory.CreateResourceSet(new ResourceSetDescription(layout, resources));

    public ResourceSet CreateResourceSet(ResourceLayoutElementDescription[] descriptions, BindableResource[] resources)
        => CreateResourceSet(CreateResourceLayout(descriptions), resources);

    public IEnumerable<Shader> CompileShaders(Shaders.Shader vertex, Shaders.Shader fragment)
        => factory.CreateFromSpirv(vertex.CreateShaderDescription(), fragment.CreateShaderDescription());

    public IEnumerable<Shader> CompileShaders(Shaders.Shader compute)
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
           .SetResourceLayouts(new[] { GlobalPropertyManager.GlobalResourceLayout })
           .SetShaderSet()
           .SetOutput(device.SwapchainFramebuffer.OutputDescription);

        defaultPipelineDescription = builder.Build();
        return createPipeline(defaultPipelineDescription);
    }

    private Pipeline createPipeline(GraphicsPipelineDescription description)
        => device.ResourceFactory.CreateGraphicsPipeline(description);

    #endregion

    public void Dispose()
    {
        foreach (var b in aliveVertexBuffers)
            b.Dispose();

        GlobalPropertyManager.Dispose();
        commandList.Dispose();
        device.Dispose();

        if (pipeline.IsValid)
            pipeline.Value.Dispose();
    }
}
