using System.Reflection;
using Arbor.Graphics.Shaders.Vertices;
using Veldrid;

namespace Arbor.Graphics.Shaders;

public abstract class VertexShader<T> : Shader, IVertexShader
    where T : struct
{
    public virtual VertexElementDescription[] CreateVertexDescriptions()
    {
        var type = typeof(T);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        
        var descriptions = new List<VertexElementDescription>();
        foreach (var field in fields)
        {
            var attribute = field.GetCustomAttribute<VertexDescriptionAttribute>();
            if (attribute == null)
                continue;

            var name = attribute.Name;
            var format = attribute.Format;
            var semantic = attribute.Semantic;
            var offset = attribute.Offset;

            var description = new VertexElementDescription(name, semantic, format, offset);
            descriptions.Add(description);
        }
        
        return descriptions.ToArray();
    }
}
