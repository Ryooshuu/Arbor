namespace Arbor.Graphics.Shaders.Uniforms;

[AttributeUsage(AttributeTargets.Field)]
public class UniformAttribute : Attribute
{
    public readonly string Name;
    public readonly string Type;

    public UniformAttribute(string name, string type)
    {
        Name = name;
        Type = type;
    }
}
