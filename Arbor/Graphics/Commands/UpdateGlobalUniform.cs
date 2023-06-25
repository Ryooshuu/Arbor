using Arbor.Graphics.Shaders.Uniforms;
using Veldrid;

namespace Arbor.Graphics.Commands;

public class UpdateGlobalUniform<T> : DrawCommand
    where T : unmanaged
{
    private readonly GlobalProperties property;
    private readonly T value;

    public UpdateGlobalUniform(DrawPipeline pipeline, GlobalProperties property, T value)
        : base(pipeline)
    {
        this.property = property;
        this.value = value;
    }

    public override void Execute(CommandList cl)
    {
        GlobalPropertyManager.Set(cl, property, value);
    }
}
