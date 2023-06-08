using Veldrid;

namespace Arbor.Graphics.Vertices;

public class VertexBuffer<T> : IVertexBuffer
    where T : unmanaged
{
    private readonly List<T> vertices = new();
    private ushort[] indices = null!;
    private DeviceBuffer? vertexBuffer;
    private DeviceBuffer? indexBuffer;

    public DeviceBuffer? VerticesBuffer => vertexBuffer;
    public DeviceBuffer? IndicesBuffer => indexBuffer;
    public uint Length => (uint) indices.Length;

    private readonly GraphicsPipeline pipeline;

    public VertexBuffer(GraphicsPipeline pipeline)
    {
        this.pipeline = pipeline;
    }

    public void Add(T vertex)
    {
        vertices.Add(vertex);
        indices = new ushort[vertices.Count];

        for (var i = 0; i < indices.Length; i++)
            indices[i] = (ushort)i;
        
        vertexBuffer?.Dispose();
        indexBuffer?.Dispose();

        vertexBuffer = pipeline.CreateBuffer(vertices.ToArray(), BufferUsage.VertexBuffer);
        indexBuffer = pipeline.CreateBuffer(indices.ToArray(), BufferUsage.IndexBuffer);
    }

    public void Dispose()
    {
        vertexBuffer?.Dispose();
        indexBuffer?.Dispose();
    }
}
