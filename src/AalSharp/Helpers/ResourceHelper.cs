using Entish;

namespace AalSharp.Helpers;

public static unsafe class ResourceHelper
{
    public static int GetAssetSize(void* ptr)
    {
        var bom = *(Endianness*)((byte*)ptr + 0x4);
        var size = *(int*)((byte*)ptr + 0xC);

        return EndianUtils.ShouldSwap(bom) ? EndianUtils.Swap(size) : size;
    }
}