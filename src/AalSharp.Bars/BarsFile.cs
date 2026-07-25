using AalSharp.Bars.IO;

namespace AalSharp.Bars;

public class BarsFile : Dictionary<uint, BarsEntry>
{
    public static unsafe BarsFile FromBinary(in ReadOnlySpan<byte> data)
    {
        BarsFile bars = [];

        fixed (byte* ptr = data) {
            BarsSerializer.DeserializeInto(ptr, bars);
        }

        return bars;
    }

    public byte[] ToBinary()
    {
        return BarsSerializer.Serialize(this);
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