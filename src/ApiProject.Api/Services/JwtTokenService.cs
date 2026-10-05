using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiProject.Api.Entities;
using ApiProject.Api.Options;
using ApiProject.Api.Tenancy;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ApiProject.Api.Services;

public class JwtTokenService
{
    private readonly JwtOptions _opt;

    public JwtTokenService(IOptions<JwtOptions> opt)
    {
        _opt = opt.Value;
    }

    public (string Token, DateTime GecerlilikBitis) Uret(Kullanici kullanici, string firmaKodu)
    {
        var bitis = DateTime.UtcNow.AddMinutes(_opt.SureDakika);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, kullanici.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, kullanici.KullaniciAdi),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(FirmaClaimTipleri.FirmaId, kullanici.FirmaId.ToString()),
            new Claim(FirmaClaimTipleri.FirmaKodu, firmaKodu),
        };

        var anahtar = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
        var token = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            expires: bitis,
            signingCredentials: new SigningCredentials(anahtar, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), bitis);
    }
}
