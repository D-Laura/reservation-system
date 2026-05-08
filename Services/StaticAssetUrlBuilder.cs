namespace ReservationSystem.Services;

public sealed class StaticAssetUrlBuilder : IStaticAssetUrlBuilder
{
    private readonly string _baseUrl;

    public StaticAssetUrlBuilder(IConfiguration configuration)
    {
        _baseUrl = configuration["STATIC_ASSETS_BASE_URL"] ?? string.Empty;
    }

    public string Url(string path)
    {
        var normalizedPath = path.TrimStart('~', '/');

        if (string.IsNullOrWhiteSpace(_baseUrl))
        {
            return "/" + normalizedPath;
        }

        return $"{_baseUrl.TrimEnd('/')}/{normalizedPath}";
    }
}
