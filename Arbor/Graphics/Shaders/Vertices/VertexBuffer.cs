using System.Runtime.InteropServices;
using Arbor.Statistics;
using Veldrid;

namespace Arbor.Graphics.Shaders.Vertices;

public class VertexBuffer<T> : IVertexBuffer
    where T : unmanaged
{
    private readonly List<T> vertices = new List<T>();
    private ushort[] indices = Array.Empty<ushort>();

    public DeviceBuffer? VerticesBuffer { get; private set; }

    public DeviceBuffer? IndicesBuffer { get; private set; }

    public uint Length => (uint) indices.Length;

    private readonly DevicePipeline pipeline;
    private readonly IndexLayout layout;

    internal VertexBuffer(DevicePipeline pipeline, IndexLayout layout)
    {
        this.pipeline = pipeline;
        this.layout = layout;
    }

    public void Add(T vertex)
    {
        vertices.Add(vertex);

        updateIndices();
        updateBuffers();
        
        FrameStatistics.Add(StatisticsCounterType.Vertices, 1);
        FrameStatistics.Add(StatisticsCounterType.VertexMemUsage, Marshal.SizeOf<T>());
    }

    public void AddRange(IEnumerable<T> vertexArray)
    {
        vertices.AddRange(vertexArray);

        updateIndices();
        updateBuffers();

        var count = vertexArray.Count();
        FrameStatistics.Add(StatisticsCounterType.Vertices, count);
        FrameStatistics.Add(StatisticsCounterType.VertexMemUsage, (long) count * Marshal.SizeOf<T>());
    }

    private void updateIndices()
    {
        const int vertices_per_quad = 4;
        const int indices_per_quad = vertices_per_quad + 2;

        FrameStatistics.Decrement(StatisticsCounterType.Indices, indices.Length);
        FrameStatistics.Decrement(StatisticsCounterType.IndexMemUsage, (long) indices.Length * sizeof(ushort));

        indices = new ushort[translateToIndex(vertices.Count)];

        switch (layout)
        {
            default:
            case IndexLayout.Linear:
                for (var i = 0; i < indices.Length; i++)
                    indices[i] = (ushort) i;
                break;
            case IndexLayout.Quad:
                for (ushort i = 0, j = 0; j < indices.Length; i += vertices_per_quad, j += indices_per_quad)
                {
                    indices[j] = i;
                    indices[j + 1] = (ushort)(i + 1);
                    indices[j + 2] = (ushort)(i + 3);
                    indices[j + 3] = i;
                    indices[j + 4] = (ushort)(i + 2);
                    indices[j + 5] = (ushort)(i + 3);
                }
                break;
        }
        
        FrameStatistics.Add(StatisticsCounterType.Indices, indices.Length);
        FrameStatistics.Add(StatisticsCounterType.IndexMemUsage, (long) indices.Length * sizeof(ushort));
    }

    private int translateToIndex(int vertexIndex)
    {
        switch (layout)
        {
            default:
            case IndexLayout.Linear:
                return vertexIndex;

            case IndexLayout.Quad:
                return 3 * vertexIndex / 2;
        }
    }

    public void Clear()
    {
        FrameStatistics.Decrement(StatisticsCounterType.Vertices, vertices.Count);
        FrameStatistics.Decrement(StatisticsCounterType.Indices, indices.Length);
        FrameStatistics.Decrement(StatisticsCounterType.VertexMemUsage, (long) vertices.Count * Marshal.SizeOf<T>());
        FrameStatistics.Decrement(StatisticsCounterType.IndexMemUsage, (long) indices.Length * sizeof(ushort));
        
        vertices.Clear();
        indices = Array.Empty<ushort>();

        VerticesBuffer?.Dispose();
        IndicesBuffer?.Dispose();
        
        FrameStatistics.Decrement(StatisticsCounterType.Buffers, 2);
    }

    private void updateBuffers()
    {
        if (VerticesBuffer != null)
            FrameStatistics.Decrement(StatisticsCounterType.Buffers, 2);
        
        VerticesBuffer?.Dispose();
        IndicesBuffer?.Dispose();

        VerticesBuffer = pipeline.CreateBuffer(vertices.ToArray(), BufferUsage.VertexBuffer);
        IndicesBuffer = pipeline.CreateBuffer(indices.ToArray(), BufferUsage.IndexBuffer);
        FrameStatistics.Add(StatisticsCounterType.Buffers, 2);
    }

    public void Dispose()
    {
        VerticesBuffer?.Dispose();
        IndicesBuffer?.Dispose();
        
        FrameStatistics.Decrement(StatisticsCounterType.Buffers, 2);
    }
}

public enum IndexLayout
{
    Linear,
    Quad
}
