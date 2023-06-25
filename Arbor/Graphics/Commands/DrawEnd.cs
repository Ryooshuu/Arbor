using Veldrid;

namespace Arbor.Graphics.Commands;

public class DrawEnd : DrawCommand
{
    public DrawEnd(DrawPipeline pipeline)
        : base(pipeline)
    {
    }

    public override void Execute(CommandList cl)
    {
        cl.End();
    }
}
