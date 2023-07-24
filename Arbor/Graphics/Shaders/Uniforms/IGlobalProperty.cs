namespace Arbor.Graphics.Shaders.Uniforms;

public interface IGlobalProperty
{
    object Value { get; }
    uint Size { get; }
    GlobalProperties Property { get; }
    
    byte[] GetBytes();
}
