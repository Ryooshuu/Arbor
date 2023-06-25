using Veldrid;

namespace Arbor.Graphics.Shaders;

public class ShaderSet : IShaderSet
{
    private readonly ShaderSetDefinition definition;
    
    private CompiledShaderSet? cachedCompiledShaders;
    
    public IVertexShader? Vertex => Shaders[0] as IVertexShader;
    public IBindableShader? Fragment => Shaders[1] as IBindableShader;
    public IShader? Compute => Shaders[0];
    public IShader[] Shaders { get; }
    
    public ShaderSet(IVertexShader vertex, IBindableShader fragment)
        : this(ShaderSetDefinition.VertexFragment, new IShader[] { vertex, fragment })
    {
    }
    
    public ShaderSet(IShader compute)
        : this(ShaderSetDefinition.Compute, new[] { compute })
    {
        if (!compute.Stage.HasFlag(ShaderStages.Compute))
            throw new ArgumentException($"Shader must be of type \"{nameof(ShaderStages.Compute)}\".", nameof(compute));
    }
    
    private ShaderSet(ShaderSetDefinition definition, IShader[] shaders)
    {
        this.definition = definition;

        if (shaders.Length > 2)
            throw new ArgumentException($"{nameof(shaders)} cannot have a length longer than 2.");

        Shaders = shaders;
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
            ShaderSetDefinition.VertexFragment => pipeline.CompileShaders(Vertex!, Fragment!),
            ShaderSetDefinition.Compute => pipeline.CompileShaders(Compute!),
            _ => throw new Exception("Unknown shader definition type")
        };

        return new CompiledShaderSet(compiledShaders.ToArray());
    }

    public IEnumerable<VertexLayoutDescription> CreateVertexLayouts()
        => new VertexLayoutDescription[] { new(Vertex?.CreateVertexDescriptions()) };

    public IEnumerable<ResourceLayoutDescription> CreateResourceLayouts()
        => new ResourceLayoutDescription[] { new(Fragment?.CreateResourceDescriptions()) };
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
