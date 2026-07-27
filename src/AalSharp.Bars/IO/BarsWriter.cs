using AalSharp.Bars.IO.Data;
using CommunityToolkit.HighPerformance.Buffers;
using Entish;

namespace AalSharp.Bars.IO;

public static class BarsWriter
{
    public static void Write(BarsFile bars, Stream output, Endianness endianness = Endianness.Little)
    {
        var size = AudioResourcesParts.GetResSize(bars);
        using var rented = SpanOwner<byte>.Allocate(size.Total);

        BarsSerializer.Serialize(bars, rented.Span, size, endianness);
        output.Write(rented.Span);
    }

    public static Task WriteAsync(BarsFile bars, Stream output, Endianness endianness = Endianness.Little, CancellationToken cancellationToken = default)
        => Task.Run(() => Write(bars, output, endianness), cancellationToken);
}