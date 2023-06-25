using System.Runtime.InteropServices;
using Veldrid;

namespace Arbor.Graphics.Shaders.Uniforms;

public class GlobalProperty<T> : IGlobalProperty
    where T : unmanaged
{
    object IGlobalProperty.Value => Value;
    public T Value { get; private set; }
    public DeviceBuffer Buffer { get; private set; } = null!;
    public GlobalProperties Property { get; }

    private uint size => (uint) Marshal.SizeOf<T>();

    public GlobalProperty(GlobalProperties property)
    {
        Property = property;
    }

    public void Init(DevicePipeline pipeline)
    {
        Buffer = pipeline.CreateBuffer(Array.Empty<T>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic, size);
    }

    public void Update(CommandList commandList, T value)
    {
        Value = value;
        commandList.UpdateBuffer(Buffer, 0, value);
    }

    public void Dispose()
    {
        Buffer.Dispose();
    }
}
