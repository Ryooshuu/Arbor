using System.ComponentModel;
using Arbor.Utils;
using GlmSharp;
using Veldrid;

namespace Arbor.Graphics.Shaders.Uniforms;

public static class GlobalPropertyManager
{
    private static readonly List<ResourceLayoutElementDescription> element_descriptions;
    private static readonly IGlobalProperty[] global_properties;

    public static ResourceLayout GlobalResourceLayout { get; private set; } = null!;
    public static ResourceSet GlobalResourceSet { get; private set; } = null!;

    static GlobalPropertyManager()
    {
        element_descriptions = new List<ResourceLayoutElementDescription>();
        global_properties = new IGlobalProperty[]
        {
            new GlobalProperty<mat4>(GlobalProperties.PixelMatrix)
        };
    }

    internal static void Init(GraphicsPipeline pipeline)
    {
        foreach (var property in global_properties)
        {
            element_descriptions.Add(
                new ResourceLayoutElementDescription(
                    property.Property.GetDescription(), ResourceKind.UniformBuffer, ShaderStages.Vertex)
            );
            
            property.Init(pipeline);
        }

        GlobalResourceLayout = pipeline.CreateResourceLayout(element_descriptions.ToArray());
        GlobalResourceSet = pipeline.CreateResourceSet(GlobalResourceLayout, global_properties.Select(p => p.Buffer).ToArray());
    }

    public static void Set<T>(CommandList cl, GlobalProperties property, T value)
        where T : unmanaged
    {
        ((GlobalProperty<T>)global_properties.First(p => p.Property == property)).Update(cl, value);
    }

    public static T Get<T>(GlobalProperties property)
    {
        return (T) global_properties.First(p => p.Property == property).Value;
    }

    public static void Dispose()
    {
        foreach (var property in global_properties)
        {
            property.Dispose();
        }
    }
}

public enum GlobalProperties
{
    [Description("g_PixelMatrix")]
    PixelMatrix
}
