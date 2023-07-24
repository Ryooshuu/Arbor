using System.Text;
using Arbor.Graphics.Shaders.Uniforms;
using Arbor.IO.Stores;
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
    
    protected byte[] ReadFromResource(IResourceStore<byte[]> resource, string name)
    {
        var bytes = resource.Get(name);
        if (bytes == null)
            return Array.Empty<byte>();

        var header = Encoding.UTF8.GetBytes(GlobalPropertyManager.CreateShaderSource());
        
        var bytes2 = new byte[header.Length + bytes.Length];
        Array.Copy(header, bytes2, header.Length);
        Array.Copy(bytes, 0, bytes2, header.Length, bytes.Length);
        
        return bytes2;
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
