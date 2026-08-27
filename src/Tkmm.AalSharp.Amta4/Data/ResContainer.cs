using Entish.Attributes;

namespace Tkmm.AalSharp.Amta.Data;

[Swappable]
public partial struct ResContainer
{
    [NeverSwap]
    public uint Magic;
    public int SectionSize;
    public int NumEntries;
}