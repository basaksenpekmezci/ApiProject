namespace ApiProject.Api.Entities;

public class FirmaDomain
{
    public int Id { get; set; }
    public int FirmaId { get; set; }
    // küçük harf, port ve "www." olmadan
    public string Domain { get; set; } = null!;
    public bool VarsayilanMi { get; set; }

    public Firma Firma { get; set; } = null!;
}
