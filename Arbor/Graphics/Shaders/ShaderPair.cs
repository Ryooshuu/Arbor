using Veldrid;

namespace Arbor.Graphics.Shaders;

public class ShaderPair
{
    private readonly Shader[] shaders;
    private readonly ShaderPairDefinition definition;

    private CompiledShaderPair? cachedCompiledShaders;

    public Shader Vertex => shaders[0];
    public Shader? Fragment => shaders[1];
    public Shader Compute => shaders[0];

    public ShaderPair(Shader vertex, Shader fragment)
        : this(ShaderPairDefinition.VertexFragment, new[] { vertex, fragment })
    {
        if (!vertex.Stage.HasFlag(ShaderStages.Vertex))
            throw new ArgumentException($"Shader must be of type \"{nameof(ShaderStages.Vertex)}\".", nameof(vertex));
        if (!fragment.Stage.HasFlag(ShaderStages.Fragment))
            throw new ArgumentException($"Shader must be of type \"{nameof(ShaderStages.Fragment)}\".", nameof(fragment));
    }

    public ShaderPair(Shader compute)
        : this(ShaderPairDefinition.Compute, new[] { compute })
    {
        if (!compute.Stage.HasFlag(ShaderStages.Compute))
            throw new ArgumentException($"Shader must be of type \"{nameof(ShaderStages.Compute)}\".", nameof(compute));
    }
    
    private ShaderPair(ShaderPairDefinition definition, Shader[] shaders)
    {
        this.definition = definition;

        if (shaders.Length > 2)
            throw new ArgumentException($"{nameof(shaders)} cannot have a length longer than 2.");

        this.shaders = shaders;
    }

    public CompiledShaderPair GetCompiledShaders(GraphicsPipeline pipeline)
    {
        if (cachedCompiledShaders != null)
            return cachedCompiledShaders;

        var compiled = compileShaders(pipeline);
        cachedCompiledShaders?.Dispose();
        cachedCompiledShaders = compiled;

        return cachedCompiledShaders;
    }

    private CompiledShaderPair compileShaders(GraphicsPipeline pipeline)
    {
        var compiledShaders = definition switch
        {
            ShaderPairDefinition.VertexFragment => pipeline.CompileShaders(Vertex, Fragment!),
            ShaderPairDefinition.Compute => pipeline.CompileShaders(Compute),
            _ => throw new Exception("Unknown shader definition type")
        };

        return new CompiledShaderPair(compiledShaders.ToArray());
    }

    public IEnumerable<VertexLayoutDescription> CreateVertexLayouts()
        => shaders.Select(s => new VertexLayoutDescription(s.CreateVertexDescriptions()))
           .Where(l => l.Elements.Length > 0);

    public IEnumerable<ResourceLayoutDescription> CreateResourceLayouts()
        => shaders.Select(s => new ResourceLayoutDescription(s.CreateResourceDescriptions()))
           .Where(l => l.Elements.Length > 0);
}

public class CompiledShaderPair : IDisposable
{
    private readonly Veldrid.Shader[] shaders;
    
    public Veldrid.Shader Vertex => shaders[0];
    public Veldrid.Shader? Fragment => shaders[1];
    public Veldrid.Shader Compute => shaders[0];
    
    public CompiledShaderPair(Veldrid.Shader[] shaders)
    {
        this.shaders = shaders;
    }

    public void Dispose()
    {
        foreach (var shader in shaders)
            shader.Dispose();
    }
}

public enum ShaderPairDefinition
{
    VertexFragment,
    Compute
}