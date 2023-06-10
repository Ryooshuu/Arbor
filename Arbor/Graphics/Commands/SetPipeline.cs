using Veldrid;

namespace Arbor.Graphics.Commands;

public class SetPipeline : DrawCommand
{
    private readonly Pipeline veldridPipeline;

    public SetPipeline(GraphicsPipeline pipeline, Pipeline veldridPipeline)
        : base(pipeline)
    {
        this.veldridPipeline = veldridPipeline;
    }

    public override void Execute(CommandList cl)
    {
        cl.SetPipeline(veldridPipeline);
    }
}
