using System.Diagnostics;
using Arbor.Platform.Windows.Native;
using Arbor.Utils;

namespace Arbor.Platform;

public class NativeStorage : Storage
{
    public NativeStorage(string path)
        : base(path)
    {
    }

    public override bool Exists(string path) => File.Exists(GetFullPath(path));

    public override bool ExistsDirectory(string path) => Directory.Exists(GetFullPath(path));

    public override void DeleteDirectory(string path)
    {
        path = GetFullPath(path);

        if (Directory.Exists(path))
            Directory.Delete(path, true);
    }
    
    public override void Delete(string path)
    {
        path = GetFullPath(path);

        if (File.Exists(path))
            File.Delete(path);
    }

    public override void Move(string from, string to)
    {
        General.AttemptWithRetryOnException<IOException>(() => File.Move(GetFullPath(from), GetFullPath(to)));
    }

    public override IEnumerable<string> GetDirectories(string path) => getRelativePaths(Directory.GetDirectories(GetFullPath(path)));

    public override IEnumerable<string> GetFiles(string path, string pattern = "*") => getRelativePaths(Directory.GetFiles(GetFullPath(path), pattern));

    private IEnumerable<string> getRelativePaths(IEnumerable<string> paths)
    {
        var basePath = Path.GetFullPath(GetFullPath(string.Empty));

        return paths.Select(Path.GetFullPath).Select(path =>
        {
            if (!path.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"\"{path}\" does not start with \"{basePath}\" and is probably malformed.");

            return path.AsSpan(basePath.Length).TrimStart(Path.DirectorySeparatorChar).ToString();
        });
    }

    public override string GetFullPath(string path, bool createIfNotExisting = false)
    {
        path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

        var basePath = Path.GetFullPath(BasePath).TrimEnd(Path.DirectorySeparatorChar);
        var resolvedPath = Path.GetFullPath(Path.Combine(basePath, path));

        if (!resolvedPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"\"{resolvedPath}\" traverses outside of \"{basePath}\" and is probably malformed.");

        if (createIfNotExisting)
            Directory.CreateDirectory(Path.GetDirectoryName(resolvedPath)!);

        return resolvedPath;
    }

    public override bool OpenFileExternally(string filename)
    {
        if (Directory.Exists(filename))
        {
            var folder = filename.TrimDirectorySeparator() + Path.DirectorySeparatorChar;
            
            Explorer.OpenFolderAndSelectItem(folder);
            return true;
        }

        openUsingShellExecute(filename);
        return true;
    }

    public override bool PresentFileExternally(string filename)
    {
        Explorer.OpenFolderAndSelectItem(filename.TrimDirectorySeparator());
        return true;
    }

    private void openUsingShellExecute(string path) => Process.Start(new ProcessStartInfo
    {
        FileName = path,
        UseShellExecute = true
    });

    public override Stream? GetStream(string path, FileAccess access = FileAccess.Read, FileMode mode = FileMode.OpenOrCreate)
    {
        path = GetFullPath(path, access != FileAccess.Read);

        if (string.IsNullOrEmpty(path))
            throw new ArgumentNullException(nameof(path));

        switch (access)
        {
            case FileAccess.Read:
                if (!File.Exists(path))
                    return null;

                return File.Open(path, FileMode.Open, access, FileShare.Read);
            
            default:
                if (RuntimeInfo.OS == RuntimeInfo.Platform.Windows)
                    return new FlushingStream(path, mode, access);

                return new FileStream(path, mode, access);
        }
    }
}
