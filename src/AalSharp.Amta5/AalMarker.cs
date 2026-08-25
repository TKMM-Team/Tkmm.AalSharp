namespace AalSharp.Amta;

public sealed class AalMarker
{
    public uint Id { get; set; }

    public int Start { get; set; }

    public int Duration { get; set; }

    public string? Name { get; set; }
}