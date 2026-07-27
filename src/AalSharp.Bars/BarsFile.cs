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

    public void Write(Stream output)
    {
        BarsWriter writer = new(this, output);
        writer.Write();
    }

    public Task WriteAsync(Stream output, CancellationToken cancellationToken = default)
    {
        BarsWriter writer = new(this, output);
        return writer.WriteAsync(cancellationToken);
    }
}