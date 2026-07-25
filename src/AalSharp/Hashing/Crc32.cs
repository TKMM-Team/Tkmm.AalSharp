namespace AalSharp.Hashing;

public class Crc32
{
    public static uint HashToUInt(StringView stringView) => HashToUInt(stringView.Value);
    
    public static uint HashToUInt(ReadOnlySpan<byte> data)
    {
        int size = data.Length;
        uint crc = 0xFFFFFFFF;
        for (int i = 0; i < size; ++i) {
            crc ^= data[i];
            for (int j = 0; j < 8; ++j) {
                uint mask = (uint)-(crc & 1);
                crc = (crc >> 1) ^ (0xEDB88320 & mask);
            }
        }
        return ~crc;
    }
}