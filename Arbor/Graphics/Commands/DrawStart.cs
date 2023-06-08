﻿using Veldrid;

namespace Arbor.Graphics.Commands;

public class DrawStart : DrawCommand
{
    public DrawStart(GraphicsPipeline pipeline)
        : base(pipeline)
    {
    }

    public override void Execute(CommandList cl)
    {
        cl.Begin();
        cl.SetFramebuffer(Pipeline.GetSwapchainFramebuffer());
        cl.ClearColorTarget(0, RgbaFloat.Black);
        cl.SetPipeline(Pipeline.GetPipeline());
    }
}