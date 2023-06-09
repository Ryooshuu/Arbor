using System.ComponentModel;
using System.Reflection;
using Arbor.Graphics.Shaders.Uniforms;

namespace Arbor.Utils;

public static class ExtensionMethods
{
    public static string GetDescription(this object value)
    {
        if (value is string description)
            return description;

        var type = value as Type ?? value.GetType();
        return (type.GetField(value.ToString()!)?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? value.ToString())!;
    }

    public static string GetUniformName(this object value)
    {
        var type = value as Type ?? value.GetType();
        return (type.GetField(value.ToString()!)?.GetCustomAttribute<UniformAttribute>()?.Name!);
    }

    public static string GetUniformType(this object value)
    {
        var type = value as Type ?? value.GetType();
        return (type.GetField(value.ToString()!)?.GetCustomAttribute<UniformAttribute>()?.Type!);
    }
}
