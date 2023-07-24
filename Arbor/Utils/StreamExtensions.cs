namespace Arbor.Utils;

public static class StreamExtensions
{
    public static byte[] ReadAllBytesToArray(this Stream stream)
    {
        if (!stream.CanSeek)
            throw new ArgumentException($"Stream must be seekable to use this function.", nameof(stream));

        if (stream.Length >= Array.MaxLength)
            throw new ArgumentException($"The stream is too long for an array.", nameof(stream));

        stream.Seek(0, SeekOrigin.Begin);
        return stream.ReadBytesToArray((int) stream.Length);
    }

    public static Task<byte[]> ReadAllBytesToArrayAsync(this Stream stream, CancellationToken cancellationToken = default)
    {
        if (!stream.CanSeek)
            throw new ArgumentException($"Stream must be seekable to use this function.", nameof(stream));
        
        if (stream.Length >= Array.MaxLength)
            throw new ArgumentException($"The stream is too long for an array.", nameof(stream));
        
        stream.Seek(0, SeekOrigin.Begin);
        return stream.ReadBytesToArrayAsync((int) stream.Length, cancellationToken);
    }

    public static byte[] ReadBytesToArray(this Stream stream, int length)
    {
        var bytes = new byte[length];
        stream.ReadToFill(bytes);
        return bytes;
    }

    public static async Task<byte[]> ReadBytesToArrayAsync(this Stream stream, int length, CancellationToken cancellationToken = default)
    {
        var bytes = new byte[length];
        await stream.ReadToFillAsync(bytes, cancellationToken).ConfigureAwait(false);
        return bytes;
    }

    public static void ReadToFill(this Stream stream, Span<byte> buffer)
    {
        var remainingBuffer = buffer;

        while (!remainingBuffer.IsEmpty)
        {
            var bytesRead = stream.Read(remainingBuffer);
            remainingBuffer = remainingBuffer[bytesRead..];

            if (bytesRead == 0)
                throw new EndOfStreamException();
        }
    }

    public static async Task ReadToFillAsync(this Stream stream, Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var remainingBuffer = buffer;

        while (!remainingBuffer.IsEmpty)
        {
            var bytesRead = await stream.ReadAsync(remainingBuffer, cancellationToken).ConfigureAwait(false);
            remainingBuffer = remainingBuffer[bytesRead..];

            if (bytesRead == 0)
                throw new EndOfStreamException();
        }
    }
}
