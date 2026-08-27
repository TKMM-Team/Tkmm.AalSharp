using Tkmm.AalSharp.Bars.IO.Data;
using CommunityToolkit.HighPerformance.Buffers;
using Entish;

namespace Tkmm.AalSharp.Bars.IO;

public static class BarsWriter
{
    public static void Write(AudioResource bars, Stream output, Endianness endianness = Endianness.Little)
    {
        var size = new ResAudioResourceSize(bars);
        using var rented = SpanOwner<byte>.Allocate(size.Total);

        BarsSerializer.Serialize(bars, rented.Span, size, endianness);
        output.Write(rented.Span);
    }

    public static Task WriteAsync(AudioResource bars, Stream output, Endianness endianness = Endianness.Little, CancellationToken cancellationToken = default)
        => Task.Run(() => Write(bars, output, endianness), cancellationToken);
}