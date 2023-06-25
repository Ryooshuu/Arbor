using System.Reflection;
using System.Text;
using Arbor.Graphics.Shaders.Uniforms;
using Arbor.Utils;
using Veldrid;

namespace Arbor.Graphics.Shaders;

public abstract class Shader : IShader, IDisposable
{
    public ShaderStages Stage { get; private set; }

    protected abstract ShaderDescription CreateShaderDescription();

    ShaderDescription IShader.CreateShaderDescriptionInternal()
    {
        var properties = CreateShaderDescription();
        Stage = properties.Stage;
        
        return properties;
    }
    
    protected byte[] ReadFromResource(string path, Assembly assembly)
    {
        using var stream = ResourceManager.ReadFromResource($"Shaders/{path}", assembly);
        if (stream == null)
            return Array.Empty<byte>();

        var reader = new StreamReader(stream);
        var source = reader.ReadToEnd();
        var bytes = Encoding.UTF8.GetBytes(GlobalPropertyManager.CreateShaderSource(source));

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
