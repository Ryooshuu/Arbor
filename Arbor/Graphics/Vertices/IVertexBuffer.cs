using Veldrid;

namespace Arbor.Graphics.Vertices;

public interface IVertexBuffer : IDisposable
{
    DeviceBuffer? VerticesBuffer { get; }
    DeviceBuffer? IndicesBuffer { get; }
    uint Length { get; }
}
