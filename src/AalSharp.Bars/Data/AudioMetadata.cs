using AalSharp.Helpers;
using Entish;
using Entish.Attributes;

namespace AalSharp.Bars.Data;

[Swappable]
public unsafe partial struct AudioMetadata : IMemoryResource<AudioMetadata>
{
    public const uint Magic = 0x41544D41;

    public AudioMetadataHeader Header;
    public Offset<AudioMetadataData> DataOffset;
    public Offset<AudioMetadataMarker> MarkerOffset;
    public Offset<AudioMetadataExt> ExtOffset;
    public Offset<AudioMetadataStringTable> StringTableOffset;

    public StringView Name {
        get {
            ref var data = ref DataOffset.Get(ref this);
            return GetString(data.NameOffset);
        }
    }

    public StringView GetString(int offset)
    {
        ref var stringTable = ref StringTableOffset.Get(ref this);
        ref int nameLength = ref MemUtils.GetRelativeTo<int, AudioMetadataStringTable>(stringTable,
            sizeof(AudioMetadataStringTable) + offset);
#pragma warning disable CS9082 // Local is returned by reference but was initialized to a value that cannot be returned by reference
        return MemUtils.GetSpanRelativeTo<byte, AudioMetadataStringTable>(stringTable,
#pragma warning restore CS9082 // Local is returned by reference but was initialized to a value that cannot be returned by reference
            // The stored string length includes the null terminator
            sizeof(AudioMetadataStringTable) + sizeof(uint) + offset, nameLength - 1);
    }

    /// <summary>
    /// Checks the metadata and returns a nullptr if the input is invalid. 
    /// </summary>
    /// <param name="metadata"></param>
    /// <returns></returns>
    public static IEnumerable<Exception> GetErrors(AudioMetadata metadata)
    {
        if (metadata.Header.Magic != Magic) {
            yield return new InvalidDataException(
                $"Invalid audio metadata magic: '{PrimitivesHelper.ToAscii(metadata.Header.Magic)}'");
        }

        if (metadata.Header.Version != 0x0400) {
            yield return new InvalidDataException(
                "Only audio metadata (AMTA) version 4 is supported");
        }

        if (metadata.DataOffset.Get(ref metadata).Magic is var dataMagic and not AudioMetadataData.AmtaDataMagic) {
            yield return new InvalidDataException(
                $"Invalid audio metadata data section magic: '{PrimitivesHelper.ToAscii(dataMagic)}'");
        }

        if (metadata.MarkerOffset.Get(ref metadata).Magic is var markMagic and not AudioMetadataMarker.AmtaMarkMagic) {
            yield return new InvalidDataException(
                $"Invalid audio metadata mark section magic: '{PrimitivesHelper.ToAscii(markMagic)}'");
        }

        if (metadata.ExtOffset.Get(ref metadata).Magic is var extMagic and not AudioMetadataExt.AmtaExtMagic) {
            yield return new InvalidDataException(
                $"Invalid audio metadata ext section magic: '{PrimitivesHelper.ToAscii(extMagic)}'");
        }

        if (metadata.StringTableOffset.Get(ref metadata).Magic is var stringTableMagic and not AudioMetadataStringTable.AmtaStringTableMagic) {
            yield return new InvalidDataException(
                $"Invalid audio metadata string table magic: '{PrimitivesHelper.ToAscii(stringTableMagic)}'");
        }
    }
}