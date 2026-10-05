namespace ApiProject.Api.Entities;

public class FirmaDomain
{
    public int Id { get; set; }
    public int FirmaId { get; set; }
    /// <summary>Küçük harf, port ve "www." olmadan saklanır.</summary>
    public string Domain { get; set; } = null!;
    public bool VarsayilanMi { get; set; }

    public Firma Firma { get; set; } = null!;
}
