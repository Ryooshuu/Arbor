using System.Text;
using Arbor.Utils;
using GlmSharp;
using Veldrid;

namespace Arbor.Graphics.Shaders.Uniforms;

public static class GlobalPropertyManager
{
    private static readonly List<ResourceLayoutElementDescription> element_descriptions;
    private static readonly IGlobalProperty[] global_properties;
    private static DeviceBuffer buffer = null!;

    public static ResourceLayout GlobalResourceLayout { get; private set; } = null!;
    public static ResourceSet GlobalResourceSet { get; private set; } = null!;

    static GlobalPropertyManager()
    {
        element_descriptions = new List<ResourceLayoutElementDescription>();
        global_properties = new IGlobalProperty[]
        {
            new GlobalProperty<mat4>(GlobalProperties.PixelMatrix),
            new GlobalProperty<mat4>(GlobalProperties.ModelMatrix),
        };
    }

    public static void Set<T>(CommandList cl, GlobalProperties property, T value)
        where T : unmanaged
    {
        ((GlobalProperty<T>) global_properties.First(p => p.Property == property)).Update(value);
        updateBuffer(cl);
    }

    public static T Get<T>(GlobalProperties property)
    {
        return (T) global_properties.First(p => p.Property == property).Value;
    }

    internal static string CreateShaderSource()
    {
        var sb = new StringBuilder();
        sb.AppendLine("#version 450");
        sb.AppendLine("layout (set=0, binding=0) uniform g_GlobalProperties {");
        foreach (var p in global_properties)
            sb.AppendLine("    " + p.Property.GetUniformType() + " " + p.Property.GetUniformName() + ";");
        sb.AppendLine("};");

        return sb + "\n";
    }

    internal static void Init(DevicePipeline pipeline)
    {
        var size = global_properties.Aggregate<IGlobalProperty, uint>(0, (current, property) => current + property.Size);
        buffer = pipeline.CreateBuffer(BufferUsage.UniformBuffer | BufferUsage.Dynamic, size);

        GlobalResourceLayout = pipeline.CreateResourceLayout(
            new ResourceLayoutElementDescription(
                "g_GlobalProperties", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment
            )
        );
        GlobalResourceSet = pipeline.CreateResourceSet(GlobalResourceLayout, buffer);
    }

    private static void updateBuffer(CommandList cl)
    {
        var size = global_properties.Aggregate<IGlobalProperty, uint>(0, (current, property) => current + property.Size);
        var bytes = new byte[size];
        
        var offset = 0u;
        foreach (var property in global_properties)
        {
            var propertyBytes = property.GetBytes();
            Array.Copy(propertyBytes, 0, bytes, offset, propertyBytes.Length);
            offset += property.Size;
        }
        
        cl.UpdateBuffer(buffer, 0, bytes);
    }

    public static void Dispose()
    {
        buffer.Dispose();
    }
}

public enum GlobalProperties
{
    [Uniform("g_PixelMatrix", "mat4")]
    PixelMatrix,

    [Uniform("g_ModelMatrix", "mat4")]
    ModelMatrix
}
