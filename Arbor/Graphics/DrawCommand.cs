using Veldrid;

namespace Arbor.Graphics;

public abstract class DrawCommand
{
    protected GraphicsPipeline Pipeline { get; }

    protected DrawCommand(GraphicsPipeline pipeline)
    {
        Pipeline = pipeline;
    }

    public abstract void Execute(CommandList cl);
}
