using System.Text;

namespace AalSharp;

public ref struct StringView
{
    public ReadOnlySpan<byte> Value;

    public StringView()
    {
    }

    public StringView(ReadOnlySpan<byte> value)
    {
        Value = value;   
    }
    
    public static implicit operator StringView(Span<byte> utf8) => new(utf8);
    
    public static implicit operator StringView(ReadOnlySpan<byte> utf8) => new(utf8);
    
    public static implicit operator StringView(string utf16) => new(Encoding.Unicode.GetBytes(utf16));

    public override string ToString()
    {
        return Encoding.UTF8.GetString(Value);
    }
}