namespace ApiProject.Api.Options;

public class RedisOptions
{
    public const string Bolum = "Redis";

    public string Url { get; set; } = "localhost:6379";
}
