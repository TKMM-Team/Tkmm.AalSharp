using System.Runtime.CompilerServices;
using Tkmm.AalSharp.Hashing;
using Tkmm.AalSharp.Helpers;
using Entish;
using Entish.Attributes;

namespace Tkmm.AalSharp.Bars.Data;

[Swappable]
public unsafe partial struct ResAudioResource() : IMemoryResource<ResAudioResource>
{
    public const uint AudioResourceMagic = 0x53524142;
    public const int AudioResourceVersion = 0x0101;

    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    [NeverSwap]
    public readonly uint Magic = AudioResourceMagic;
    public int FileSize;
    public Endianness Endianness;
    public ushort Version = AudioResourceVersion;
    public int AssetCount;

    public ref ResAssetOffset this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get {
            if (index > AssetCount) {
                throw new ArgumentOutOfRangeException($"Index {index} was outside the bounds of the array [{AssetCount}].");
            }

            int resourceOffset = sizeof(ResAudioResource)
                                 + AssetCount * sizeof(int)
                                 + sizeof(ResAssetOffset) * index;
#pragma warning disable CS9084 // Expected: Return lifetime is the same as 'this' lifetime
            return ref MemUtils.GetRelativeTo<ResAssetOffset, ResAudioResource>(this, resourceOffset);
#pragma warning restore CS9084 // Expected: Return lifetime is the same as 'this' lifetime
        }
    }

    public ref ResAssetOffset this[StringView name] {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get {
            if (AssetCount == 0) {
                return ref Unsafe.NullRef<ResAssetOffset>();
            }

            uint expectedHash = Crc32.HashToUInt(name);

            int a = 0;
            int b = AssetCount - 1;
            while (a <= b) {
                int m = (a + b) / 2;
                uint hash = MemUtils.GetRelativeTo<uint, ResAudioResource>(this, sizeof(ResAudioResource) + sizeof(uint) * m);

                if (expectedHash < hash) {
                    b = m - 1;
                }
                else if (expectedHash > hash) {
                    a = m + 1;
                }
                else {
                    return ref this[m];
                }
            }

            return ref Unsafe.NullRef<ResAssetOffset>();
        }
    }

    public static IEnumerable<Exception> GetErrors(ResAudioResource value)
    {
        if (value.Magic is not AudioResourceMagic) {
            yield return new InvalidDataException(
                $"Invalid audio resources magic: '{PrimitivesHelper.ToAscii(value.Magic)}'");
        }

        if (value.Version is not AudioResourceVersion) {
            yield return new InvalidDataException(
                $"Unsupported audio resources version: '{value.Version}'");
        }
    }

    public Enumerator GetEnumerator()
    {
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
        return new Enumerator(this);
#pragma warning restore CS9084 // Struct member returns 'this' or other instance members by reference
    }

    public ref struct Enumerator(in ResAudioResource resource)
    {
        private readonly ResAudioResource* _resources = (ResAudioResource*)Unsafe.AsPointer(in resource);
        private int _index = -1;

        public ref ResAssetOffset Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get => ref Unsafe.AsRef<ResAudioResource>(_resources)[_index];
        }

        public bool MoveNext()
        {
            return ++_index < (*_resources).AssetCount;
        }

        public void Reset()
        {
            _index = -1;
        }
    }
}