namespace ApiProject.Api.Tenancy;

public static class DomainYardimci
{
    // "WWW.Abc.Ornek.com." -> "abc.ornek.com"
    public static string Normalize(string host)
    {
        var d = host.Trim().TrimEnd('.').ToLowerInvariant();
        return d.StartsWith("www.") ? d[4..] : d;
    }
}
