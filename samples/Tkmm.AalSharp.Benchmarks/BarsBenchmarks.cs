using BenchmarkDotNet.Attributes;
using Tkmm.AalSharp.Bars;
using Tkmm.AalSharp.Bars.IO;
using Tkmm.AalSharp.Bars.IO.Data;

namespace Tkmm.AalSharp.Benchmarks;

[MemoryDiagnoser]
public class BarsBenchmarks
{
    private readonly byte[] _buffer = File.ReadAllBytes("path/to/file.bars");
    private readonly byte[] _bufferOut = new byte[0x12075A0];
    
    private readonly AudioResource _file;

    public BarsBenchmarks()
    {
        _file = AudioResource.FromBinary(_buffer);
    }

    [Benchmark]
    public void Read()
    {
        _ = AudioResource.FromBinary(_buffer);
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
        var size = new ResAudioResourceSize(_file);
        BarsSerializer.Serialize(_file, _bufferOut, size);
    }
}