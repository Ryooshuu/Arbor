using Veldrid;

namespace Arbor.Graphics.Shaders;

public abstract class FragmentShader : Shader, IBindableShader
{
    public virtual ResourceLayoutElementDescription[] CreateResourceDescriptions()
        => Array.Empty<ResourceLayoutElementDescription>();

    public virtual BindableResource[] CreateBindableResources()
        => Array.Empty<BindableResource>();
}
