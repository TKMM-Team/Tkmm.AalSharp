using System.Runtime.CompilerServices;
using AalSharp.Hashing;
using AalSharp.Helpers;
using Entish;
using Entish.Attributes;

namespace AalSharp.Bars.Data;

[Swappable]
public unsafe partial struct AudioResources() : IMemoryResource<AudioResources>
{
    public const uint Magic = 0x53524142;
    public const int Version = 0x0101;

    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    public AudioResourcesHeader Header = default;

    public ref AudioResource this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get {
            if (index > Header.AssetCount) {
                throw new ArgumentOutOfRangeException($"Index {index} was outside the bounds of the array [{Header.AssetCount}].");
            }

            int resourceOffset = sizeof(AudioResourcesHeader)
                                 + Header.AssetCount * sizeof(int)
                                 + sizeof(AudioResource) * index;
#pragma warning disable CS9084 // Expected: Return lifetime is the same as 'this' lifetime
            return ref MemUtils.GetRelativeTo<AudioResource, AudioResources>(this, resourceOffset);
#pragma warning restore CS9084 // Expected: Return lifetime is the same as 'this' lifetime
        }
    }

    public ref AudioResource this[StringView name] {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get {
            if (Header.AssetCount == 0) {
                return ref Unsafe.NullRef<AudioResource>();
            }

            uint expectedHash = Crc32.HashToUInt(name);

            int a = 0;
            int b = Header.AssetCount - 1;
            while (a <= b) {
                int m = (a + b) / 2;
                uint hash = MemUtils.GetRelativeTo<uint, AudioResources>(this, sizeof(AudioResourcesHeader) + sizeof(uint) * m);

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

            return ref Unsafe.NullRef<AudioResource>();
        }
    }

    public static IEnumerable<Exception> GetErrors(AudioResources value)
    {
        if (value.Header.Magic is not Magic) {
            yield return new InvalidDataException(
                $"Invalid audio resources magic: '{PrimitivesHelper.ToAscii(value.Header.Magic)}'");
        }

        if (value.Header.Version is not Version) {
            yield return new InvalidDataException(
                $"Unsupported audio resources version: '{value.Header.Version}'");
        }
    }

    public Enumerator GetEnumerator()
    {
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
        return new Enumerator(this);
#pragma warning restore CS9084 // Struct member returns 'this' or other instance members by reference
    }

    public ref struct Enumerator(in AudioResources resources)
    {
        private readonly AudioResources* _resources = (AudioResources*)Unsafe.AsPointer(in resources);
        private int _index = -1;

        public ref AudioResource Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get => ref Unsafe.AsRef<AudioResources>(_resources)[_index];
        }

        public bool MoveNext()
        {
            return ++_index < (*_resources).Header.AssetCount;
        }

        public void Reset()
        {
            _index = -1;
        }
    }
}