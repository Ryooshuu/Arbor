using Veldrid;

namespace Arbor.Graphics.Utils;

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

    public GraphicsPipelineDescriptionBuilder PushResourceLayouts(ResourceLayout layout)
    {
        description.ResourceLayouts = addToImmutableArray(description.ResourceLayouts, layout);
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

    public GraphicsPipelineDescriptionBuilder SetShaderSet(VertexLayoutDescription layout, Shader[] shader)
    {
        description.ShaderSet = new ShaderSetDescription(Array.Empty<VertexLayoutDescription>(), Array.Empty<Shader>());
        return AddShaderSet(layout, shader);
    }

    public GraphicsPipelineDescriptionBuilder SetShaderSet(VertexLayoutDescription layout, Shader shader)
    {
        description.ShaderSet = new ShaderSetDescription(Array.Empty<VertexLayoutDescription>(), Array.Empty<Shader>());
        return AddShaderSet(layout, shader);
    }

    public GraphicsPipelineDescriptionBuilder SetShaderSet()
    {
        description.ShaderSet = new ShaderSetDescription(Array.Empty<VertexLayoutDescription>(), Array.Empty<Shader>());
        return this;
    }

    public GraphicsPipelineDescriptionBuilder AddShaderSet(VertexLayoutDescription layout, Shader[] shader)
    {
        description.ShaderSet.VertexLayouts = addToImmutableArray(description.ShaderSet.VertexLayouts, layout);
        description.ShaderSet.Shaders = addToImmutableArray(description.ShaderSet.Shaders, shader);

        return this;
    }

    public GraphicsPipelineDescriptionBuilder AddShaderSet(VertexLayoutDescription layout, Shader shader)
    {
        description.ShaderSet.VertexLayouts = addToImmutableArray(description.ShaderSet.VertexLayouts, layout);
        description.ShaderSet.Shaders = addToImmutableArray(description.ShaderSet.Shaders, shader);

        return this;
    }

    public GraphicsPipelineDescriptionBuilder SetOutput(OutputDescription output)
    {
        description.Outputs = output;
        return this;
    }

    public GraphicsPipelineDescription Build()
    {
        return description;
    }

    // TODO: move this to its own utilities folder
    private static T[] addToImmutableArray<T>(IEnumerable<T> values, T value)
    {
        var list = values.ToList();
        list.Add(value);

        return list.ToArray();
    }

    private static T[] addToImmutableArray<T>(IEnumerable<T> values, T[] value)
    {
        var list = values.ToList();
        list.AddRange(value);

        return list.ToArray();
    }
}
