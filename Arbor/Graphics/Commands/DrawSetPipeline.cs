using Veldrid;

namespace Arbor.Graphics.Commands;

public class DrawSetPipeline : DrawCommand
{
    private readonly Pipeline veldridPipeline;

    public DrawSetPipeline(GraphicsPipeline pipeline, Pipeline veldridPipeline)
        : base(pipeline)
    {
        this.veldridPipeline = veldridPipeline;
    }

    public override void Execute(CommandList cl)
    {
        cl.SetPipeline(veldridPipeline);
    }
}
