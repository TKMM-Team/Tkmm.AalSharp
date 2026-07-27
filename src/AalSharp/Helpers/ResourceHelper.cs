namespace AalSharp.Helpers;

public static unsafe class ResourceHelper
{
    public static int GetAalResourcesSize(void* ptr)
        => *(int*)((byte*)ptr + 0x8);
    
    public static int GetAssetSize(void* ptr)
        => *(int*)((byte*)ptr + 0xC);
}