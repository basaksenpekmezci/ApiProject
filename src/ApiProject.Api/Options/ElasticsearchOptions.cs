namespace ApiProject.Api.Options;

public class ElasticsearchOptions
{
    public const string Bolum = "Elasticsearch";

    public string Url { get; set; } = "http://localhost:9200";
}
