using AalSharp.Bars;
using AalSharp.Bars.IO;

var outputFile = args[0] + ".out.bars";

{
    var data = File.ReadAllBytes(args[0]);
    var bars = BarsFile.FromBinary(data, out var endianness);

    var output = args[0] + ".dir";
    Directory.CreateDirectory(output);

    foreach (var (hash, entry) in bars) {
        File.WriteAllBytes(Path.Combine(output, $"{entry.Metadata.Data.Name}.amta"), AmtaSerializer.Serialize(entry.Metadata, endianness));
        File.WriteAllBytes(Path.Combine(output, $"{entry.Metadata.Data.Name}.b{entry.Hint?.ToLower() ?? "in"}"), entry.Asset);
    }

    File.WriteAllBytes(outputFile, bars.ToBinary(endianness));
}

{
    var data = File.ReadAllBytes(outputFile);
    var bars = BarsFile.FromBinary(data, out var endianness);

    var output = outputFile + ".dir";
    Directory.CreateDirectory(output);

    foreach (var (hash, entry) in bars) {
        File.WriteAllBytes(Path.Combine(output, $"{entry.Metadata.Data.Name}.amta"), AmtaSerializer.Serialize(entry.Metadata, endianness));
        File.WriteAllBytes(Path.Combine(output, $"{entry.Metadata.Data.Name}.b{entry.Hint?.ToLower() ?? "in"}"), entry.Asset);
    }
}