namespace ShioUI.Caching;

partial class CacheStore<T>
{
    public readonly record struct Body(T[] Array, int Count);
}
