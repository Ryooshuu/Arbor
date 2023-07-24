using Veldrid;

namespace Arbor.Graphics.Shaders;

public interface IShaderSet : IDisposable
{
    IShader[] Shaders { get; }
    
    CompiledShaderSet GetCompiledShaders(DevicePipeline pipeline);

    IEnumerable<VertexLayoutDescription> CreateVertexLayouts();
    IEnumerable<ResourceLayoutDescription> CreateResourceLayouts();
}
