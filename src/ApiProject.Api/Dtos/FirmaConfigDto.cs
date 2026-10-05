using System.Text.Json;

namespace ApiProject.Api.Dtos;

public record FirmaConfigDto(
    string FirmaKodu,
    string FirmaAdi,
    string? LogoUrl,
    string? TemaRengi,
    string Dil,
    string ZamanDilimi,
    JsonElement? EkAyarlar);
