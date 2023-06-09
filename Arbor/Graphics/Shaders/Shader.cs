using System.Reflection;
using System.Text;
using Arbor.Utils;
using Veldrid;

namespace Arbor.Graphics.Shaders;

public abstract class Shader : IDisposable
{
    public abstract ShaderStages Stage { get; }

    public abstract ShaderDescription CreateShaderDescription();

    public virtual VertexElementDescription[] CreateVertexDescriptions()
        => Array.Empty<VertexElementDescription>();

    public virtual ResourceLayoutElementDescription[] CreateResourceDescriptions()
        => Array.Empty<ResourceLayoutElementDescription>();

    public virtual BindableResource[] CreateBindableResources()
        => Array.Empty<BindableResource>();

    protected byte[] ReadFromResource(string path, Assembly assembly)
    {
        using var stream = ResourceManager.ReadFromResource($"Shaders/{path}", assembly);
        if (stream == null)
            return Array.Empty<byte>();

        var reader = new StreamReader(stream);
        var source = reader.ReadToEnd();

        const string initial_code = @"
#version 450

layout (set=0, binding=0) uniform g_GlobalProperties {
    mat4 g_PixelMatrix;
};";
        var bytes = Encoding.UTF8.GetBytes(initial_code + "\n" + source);

        return bytes;
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
