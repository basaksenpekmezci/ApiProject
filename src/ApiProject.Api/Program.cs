using System.Text;
using ApiProject.Api.Arama;
using ApiProject.Api.Data;
using ApiProject.Api.Entities;
using ApiProject.Api.Middleware;
using ApiProject.Api.Options;
using ApiProject.Api.Services;
using ApiProject.Api.Swagger;
using ApiProject.Api.Tenancy;
using ApiProject.Api.Yetki;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Serialization;
using Elastic.Transport;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IFirmaBaglami, FirmaBaglami>();
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddMemoryCache();

builder.Services.AddScoped<FirmaService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<KullaniciService>();
builder.Services.AddScoped<UrunService>();
builder.Services.AddScoped<SepetService>();
builder.Services.AddScoped<SiparisService>();
builder.Services.AddScoped<YonetimService>();
builder.Services.AddScoped<UrunSeedService>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddSingleton<IPasswordHasher<Kullanici>, PasswordHasher<Kullanici>>();

var jwtBolum = builder.Configuration.GetSection(JwtOptions.Bolum);
builder.Services.AddOptions<JwtOptions>()
    .Bind(jwtBolum)
    .Validate(o => !string.IsNullOrEmpty(o.Key) && o.Key.Length >= 32, "Jwt:Key en az 32 karakter olmalı.")
    .ValidateOnStart();
var jwt = jwtBolum.Get<JwtOptions>()!;

builder.Services.Configure<ElasticsearchOptions>(builder.Configuration.GetSection(ElasticsearchOptions.Bolum));
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection(RedisOptions.Bolum));

var elasticAyar = builder.Configuration.GetSection(ElasticsearchOptions.Bolum).Get<ElasticsearchOptions>() ?? new();
builder.Services.AddSingleton(new ElasticsearchClient(
    new ElasticsearchClientSettings(new SingleNodePool(new Uri(elasticAyar.Url)),
            (_, ayar) => new DefaultSourceSerializer(ayar, o => o.PropertyNamingPolicy = null))
        .DefaultFieldNameInferrer(ad => ad)
        .RequestTimeout(TimeSpan.FromSeconds(60))));
builder.Services.AddScoped<ElasticService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false; // claim adları (sub, firma_id) dönüştürülmesin
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
builder.Services.AddAuthorization(o =>
    o.AddPolicy(YetkiTanimlari.YoneticiPolitikasi, p => p.RequireClaim(YetkiTanimlari.YoneticiClaim, "true")));

var izinliOriginler = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(izinliOriginler)
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiProject", Version = "v1" });
    c.OperationFilter<XClientHeaderFilter>();
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
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
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();

    try
    {
        await scope.ServiceProvider.GetRequiredService<ElasticService>().IndeksHazirlaAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning("Elasticsearch index'i hazırlanamadı, arama SQL'den yapılacak: {Hata}", ex.Message);
    }

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseMiddleware<FirmaCozumlemeMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
