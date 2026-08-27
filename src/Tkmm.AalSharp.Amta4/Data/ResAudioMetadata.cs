using Tkmm.AalSharp.Helpers;
using Entish;
using Entish.Attributes;

namespace Tkmm.AalSharp.Amta.Data;

[Swappable]
public unsafe partial struct ResAudioMetadata() : IMemoryResource<ResAudioMetadata>
{
    public const uint AmtaMagic = 0x41544D41;
    public const uint AmtaAttributesMagic = 0x5F545845;
    public const uint AmtaMarkerMagic = 0x4B52414D;

    [NeverSwap]
    public readonly uint Magic = AmtaMagic;
    public Endianness Endianness;
    public ushort Version = 0x400;
    public int FileSize;
    public Offset<ResData> DataOffset;
    public Offset<ResContainer> MarkerOffset;
    public Offset<ResContainer> ExtOffset;
    public Offset<ResStringTable> StringTableOffset;

    public StringView Name {
        get {
            ref var data = ref DataOffset.Get(ref this);
            return GetString(data.NameOffset);
        }
    }

    public StringView GetString(int offset)
    {
        ref var stringTable = ref StringTableOffset.Get(ref this);
        ref int nameLength = ref MemUtils.GetRelativeTo<int, ResStringTable>(stringTable,
            sizeof(ResStringTable) + offset);
#pragma warning disable CS9082 // Local is returned by reference but was initialized to a value that cannot be returned by reference
        return MemUtils.GetSpanRelativeTo<byte, ResStringTable>(stringTable,
#pragma warning restore CS9082 // Local is returned by reference but was initialized to a value that cannot be returned by reference
            // The stored string length includes the null terminator
            sizeof(ResStringTable) + sizeof(uint) + offset, nameLength - 1);
    }

    /// <summary>
    /// Checks the metadata and returns a nullptr if the input is invalid. 
    /// </summary>
    /// <param name="metadata"></param>
    /// <returns></returns>
    public static IEnumerable<Exception> GetErrors(ResAudioMetadata metadata)
    {
        if (metadata.Magic != AmtaMagic) {
            yield return new InvalidDataException(
                $"Invalid audio metadata magic: '{PrimitivesHelper.ToAscii(metadata.Magic)}'");
        }

        if (metadata.Version != 0x0400) {
            yield return new InvalidDataException(
                "Only audio metadata (AMTA) version 4 is supported");
        }

        if (metadata.DataOffset.Get(ref metadata).Magic is var dataMagic and not ResData.AmtaDataMagic) {
            yield return new InvalidDataException(
                $"Invalid audio metadata data section magic: '{PrimitivesHelper.ToAscii(dataMagic)}'");
        }

        if (metadata.MarkerOffset.Get(ref metadata).Magic is var markMagic and not AmtaMarkerMagic) {
            yield return new InvalidDataException(
                $"Invalid audio metadata marker section magic: '{PrimitivesHelper.ToAscii(markMagic)}'");
        }

        if (metadata.ExtOffset.Get(ref metadata).Magic is var extMagic and not AmtaAttributesMagic) {
            yield return new InvalidDataException(
                $"Invalid audio metadata ext (attributes) section magic: '{PrimitivesHelper.ToAscii(extMagic)}'");
        }

        if (metadata.StringTableOffset.Get(ref metadata).Magic is var stringTableMagic and not ResStringTable.AmtaStringTableMagic) {
            yield return new InvalidDataException(
                $"Invalid audio metadata string table magic: '{PrimitivesHelper.ToAscii(stringTableMagic)}'");
        }
    }
}