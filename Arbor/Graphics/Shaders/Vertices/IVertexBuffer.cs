using Veldrid;

namespace Arbor.Graphics.Shaders.Vertices;

public interface IVertexBuffer : IDisposable
{
    DeviceBuffer? VerticesBuffer { get; }
    DeviceBuffer? IndicesBuffer { get; }
    uint Length { get; }
}
