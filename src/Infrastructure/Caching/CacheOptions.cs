namespace Infrastructure.Caching;

internal class CacheOptions
{
    public const string Settings = nameof(CacheOptions);

    public int SlidingExpiration { get; set; } = 10;
    public int AbsoluteExpiration { get; set; } = 60;
}
