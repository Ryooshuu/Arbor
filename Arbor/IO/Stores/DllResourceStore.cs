using System.Reflection;
using Arbor.Utils;

namespace Arbor.IO.Stores;

public class DllResourceStore : IResourceStore<byte[]>
{
    private readonly Assembly assembly;
    private readonly string prefix;

    public DllResourceStore(string dllName)
    {
        var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location)!, dllName);

        assembly = File.Exists(filePath) ? Assembly.LoadFrom(filePath) : Assembly.Load(Path.GetFileNameWithoutExtension(dllName));
        prefix = Path.GetFileNameWithoutExtension(dllName);
    }

    public DllResourceStore(AssemblyName name)
        : this(Assembly.Load(name))
    {
    }

    public DllResourceStore(Assembly assembly)
    {
        this.assembly = assembly;
        prefix = assembly.GetName().Name!;
    }

    public byte[]? Get(string name)
    {
        using (var input = GetStream(name))
            return input?.ReadAllBytesToArray();
    }

    public virtual async Task<byte[]?> GetAsync(string name, CancellationToken cancellationToken = default)
    {
        using (var input = GetStream(name))
        {
            if (input == null)
                return null;

            return await input.ReadAllBytesToArrayAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public IEnumerable<string> GetAvailableResources()
        => assembly.GetManifestResourceNames().Select(n =>
        {
            n = n.Substring(n.StartsWith(prefix, StringComparison.Ordinal) ? prefix.Length + 1 : 0);

            var lastDot = n.LastIndexOf('.');
            var chars = n.ToCharArray();

            for (int i = 0; i < lastDot; i++)
            {
                if (chars[i] == '.')
                    chars[i] = '/';
            }

            return new string(chars);
        });

    public Stream? GetStream(string name)
    {
        var split = name.Split('/');
        for (int i = 0; i < split.Length - 1; i++)
            split[i] = split[i].Replace('-', '_');

        return assembly?.GetManifestResourceStream($@"{prefix}.{string.Join('.', split)}");
    }

    #region IDisposable Support

    public void Dispose()
    {
    }

    #endregion
}
