using AalSharp.Bars;
using AalSharp.Bars.IO;

var data = File.ReadAllBytes(args[0]);
var bars = BarsFile.FromBinary(data);

var output = args[0] + ".dir";
Directory.CreateDirectory(output);

foreach (var (hash, entry) in bars) {
    File.WriteAllBytes(Path.Combine(output, $"0x{hash:x8}.amta"), AmtaSerializer.Serialize(entry.Metadata));
    File.WriteAllBytes(Path.Combine(output, $"0x{hash:x8}.b{entry.Hint?.ToLower() ?? "in"}"), entry.Asset);
}

var outputFile = args[0] + ".out.bars";
File.WriteAllBytes(outputFile, bars.ToBinary());