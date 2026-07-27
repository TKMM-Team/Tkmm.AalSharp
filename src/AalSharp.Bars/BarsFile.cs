using AalSharp.Bars.IO;
using Entish;

namespace AalSharp.Bars;

public class BarsFile : Dictionary<uint, BarsEntry>
{
    public static unsafe BarsFile FromBinary(in ReadOnlySpan<byte> data)
    {
        fixed (byte* ptr = data) {
            return BarsSerializer.Deserialize(ptr);
        }
    }

    public byte[] ToBinary(Endianness endianness = Endianness.Little)
    {
        return BarsSerializer.Serialize(this, endianness);
    }

    public void Write(Stream output, Endianness endianness = Endianness.Little)
        => BarsWriter.Write(this, output, endianness);

    public Task WriteAsync(Stream output, Endianness endianness = Endianness.Little, CancellationToken cancellationToken = default)
        => BarsWriter.WriteAsync(this, output, endianness, cancellationToken);
}