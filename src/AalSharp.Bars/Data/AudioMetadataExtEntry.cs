using Entish.Attributes;

namespace AalSharp.Bars.Data;

[Swappable]
public unsafe partial struct AudioMetadataExtEntry
{
    public fixed uint Unknown[2];
};