namespace AalSharp.Bars.IO;

public sealed class BarsWriter
{
    private readonly Stream _output;
    private readonly byte[]? _serialized;
    private readonly Dictionary<long, long> _pointers;
    
    public BarsWriter(BarsFile bars, Stream output)
    {
        _output = output;
        
        if (!output.CanSeek) {
            _serialized = BarsSerializer.Serialize(bars);
            _pointers = null!;
            return;
        }

        _pointers = [];
    }

    public void Write()
    {
        if (_serialized != null) {
            _output.Write(_serialized, 0, _serialized.Length);
            return;
        }
        
        throw new NotImplementedException();
    }

    public Task WriteAsync(CancellationToken cancellationToken = default)
    {
        if (_serialized != null) {
            return _output.WriteAsync(_serialized, 0, _serialized.Length, cancellationToken: cancellationToken);
        }

        throw new NotImplementedException();
    }
}