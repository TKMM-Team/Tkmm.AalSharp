using Tkmm.AalSharp.Bars;

var outputFile = args[0] + ".out.bars";

{
    var data = File.ReadAllBytes(args[0]);
    var bars = AudioResource.FromBinary(data, out var endianness);

    var output = args[0] + ".dir";
    Directory.CreateDirectory(output);

    foreach (var (hash, entry) in bars) {
        File.WriteAllBytes(Path.Combine(output, $"{hash:X8}.amta"), entry.Metadata);

        if (entry.Asset is not null) {
            File.WriteAllBytes(Path.Combine(output, $"{hash:X8}.{entry.Hint?.ToLower() ?? "bin"}"), entry.Asset);
        }
    }

    File.WriteAllBytes(outputFile, bars.ToBinary(endianness));
}

{
    var data = File.ReadAllBytes(outputFile);
    var bars = AudioResource.FromBinary(data, out var endianness);

    var output = outputFile + ".dir";
    Directory.CreateDirectory(output);

    foreach (var (hash, entry) in bars) {
        File.WriteAllBytes(Path.Combine(output, $"{hash:X8}.amta"), entry.Metadata);

        if (entry.Asset is not null) {
            File.WriteAllBytes(Path.Combine(output, $"{hash:X8}.{entry.Hint?.ToLower() ?? "bin"}"), entry.Asset);
        }
    }
}