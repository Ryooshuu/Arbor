using Veldrid;

namespace Arbor.Graphics.Shaders.Vertices;

public class VertexBuffer<T> : IVertexBuffer
    where T : unmanaged
{
    private readonly List<T> vertices = new();
    private ushort[] indices = null!;

    public DeviceBuffer? VerticesBuffer { get; private set; }

    public DeviceBuffer? IndicesBuffer { get; private set; }

    public uint Length => (uint) indices.Length;

    private readonly DevicePipeline pipeline;

    public VertexBuffer(DevicePipeline pipeline)
    {
        this.pipeline = pipeline;
    }

    public void Add(T vertex)
    {
        vertices.Add(vertex);
        indices = new ushort[vertices.Count];

        for (var i = 0; i < indices.Length; i++)
            indices[i] = (ushort) i;

        VerticesBuffer?.Dispose();
        IndicesBuffer?.Dispose();

        VerticesBuffer = pipeline.CreateBuffer(vertices.ToArray(), BufferUsage.VertexBuffer);
        IndicesBuffer = pipeline.CreateBuffer(indices.ToArray(), BufferUsage.IndexBuffer);
    }

    public void Dispose()
    {
        VerticesBuffer?.Dispose();
        IndicesBuffer?.Dispose();
    }
}
