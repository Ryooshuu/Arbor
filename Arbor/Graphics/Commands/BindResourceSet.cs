using Veldrid;

namespace Arbor.Graphics.Commands;

public class BindResourceSet : DrawCommand
{
    private readonly uint slot;
    private readonly ResourceSet? set;

    public BindResourceSet(DrawPipeline pipeline, uint slot, ResourceSet? set)
        : base(pipeline)
    {
        this.slot = slot;
        this.set = set;
    }

    public override void Execute(CommandList cl)
    {
        cl.SetGraphicsResourceSet(slot, set);
    }
}
