namespace AalSharp;

public interface IMemoryResource<T> where T : unmanaged
{
    static abstract IEnumerable<Exception> GetErrors(T value);
}