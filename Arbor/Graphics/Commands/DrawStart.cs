using Arbor.Graphics.Shaders.Uniforms;
using Veldrid;

namespace Arbor.Graphics.Commands;

public class DrawStart : DrawCommand
{
    public DrawStart(DrawPipeline pipeline)
        : base(pipeline)
    {
    }

    public override void Execute(CommandList cl)
    {
        cl.Begin();
        cl.SetFramebuffer(Pipeline.DevicePipeline.GetSwapchainFramebuffer());
        cl.ClearColorTarget(0, RgbaFloat.Black);
        cl.SetPipeline(Pipeline.GetPipeline());
        cl.SetGraphicsResourceSet(0, GlobalPropertyManager.GlobalResourceSet);
    }
}
