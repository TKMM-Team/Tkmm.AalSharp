using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Entish;

namespace AalSharp;

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 4)]
public readonly unsafe struct Offset<T> : ISwappable<Offset<T>> where T : unmanaged
{
    public readonly uint Value;

    public Offset()
    {
    }

    public Offset(uint value)
    {
        Value = value;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Get<TParent>(ref TParent relativeOffset) where TParent : unmanaged
    {
        fixed (TParent* loc = &relativeOffset) {
            byte* ptr = (byte*)loc + Value;
            return ref *(T*)ptr;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* GetPointer(void* relativeOffset)
    {
        return (T*)((byte*)relativeOffset + Value);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> GetSpan<TParent>(ref TParent relativeOffset, int length) where TParent : unmanaged
    {
        fixed (TParent* loc = &relativeOffset) {
            byte* ptr = (byte*)loc + Value;
            return new Span<T>(ptr, length);
        }
    }

    public static void Swap(Offset<T>* value)
    {
        EndianUtils.Swap(&value->Value);
    }
}