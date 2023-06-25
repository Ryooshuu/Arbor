using Veldrid;

namespace Arbor.Graphics.Shaders;

public interface IBindableShader : IShader
{
    ResourceLayoutElementDescription[] CreateResourceDescriptions();
    
    BindableResource[] CreateBindableResources();
}
