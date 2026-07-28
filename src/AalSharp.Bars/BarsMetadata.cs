namespace AalSharp.Bars;

public sealed class BarsMetadata
{
    public required BarsMetadataData Data { get; set; }
    
    public required BarsMetadataMarker Marker { get; set; }
    
    public required BarsMetadataExt Ext { get; set; }
}