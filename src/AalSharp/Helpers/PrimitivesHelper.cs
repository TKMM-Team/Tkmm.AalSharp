using System.Text;

namespace AalSharp.Helpers;

public class PrimitivesHelper
{
    public static unsafe string ToAscii<T>(T value) where T : unmanaged
    {
        return Encoding.ASCII.GetString(new Span<byte>(&value, sizeof(T)));
    }
}