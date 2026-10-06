using System.Security.Claims;

namespace ApiProject.Api.Yetki;

public static class ClaimsPrincipalUzantilari
{
    // Giriş yapmış kullanıcının Id'si token'ın "sub" claim'inde.
    public static Guid KullaniciId(this ClaimsPrincipal user)
    {
        return Guid.Parse(user.FindFirst("sub")!.Value);
    }
}
