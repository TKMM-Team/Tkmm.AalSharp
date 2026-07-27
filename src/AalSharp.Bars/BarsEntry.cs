namespace AalSharp.Bars;

public sealed class BarsEntry
{
    public string? Hint { get; set; }
    
    public required BarsMetadata Metadata { get; set; }
    
    public required byte[] Asset { get; set; }
}