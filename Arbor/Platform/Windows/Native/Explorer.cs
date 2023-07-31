using System.Runtime.InteropServices;

namespace Arbor.Platform.Windows.Native;

public class Explorer
{
    [DllImport("shell32.dll", SetLastError = true)]
    private static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, uint dwFlags);

    [DllImport("shell32.dll", SetLastError = true)]
    private static extern int SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr bindingContext, [Out] out IntPtr pidl, uint sfgaoIn,
                                                  [Out] out uint psfgaoOut);

    internal static void OpenFolderAndSelectItem(string filename)
    {
        Task.Run(() =>
        {
            var nativeFile = IntPtr.Zero;
            var nativeFolder = IntPtr.Zero;

            try
            {
                filename = filename.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

                var folderPath = Path.GetDirectoryName(filename);

                if (folderPath == null)
                {
                    Console.WriteLine($"Failed to get directory for {filename}");
                    return;
                }
                
                SHParseDisplayName(folderPath, IntPtr.Zero, out nativeFolder, 0, out _);

                if (nativeFolder == IntPtr.Zero)
                {
                    Console.WriteLine($"Cannot find native folder for '{folderPath}'");
                    return;
                }
                
                SHParseDisplayName(filename, IntPtr.Zero, out nativeFile, 0, out _);
                
                IntPtr[] fileArray;

                if (nativeFile != IntPtr.Zero)
                {
                    fileArray = new[] { nativeFile };
                }
                else
                {
                    Console.WriteLine($"Cannot find native file for '{filename}'");
                    fileArray = new[] { nativeFolder };
                }

                SHOpenFolderAndSelectItems(nativeFolder, (uint) fileArray.Length, fileArray, 0);
            }
            finally
            {
                if (nativeFolder != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(nativeFolder);
                
                if (nativeFile != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(nativeFile);
            }
        });
    }
}
