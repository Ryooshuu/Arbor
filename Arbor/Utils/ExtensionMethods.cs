using System.ComponentModel;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Arbor.Graphics.Shaders.Uniforms;

namespace Arbor.Utils;

public static class ExtensionMethods
{
    public static string GetDescription(this object value)
    {
        if (value is string description)
            return description;

        var type = value as Type ?? value.GetType();
        return (type.GetField(value.ToString()!)?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? value.ToString())!;
    }

    public static string GetUniformName(this object value)
    {
        var type = value as Type ?? value.GetType();
        return type.GetField(value.ToString()!)?.GetCustomAttribute<UniformAttribute>()?.Name!;
    }

    public static string GetUniformType(this object value)
    {
        var type = value as Type ?? value.GetType();
        return type.GetField(value.ToString()!)?.GetCustomAttribute<UniformAttribute>()?.Type!;
    }
    
    public static string TrimDirectorySeparator(this string path)
        => path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    private static string toLowercaseHex(this byte[] bytes)
        => string.Create(bytes.Length * 2, bytes, (span, b) =>
        {
            for (var i = 0; i < b.Length; i++)
                _ = b[i].TryFormat(span[(i * 2)..], out _, "x2");
        });
    
    public static string ComputeSHA2Hash(this Stream stream)
    {
        string hash;

        stream.Seek(0, SeekOrigin.Begin);

        using (var alg = SHA256.Create())
            hash = alg.ComputeHash(stream).toLowercaseHex();

        stream.Seek(0, SeekOrigin.Begin);
        
        return hash;
    }

    public static string ComputeSHA2Hash(this string str)
        => SHA256.HashData(Encoding.UTF8.GetBytes(str)).toLowercaseHex();

    public static string ComputeMD5Hash(this Stream stream)
    {
        string hash;

        stream.Seek(0, SeekOrigin.Begin);
        using (var md5 = MD5.Create())
            hash = md5.ComputeHash(stream).toLowercaseHex();
        stream.Seek(0, SeekOrigin.Begin);
        
        return hash;
    }

    public static string ComputeMD5Hash(this string input)
        => MD5.HashData(Encoding.UTF8.GetBytes(input)).toLowercaseHex();
}
