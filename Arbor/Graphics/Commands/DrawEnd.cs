using Veldrid;

namespace Arbor.Graphics.Commands;

public class DrawEnd : DrawCommand
{
    public DrawEnd(GraphicsPipeline pipeline)
        : base(pipeline)
    {
    }

    public override void Execute(CommandList cl)
    {
        cl.End();
    }
}
