using System.Text;
using ApiProject.Api.Data;
using ApiProject.Api.Entities;
using ApiProject.Api.Middleware;
using ApiProject.Api.Options;
using ApiProject.Api.Services;
using ApiProject.Api.Tenancy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Veritabanı ve firma bağlamı
builder.Services.AddScoped<IFirmaBaglami, FirmaBaglami>();
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddMemoryCache();

// Servisler
builder.Services.AddScoped<FirmaService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddSingleton<IPasswordHasher<Kullanici>, PasswordHasher<Kullanici>>();

// JWT
var jwtBolum = builder.Configuration.GetSection(JwtOptions.Bolum);
builder.Services.AddOptions<JwtOptions>()
    .Bind(jwtBolum)
    .Validate(o => !string.IsNullOrEmpty(o.Key) && o.Key.Length >= 32, "Jwt:Key en az 32 karakter olmalı.")
    .ValidateOnStart();
var jwt = jwtBolum.Get<JwtOptions>()!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false; // "sub", "firma_id" gibi claim adları olduğu gibi kalsın
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "unique_name"
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiProject", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Login'den dönen token'ı yapıştırın (başına 'Bearer' yazmadan)."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Geliştirmede veritabanını otomatik oluştur / güncelle (seed data dahil).
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseMiddleware<FirmaCozumlemeMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
