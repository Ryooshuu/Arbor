using Veldrid;

namespace Arbor.Graphics;

public abstract class DrawCommand
{
    protected DrawPipeline Pipeline { get; }

    protected DrawCommand(DrawPipeline pipeline)
    {
        Pipeline = pipeline;
    }

    public abstract void Execute(CommandList cl);
}
