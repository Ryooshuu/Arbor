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
        Window.Igr.Render(Pipeline.DevicePipeline.Device, cl);
        cl.End();
    }
}
