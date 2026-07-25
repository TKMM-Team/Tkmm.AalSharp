namespace AalSharp.Bars;

public sealed class BarsEntry
{
    public string? Hint { get; set; }
    
    public required byte[] Metadata { get; set; }
    
    public required byte[] Asset { get; set; }
}