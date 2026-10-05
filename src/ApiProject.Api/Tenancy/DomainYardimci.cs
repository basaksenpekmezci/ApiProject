namespace ApiProject.Api.Tenancy;

public static class DomainYardimci
{
    /// <summary>"WWW.Abc.Ornek.com." gibi bir host'u "abc.ornek.com" biçimine getirir. Port zaten içermez.</summary>
    public static string Normalize(string host)
    {
        var d = host.Trim().TrimEnd('.').ToLowerInvariant();
        return d.StartsWith("www.") ? d[4..] : d;
    }
}
