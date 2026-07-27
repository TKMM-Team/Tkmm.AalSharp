namespace AalSharp.Bars;

public sealed class BarsMetadataMarker : List<BarsMetadataMarkerEntry>
{
    public BarsMetadataMarker()
    {
    }

    public BarsMetadataMarker(int capacity) : base(capacity)
    {
    }
}