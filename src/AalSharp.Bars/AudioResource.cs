using AalSharp.Amta.IO;
using AalSharp.Bars.IO;
using AalSharp.Hashing;
using Entish;

namespace AalSharp.Bars;

public class AudioResource : Dictionary<uint, AudioResourceAsset>
{
    public static AudioResource FromBinary<TAmtaSerializer>(in ReadOnlySpan<byte> data)
        where TAmtaSerializer : IAmtaSerializer
        => FromBinary<TAmtaSerializer>(data, out _);

    public static unsafe AudioResource FromBinary<TAmtaSerializer>(in ReadOnlySpan<byte> data, out Endianness endianness)
        where TAmtaSerializer : IAmtaSerializer
    {
        fixed (byte* ptr = data) {
            return BarsSerializer.Deserialize<TAmtaSerializer>(ptr, out endianness);
        }
    }

    public AudioResourceAsset this[string key] {
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