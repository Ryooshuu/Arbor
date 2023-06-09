using System.Diagnostics;
using System.Reflection;

namespace Arbor.Utils;

public static class ResourceManager
{
    public static Stream? ReadFromResource(string path, Assembly assembly)
    {
        var truePath = path.Replace('/', '.');
        var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{truePath}");
        
        Debug.Assert(stream != null, nameof(stream) + " != null");
        return stream;
    }
}
