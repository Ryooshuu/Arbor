using Arbor.Graphics.Shaders.Vertices;
using Veldrid;

namespace Arbor.Graphics.Commands;

public class DrawVertexBuffer : DrawCommand
{
    private readonly IVertexBuffer buffer;

    public DrawVertexBuffer(GraphicsPipeline pipeline, IVertexBuffer buffer)
        : base(pipeline)
    {
        this.buffer = buffer;
    }

    public override void Execute(CommandList cl)
    {
        cl.SetVertexBuffer(0, buffer.VerticesBuffer);
        cl.SetIndexBuffer(buffer.IndicesBuffer, IndexFormat.UInt16);
        cl.DrawIndexed(buffer.Length);
    }
}
