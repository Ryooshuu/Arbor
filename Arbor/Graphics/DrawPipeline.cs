using Arbor.Caching;
using Arbor.Graphics.Commands;
using Arbor.Graphics.Shaders;
using Arbor.Graphics.Shaders.Uniforms;
using Arbor.Graphics.Shaders.Vertices;
using Arbor.Utils;
using Veldrid;

namespace Arbor.Graphics;

public class DrawPipeline : IDisposable
{
    private readonly DrawStack drawStack = new();

    private readonly CommandList commandList;
    private GraphicsPipelineDescription defaultPipelineDescription;
    private readonly Cached<Pipeline> pipeline = new();
    
    public DevicePipeline DevicePipeline { get; }
    
    public DrawPipeline(DevicePipeline pipeline)
    {
        DevicePipeline = pipeline;
        
        commandList = DevicePipeline.Factory.CreateCommandList();
        GlobalPropertyManager.Init(DevicePipeline);
        createDefaultPipeline();
    }
    
    public void Start()
    {
        drawStack.Push(new DrawStart(this));
    }

    public void BindShader(IShaderSet set)
    {
        var compiledShaders = set.GetCompiledShaders(DevicePipeline);
        var vertexLayouts = set.CreateVertexLayouts();
        var resourceLayouts = set.CreateResourceLayouts().Select(DevicePipeline.Factory.CreateResourceLayout).ToArray();

        var builder = new GraphicsPipelineDescriptionBuilder(defaultPipelineDescription);
        builder.PushShaderSet(vertexLayouts.ToArray(), new[] { compiledShaders.Vertex, compiledShaders.Fragment! });
        builder.PushResourceLayouts(resourceLayouts);

        var shaderPipeline = DevicePipeline.CreatePipeline(builder.Build());
        pipeline.Value = shaderPipeline;
        drawStack.Push(new SetPipeline(this, GetPipeline()));

        uint slot = 1;
        
        foreach (var shader in set.Shaders)
        {
            if (shader is not IBindableShader bindableShader)
                continue;
            
            var descriptions = bindableShader.CreateResourceDescriptions();
            if (!descriptions.Any())
                continue;
            
            var resourceLayout = DevicePipeline.Factory.CreateResourceLayout(new ResourceLayoutDescription(descriptions));
            var resourceSet = DevicePipeline.Factory.CreateResourceSet(new ResourceSetDescription(resourceLayout, bindableShader.CreateBindableResources()));
            
            drawStack.Push(new BindResourceSet(this, slot, resourceSet));
            slot++;
        }
    }

    public void UnbindShader()
    {
        pipeline.Invalidate();
        drawStack.Push(new SetPipeline(this, GetPipeline()));
    }

    public void SetGlobalUniform<T>(GlobalProperties property, T value)
        where T : unmanaged
    {
        drawStack.Push(new UpdateGlobalUniform<T>(this, property, value));
    }

    public void DrawVertexBuffer(IVertexBuffer buffer)
    {
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
        
        DevicePipeline.Submit(commandList);
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
           .SetResourceLayouts(new[] { GlobalPropertyManager.GlobalResourceLayout })
           .SetShaderSet()
           .SetOutput(DevicePipeline.GetSwapchainFramebuffer().OutputDescription);
           // .SetOutput(device.SwapchainFramebuffer.OutputDescription);

        defaultPipelineDescription = builder.Build();
        return DevicePipeline.CreatePipeline(defaultPipelineDescription);
    }

    #endregion

    public void Dispose()
    {
        GlobalPropertyManager.Dispose();
        commandList.Dispose();

        if (pipeline.IsValid)
            pipeline.Value.Dispose();
    }
}
