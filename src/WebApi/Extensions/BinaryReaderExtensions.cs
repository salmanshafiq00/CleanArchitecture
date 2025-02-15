namespace WebApi.Extensions;

public static class BinaryReaderExtensions
{
    public static async Task<byte[]> ReadBytesAsync(this BinaryReader reader, int count)
    {
        var buffer = new byte[count];
        var totalBytesRead = 0;

        while (totalBytesRead < count)
        {
            var bytesRead = await reader.BaseStream.ReadAsync(buffer.AsMemory(totalBytesRead, count - totalBytesRead));
            if (bytesRead == 0) break;  // End of stream
            totalBytesRead += bytesRead;
        }

        return buffer;
    }
}
