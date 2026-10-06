# ApiProject

C1Soft eğitimi 6. gün ödevi: domain'e göre firma config'i dönen, JWT login'li çok firmalı (SaaS) bir .NET 8 Web API.

## Ödevde istenen
- Tüm firmalar tek veritabanında, tek tablo mantığıyla tutulsun.
- İsteğin geldiği domainden firma bulunsun ve o firmanın config'i dönsün.
- JWT ile giriş olsun. Aynı kullanıcı adı farklı firmalarda bulunabileceği için kullanıcı, firmasıyla birlikte ayırt edilsin.

## Nasıl çözdüm
- Tablolar: `Firma`, `FirmaDomain` (bir firmanın birden çok domaini olabilir), `FirmaAyar` (config) ve `Kullanici`.
- Kullanıcı adı tek başına değil, firmasıyla birlikte benzersiz: `(FirmaId, KullaniciAdi)` unique index. Böylece iki firmada `admin` olabilir, ama aynı firmada iki `admin` olamaz.
- Firma önce isteğin Host bilgisinden bulunuyor. Bulunamazsa (localhost, mobil gibi) `X-Client` header'ındaki firma kodu kullanılıyor. İkisi de yoksa ya da farklı firmaları gösteriyorsa 400 dönüyor.
- Token'a firma bilgisi yazılıyor. Bir firmanın token'ı başka firmanın domaininde reddediliyor.
- Derste belirtildiği gibi çok katmanlı bir mimari yerine tek proje tercih ettim.
- Domain sorguları 5 dakika cache'leniyor, her istekte veritabanına gidilmiyor.

## Geliştirme süreci
Projeyi Claude Code ile geliştirdim. Kod yazılmadan önce tablo yapısını ve iskeleti planladık. Bu aşamada FirmaId ile bağlama, tek proje yapısı, refresh token ve rollerin şimdilik eklenmemesi gibi kararları verdim. Ardından projeyi Mac'te, Docker üzerindeki SQL Server ile çalıştırıp senaryoları Swagger'da test ettim.

## Test ettiklerim
| Giriş | Sonuç |
|---|---|
| `X-Client: ABC`, admin / Abc123! | 200, token döndü |
| `X-Client: XYZ`, admin / Abc123! | 401, XYZ'deki admin farklı kullanıcı |
| `X-Client: XYZ`, admin / Xyz123! | 200, token döndü |
| Host ve X-Client olmadan login | 400, firma belirlenemedi |
| abc.localhost üzerinden config isteği | 200, ABC'nin ayarları döndü |
| xyz.localhost üzerinden config isteği | 200, XYZ'nin ayarları döndü |

## Çalıştırma
1. SQL Server'ı Docker'da başlat:
```bash
docker run -d --name apiproject-sql-1434 --platform linux/amd64 \
  -e 'ACCEPT_EULA=Y' -e 'MSSQL_SA_PASSWORD=ApiProject_Sql_2026' \
  -p 1434:1433 -v apiproject-sql-1434-data:/var/opt/mssql \
  mcr.microsoft.com/mssql/server:2022-latest
```
2. Yerel ayar dosyasını örnekten oluştur:
```bash
cd src/ApiProject.Api
cp appsettings.Development.example.json appsettings.Development.json
```
3. `appsettings.Development.json` içinde iki yer tutucuyu değiştir:
   - `BURAYA_KENDI_KEYINIZI_YAZIN` yerine en az 32 karakterlik bir key yaz. Üretmek için: `openssl rand -base64 64`
   - `BURAYA_SQL_SIFRENIZI_YAZIN` yerine 1. adımdaki SQL şifresini yaz.

   Bu dosya `.gitignore`'da olduğu için repoya gitmez.
4. Projeyi çalıştır. Veritabanı, tablolar ve örnek veri otomatik oluşur:
```bash
dotnet run
```
5. http://localhost:5000/swagger adresini aç. Login'de `X-Client` alanına firma kodunu (`ABC` ya da `XYZ`) yaz.
