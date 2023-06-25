using Veldrid;

namespace Arbor.Graphics.Shaders.Uniforms;

public interface IGlobalProperty : IDisposable
{
    public object Value { get; }
    public DeviceBuffer Buffer { get; }
    GlobalProperties Property { get; }

    void Init(DevicePipeline pipeline);
}
