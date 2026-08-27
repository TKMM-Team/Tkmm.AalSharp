using Tkmm.AalSharp.Amta;
using Tkmm.AalSharp.Amta.IO;
using Tkmm.AalSharp.Bars;

var outputFile = args[0] + ".out.bars";

{
    var data = File.ReadAllBytes(args[0]);
    var bars = AudioResource.FromBinary<AmtaSerializer>(data, out var endianness);

    var output = args[0] + ".dir";
    Directory.CreateDirectory(output);

    foreach (var (hash, entry) in bars) {
        File.WriteAllBytes(Path.Combine(output, $"{((AalAudioMetadata)entry.Metadata).Data.Name}.amta"), AmtaSerializer.Serialize((AalAudioMetadata)entry.Metadata, endianness));

        if (entry.Asset is not null) {
            File.WriteAllBytes(Path.Combine(output, $"{((AalAudioMetadata)entry.Metadata).Data.Name}.b{entry.Hint?.ToLower() ?? "in"}"), entry.Asset);
        }
    }

    File.WriteAllBytes(outputFile, bars.ToBinary(endianness));
}

{
    var data = File.ReadAllBytes(outputFile);
    var bars = AudioResource.FromBinary<AmtaSerializer>(data, out var endianness);

    var output = outputFile + ".dir";
    Directory.CreateDirectory(output);

    foreach (var (hash, entry) in bars) {
        File.WriteAllBytes(Path.Combine(output, $"{((AalAudioMetadata)entry.Metadata).Data.Name}.amta"), AmtaSerializer.Serialize((AalAudioMetadata)entry.Metadata, endianness));

        if (entry.Asset is not null) {
            File.WriteAllBytes(Path.Combine(output, $"{((AalAudioMetadata)entry.Metadata).Data.Name}.b{entry.Hint?.ToLower() ?? "in"}"), entry.Asset);
        }
    }
}