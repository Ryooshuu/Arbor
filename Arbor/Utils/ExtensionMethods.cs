using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Arbor.Graphics.Shaders.Uniforms;

namespace Arbor.Utils;

public static class ExtensionMethods
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool HasFlagFast<T>(this T enumValue, T flag) where T : unmanaged, Enum
    {
        // Note: Using a switch statement would eliminate inlining.

        if (sizeof(T) == 1)
        {
            byte value1 = Unsafe.As<T, byte>(ref enumValue);
            byte value2 = Unsafe.As<T, byte>(ref flag);
            return (value1 & value2) == value2;
        }

        if (sizeof(T) == 2)
        {
            short value1 = Unsafe.As<T, short>(ref enumValue);
            short value2 = Unsafe.As<T, short>(ref flag);
            return (value1 & value2) == value2;
        }

        if (sizeof(T) == 4)
        {
            int value1 = Unsafe.As<T, int>(ref enumValue);
            int value2 = Unsafe.As<T, int>(ref flag);
            return (value1 & value2) == value2;
        }

        if (sizeof(T) == 8)
        {
            long value1 = Unsafe.As<T, long>(ref enumValue);
            long value2 = Unsafe.As<T, long>(ref flag);
            return (value1 & value2) == value2;
        }

        throw new ArgumentException($"Invalid enum type provided: {typeof(T)}.");
    }
    
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
