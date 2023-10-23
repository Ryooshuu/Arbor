using Veldrid;

namespace Arbor.Graphics.Commands;

public class DrawDebug : DrawCommand
{
    public DrawDebug(DrawPipeline pipeline)
        : base(pipeline)
    {
    }

    public override void Execute(CommandList cl)
    {
        while (Pipeline.DebugDrawQueue.TryDequeue(out var action))
            action(cl);
    }
}
