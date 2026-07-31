using AalSharp.Bars;
using AalSharp.Bars.IO;
using AalSharp.Bars.IO.Data;
using BenchmarkDotNet.Attributes;

namespace AalSharp.Benchmarks;

[MemoryDiagnoser]
public class BarsBenchmarks
{
    private readonly byte[] _buffer = File.ReadAllBytes("path/to/file.bars");
    private readonly byte[] _bufferOut = new byte[0x12075A0];
    
    private readonly BarsFile _file;

    public BarsBenchmarks()
    {
        _file = BarsFile.FromBinary(_buffer);
    }

    [Benchmark]
    public void Read()
    {
        _ = BarsFile.FromBinary(_buffer);
    }

    [Benchmark]
    public void Write()
    {
        using MemoryStream ms = new();
        _file.Write(ms);
    }

    [Benchmark]
    public void ToBinary()
    {
        _ = _file.ToBinary();
    }

    [Benchmark]
    public void SerializeNoAlloc()
    {
        var size = AudioResourcesParts.GetResSize(_file);
        BarsSerializer.Serialize(_file, _bufferOut, size);
    }
}