using Veldrid;

namespace Arbor.Graphics.Shaders;

public interface IVertexShader : IShader
{
    VertexElementDescription[] CreateVertexDescriptions();
}
