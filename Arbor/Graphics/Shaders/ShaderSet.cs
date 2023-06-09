using Veldrid;

namespace Arbor.Graphics.Shaders;

public class ShaderSet
{
    private readonly Shader[] shaders;
    private readonly ShaderSetDefinition definition;

    private CompiledShaderSet? cachedCompiledShaders;

    public Shader Vertex => shaders[0];
    public Shader? Fragment => shaders[1];
    public Shader Compute => shaders[0];

    public ShaderSet(Shader vertex, Shader fragment)
        : this(ShaderSetDefinition.VertexFragment, new[] { vertex, fragment })
    {
        if (!vertex.Stage.HasFlag(ShaderStages.Vertex))
            throw new ArgumentException($"Shader must be of type \"{nameof(ShaderStages.Vertex)}\".", nameof(vertex));
        if (!fragment.Stage.HasFlag(ShaderStages.Fragment))
            throw new ArgumentException($"Shader must be of type \"{nameof(ShaderStages.Fragment)}\".", nameof(fragment));
    }

    public ShaderSet(Shader compute)
        : this(ShaderSetDefinition.Compute, new[] { compute })
    {
        if (!compute.Stage.HasFlag(ShaderStages.Compute))
            throw new ArgumentException($"Shader must be of type \"{nameof(ShaderStages.Compute)}\".", nameof(compute));
    }
    
    private ShaderSet(ShaderSetDefinition definition, Shader[] shaders)
    {
        this.definition = definition;

        if (shaders.Length > 2)
            throw new ArgumentException($"{nameof(shaders)} cannot have a length longer than 2.");

        this.shaders = shaders;
    }

    public CompiledShaderSet GetCompiledShaders(GraphicsPipeline pipeline)
    {
        if (cachedCompiledShaders != null)
            return cachedCompiledShaders;

        var compiled = compileShaders(pipeline);
        cachedCompiledShaders?.Dispose();
        cachedCompiledShaders = compiled;

        return cachedCompiledShaders;
    }

    private CompiledShaderSet compileShaders(GraphicsPipeline pipeline)
    {
        var compiledShaders = definition switch
        {
            ShaderSetDefinition.VertexFragment => pipeline.CompileShaders(Vertex, Fragment!),
            ShaderSetDefinition.Compute => pipeline.CompileShaders(Compute),
            _ => throw new Exception("Unknown shader definition type")
        };

        return new CompiledShaderSet(compiledShaders.ToArray());
    }

    public IEnumerable<VertexLayoutDescription> CreateVertexLayouts()
        => shaders.Select(s => new VertexLayoutDescription(s.CreateVertexDescriptions()))
           .Where(l => l.Elements.Length > 0);

    public IEnumerable<ResourceLayoutDescription> CreateResourceLayouts()
        => shaders.Select(s => new ResourceLayoutDescription(s.CreateResourceDescriptions()))
           .Where(l => l.Elements.Length > 0);
}

public class CompiledShaderSet : IDisposable
{
    private readonly Veldrid.Shader[] shaders;
    
    public Veldrid.Shader Vertex => shaders[0];
    public Veldrid.Shader? Fragment => shaders[1];
    public Veldrid.Shader Compute => shaders[0];
    
    public CompiledShaderSet(Veldrid.Shader[] shaders)
    {
        this.shaders = shaders;
    }

    public void Dispose()
    {
        foreach (var shader in shaders)
            shader.Dispose();
    }
}

public enum ShaderSetDefinition
{
    VertexFragment,
    Compute
}