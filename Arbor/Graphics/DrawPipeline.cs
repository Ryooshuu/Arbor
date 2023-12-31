using Arbor.Caching;
using Arbor.Graphics.Commands;
using Arbor.Graphics.Shaders;
using Arbor.Graphics.Shaders.Uniforms;
using Arbor.Graphics.Shaders.Vertices;
using Arbor.Statistics;
using Arbor.Utils;
using GlmSharp;
using Veldrid;

namespace Arbor.Graphics;

public class DrawPipeline : IDisposable
{
    private readonly DrawStack drawStack = new DrawStack();
    private readonly Stack<mat4> matrixStack = new Stack<mat4>();

    private readonly CommandList commandList;
    private readonly GraphicsPipelineDescription defaultPipelineDescription;
    private readonly Cached<Pipeline> pipeline = new Cached<Pipeline>();
    
    internal readonly Queue<Action<CommandList>> DebugDrawQueue = new Queue<Action<CommandList>>();
    
    public DevicePipeline DevicePipeline { get; }
    
    public DrawPipeline(DevicePipeline pipeline)
    {
        DevicePipeline = pipeline;
        
        commandList = DevicePipeline.Factory.CreateCommandList();
        GlobalPropertyManager.Init(DevicePipeline);
        defaultPipelineDescription = CreateDefaultPipeline();
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

        var shaderPipeline = DevicePipeline.CreatePipeline(new GraphicsPipelineDescriptionBuilder(defaultPipelineDescription)
           .PushShaderSet(vertexLayouts.ToArray(), new[] { compiledShaders.Vertex, compiledShaders.Fragment! })
           .PushResourceLayouts(resourceLayouts)
           .Build());
        pipeline.Value = shaderPipeline;
        drawStack.Push(new SetPipeline(this, pipeline.Value));

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
        FrameStatistics.Increment(StatisticsCounterType.DrawCalls);
    }
    
    public void PushMatrix(mat4 matrix)
    {
        matrixStack.Push(matrix);
        var oldMatrix = matrixStack.Count > 1 ? matrixStack.Peek() : mat4.Identity;
        
        drawStack.Push(new UpdateGlobalUniform<mat4>(this, GlobalProperties.ModelMatrix, matrix * oldMatrix));
    }
    
    public void PopMatrix()
    {
        matrixStack.TryPop(out _);
        var oldMatrix = matrixStack.Count > 1 ? matrixStack.Peek() : mat4.Identity;
        
        drawStack.Push(new UpdateGlobalUniform<mat4>(this, GlobalProperties.ModelMatrix, oldMatrix));
    }

    public void End()
    {
        drawStack.Push(new DrawDebug(this));
        drawStack.Push(new DrawEnd(this));
    }

    public void Flush()
    {
        while (drawStack.TryPop(out var command))
            command.Execute(commandList);
        
        DevicePipeline.Submit(commandList);
    }
    
    internal void QueueForDebug(Action<CommandList> action)
    {
        DebugDrawQueue.Enqueue(action);
    }
    
    #region Veldrid Pipeline

    public Pipeline GetPipeline()
    {
        if (pipeline.IsValid)
            return pipeline.Value;

        return pipeline.Value = DevicePipeline.CreatePipeline(defaultPipelineDescription);
    }

    internal GraphicsPipelineDescription CreateDefaultPipeline()
    {
        var builder = new GraphicsPipelineDescriptionBuilder();

        builder.SetDepthStencilState(DepthStencilStateDescription.Disabled)
           .SetRasterizerState(RasterizerStateDescription.CullNone)
           .SetPrimitiveTopology(PrimitiveTopology.TriangleList)
           .SetBlendState(BlendStateDescription.SingleAlphaBlend)
           .SetResourceLayouts(new[] { GlobalPropertyManager.GlobalResourceLayout })
           .SetShaderSet()
           .SetOutput(DevicePipeline.GetSwapchainFramebuffer().OutputDescription);
        
        return builder.Build();
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
