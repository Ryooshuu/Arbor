using System.Runtime.InteropServices;

namespace Arbor.Graphics.Shaders.Uniforms;

public class GlobalProperty<T> : IGlobalProperty
    where T : unmanaged
{
    object IGlobalProperty.Value => Value;
    public T Value { get; private set; }
    public GlobalProperties Property { get; }

    public uint Size => (uint) Marshal.SizeOf<T>();

    public GlobalProperty(GlobalProperties property)
    {
        Property = property;
    }

    internal void Update(T value)
    {
        Value = value;
    }

    public byte[] GetBytes()
    {
        var bytes = new byte[Size];
        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

        Marshal.StructureToPtr(Value, handle.AddrOfPinnedObject(), true);
        handle.Free();

        return bytes;
    }
}
