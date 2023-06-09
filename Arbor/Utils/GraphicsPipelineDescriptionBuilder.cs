using Veldrid;

namespace Arbor.Utils;

public class GraphicsPipelineDescriptionBuilder
{
    private GraphicsPipelineDescription description;

    public GraphicsPipelineDescriptionBuilder()
    {
        description = new GraphicsPipelineDescription();
    }

    public GraphicsPipelineDescriptionBuilder(GraphicsPipelineDescription description)
    {
        this.description = description;
    }

    public GraphicsPipelineDescriptionBuilder SetDepthStencilState(DepthStencilStateDescription stencil)
    {
        description.DepthStencilState = stencil;
        return this;
    }

    public GraphicsPipelineDescriptionBuilder SetRasterizerState(RasterizerStateDescription rasterizer)
    {
        description.RasterizerState = rasterizer;
        return this;
    }

    public GraphicsPipelineDescriptionBuilder SetPrimitiveTopology(PrimitiveTopology topology)
    {
        description.PrimitiveTopology = topology;
        return this;
    }

    public GraphicsPipelineDescriptionBuilder SetBlendState(BlendStateDescription blending)
    {
        description.BlendState = blending;
        return this;
    }

    public GraphicsPipelineDescriptionBuilder SetResourceLayouts(ResourceLayout[] layouts)
    {
        description.ResourceLayouts = layouts;
        return this;
    }

    public GraphicsPipelineDescriptionBuilder SetResourceLayouts()
    {
        description.ResourceLayouts = Array.Empty<ResourceLayout>();
        return this;
    }

    public GraphicsPipelineDescriptionBuilder PushResourceLayouts(ResourceLayout[] layout)
    {
        description.ResourceLayouts = ArrayExtensions.AddToImmutableArray(description.ResourceLayouts, layout);
        return this;
    }

    public GraphicsPipelineDescriptionBuilder ClearResourceLayouts()
    {
        description.ResourceLayouts = Array.Empty<ResourceLayout>();
        return this;
    }

    public GraphicsPipelineDescriptionBuilder SetShaderSet(ShaderSetDescription shader)
    {
        description.ShaderSet = shader;
        return this;
    }

    public GraphicsPipelineDescriptionBuilder SetShaderSet(VertexLayoutDescription[] layout, Shader[] shader)
    {
        description.ShaderSet = new ShaderSetDescription(Array.Empty<VertexLayoutDescription>(), Array.Empty<Shader>());
        return PushShaderSet(layout, shader);
    }

    public GraphicsPipelineDescriptionBuilder SetShaderSet(VertexLayoutDescription layout, Shader shader)
    {
        description.ShaderSet = new ShaderSetDescription(Array.Empty<VertexLayoutDescription>(), Array.Empty<Shader>());
        return PushShaderSet(layout, shader);
    }

    public GraphicsPipelineDescriptionBuilder SetShaderSet()
    {
        description.ShaderSet = new ShaderSetDescription(Array.Empty<VertexLayoutDescription>(), Array.Empty<Shader>());
        return this;
    }

    public GraphicsPipelineDescriptionBuilder PushShaderSet(VertexLayoutDescription[] layout, Shader[] shader)
    {
        description.ShaderSet.VertexLayouts = ArrayExtensions.AddToImmutableArray(description.ShaderSet.VertexLayouts, layout);
        description.ShaderSet.Shaders = ArrayExtensions.AddToImmutableArray(description.ShaderSet.Shaders, shader);

        return this;
    }

    public GraphicsPipelineDescriptionBuilder PushShaderSet(VertexLayoutDescription layout, Shader shader)
    {
        description.ShaderSet.VertexLayouts = ArrayExtensions.AddToImmutableArray(description.ShaderSet.VertexLayouts, layout);
        description.ShaderSet.Shaders = ArrayExtensions.AddToImmutableArray(description.ShaderSet.Shaders, shader);

        return this;
    }

    public GraphicsPipelineDescriptionBuilder SetOutput(OutputDescription output)
    {
        description.Outputs = output;
        return this;
    }

    public GraphicsPipelineDescription Build()
        => description;
}
