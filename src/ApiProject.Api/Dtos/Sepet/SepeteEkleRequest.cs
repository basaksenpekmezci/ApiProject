using System.ComponentModel.DataAnnotations;

namespace ApiProject.Api.Dtos;

public class SepeteEkleRequest
{
    [Required]
    public Guid UrunId { get; set; }

    [Range(1, 1000)]
    public int Adet { get; set; } = 1;
}
