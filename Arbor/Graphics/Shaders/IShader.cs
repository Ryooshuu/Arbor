using Veldrid;

namespace Arbor.Graphics.Shaders;

public interface IShader
{
    ShaderStages Stage { get; }

    internal ShaderDescription CreateShaderDescriptionInternal();
}
