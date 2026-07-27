namespace AalSharp.Bars;

public sealed class BarsMetadata
{
    public ushort Version { get; set; }

    public required BarsMetadataData Data { get; set; }
    
    public required BarsMetadataMarker Marker { get; set; }
    
    public required BarsMetadataExt Ext { get; set; }
}