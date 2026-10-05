namespace ApiProject.Api.Options;

public class JwtOptions
{
    public const string Bolum = "Jwt";

    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    /// <summary>HMAC-SHA256 anahtarı, en az 32 karakter.</summary>
    public string Key { get; set; } = null!;
    public int SureDakika { get; set; } = 60;
}
