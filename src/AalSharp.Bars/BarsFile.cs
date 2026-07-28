using AalSharp.Bars.IO;
using AalSharp.Hashing;
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

    public BarsEntry this[string key] {
        get => this[Crc32.HashToUInt(key)];
        set => this[Crc32.HashToUInt(key)] = value;
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