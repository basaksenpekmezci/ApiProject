# ApiProject

Çok firmalı (SaaS) .NET 8 Web API. Tüm firmalar tek SQL Server veritabanında tutulur ve `FirmaId` ile ayrılır.

- `GET /api/firma/config`: isteğin Host'una göre firmayı bulur ve config bilgilerini döner.
- `POST /api/auth/login`: JWT login. Firma önce Host'tan bulunur. Bulunamazsa body'deki `firmaKodu` kullanılır.
- `GET /api/auth/ben`: token'ı denemek için korumalı endpoint.

## Örnek veri (seed)

| Firma | Domainler | Kullanıcı | Şifre |
|---|---|---|---|
| ABC (ABC Teknoloji) | `abc.localhost`, `abc.ornek.com` | `admin` | `Abc123!` |
| XYZ (XYZ Lojistik) | `xyz.localhost`, `xyz.ornek.com` | `admin` | `Xyz123!` |

İki firmada da kullanıcı adı `admin`. Şifreler farklı olduğu için hangi firmanın kullanıcısıyla giriş yapıldığı kolayca görülür.

## Çalıştırma

### 1. Gereksinimler
- .NET 8 SDK (`dotnet --list-sdks` çıktısında 8.0.x görünmeli)
- Docker Desktop (SQL Server Docker'da çalışır)

### 2. SQL Server'ı Docker'da başlat
```bash
docker run -d --name apiproject-sql-1434 --platform linux/amd64 \
  -e 'ACCEPT_EULA=Y' -e 'MSSQL_SA_PASSWORD=ApiProject_Sql_2026' \
  -p 1434:1433 -v apiproject-sql-1434-data:/var/opt/mssql \
  mcr.microsoft.com/mssql/server:2022-latest
```
- Apple Silicon (M1/M2/M3/M4) Mac'te Docker Desktop > Settings > General altında **"Use Rosetta for x86_64/amd64 emulation on Apple Silicon"** açık olmalı. SQL Server imajı yalnızca amd64 için yayınlanıyor.
- Veriler `apiproject-sql-1434-data` volume'unda kalır, container silinse bile kaybolmaz.
- Sonraki seferlerde `docker start apiproject-sql-1434` yeterli.
- Hazır olup olmadığını görmek için: `docker logs apiproject-sql-1434 | grep "Recovery is complete"`

`src/ApiProject.Api/appsettings.json` içindeki bağlantı cümlesi bu container'a göre ayarlı:
```
Server=localhost,1434;Database=ApiProjectDb;User Id=sa;Password=ApiProject_Sql_2026;TrustServerCertificate=True
```
Bu şifre yalnızca yerel geliştirme içindir. Canlı ortamda bağlantı cümlesini `ConnectionStrings__Default` environment variable'ı ile ver.

### 3. Çalıştır
```bash
cd src/ApiProject.Api
dotnet run
```
Development ortamında uygulama açılırken migration'ları otomatik uygular. Bu adım `ApiProjectDb` veritabanını, tabloları ve seed verisini oluşturur. Swagger şu adreste açılır: http://localhost:5000/swagger

Visual Studio kullanıyorsan `ApiProject.sln` dosyasını aç ve **http** profiliyle F5'e bas.

## Test

### Swagger ile
1. http://localhost:5000/swagger adresinde `POST /api/auth/login` çalıştır:
   ```json
   { "firmaKodu": "ABC", "kullaniciAdi": "admin", "sifre": "Abc123!" }
   ```
   `localhost` hiçbir firmaya bağlı olmadığı için burada `firmaKodu` zorunludur. Yanıtta `token` döner.
2. Sağ üstteki **Authorize** butonuna bas, token'ı yapıştır (başına "Bearer" yazma).
3. `GET /api/auth/ben` çalıştır. `firmaKodu: "ABC"` döner.
4. Aynı login'i `"firmaKodu": "XYZ"` ve `"sifre": "Abc123!"` ile dene. 401 döner, çünkü XYZ'deki `admin` farklı bir kullanıcı.

**Domainle test (Swagger):** Chrome, Edge ve Firefox `*.localhost` adreslerini kendi bilgisayarına yönlendirir. Swagger'ı http://abc.localhost:5000/swagger adresinden açarsan:
- `GET /api/firma/config` ABC'nin config'ini döner.
- `firmaKodu` göndermeden login olabilirsin.

http://xyz.localhost:5000/swagger adresinde aynı istekler XYZ için çalışır.

### Postman ile
Adres her zaman `http://localhost:5000/...` olsun. Domaini taklit etmek için **Headers** sekmesine `Host` header'ı ekle:

| İstek | Header / Body | Beklenen |
|---|---|---|
| `GET /api/firma/config` | `Host: abc.localhost` | 200, ABC config |
| `GET /api/firma/config` | `Host: xyz.localhost` | 200, XYZ config |
| `GET /api/firma/config` | (Host yok) | 404 |
| `POST /api/auth/login` | Host yok, body'de `"firmaKodu":"ABC"`, `admin` / `Abc123!` | 200 + token |
| `POST /api/auth/login` | `Host: xyz.localhost`, body'de `admin` / `Xyz123!` | 200 + token (XYZ) |
| `POST /api/auth/login` | `Host: xyz.localhost`, body'de `"firmaKodu":"ABC"` | 400, kod domainle uyuşmuyor |
| `GET /api/auth/ben` | `Authorization: Bearer <ABC token>` | 200 |
| `GET /api/auth/ben` | ABC token + `Host: xyz.localhost` | 403, token başka firmaya ait |

Hazır istekler `src/ApiProject.Api/ApiProject.Api.http` dosyasında. Bu dosyayı Visual Studio ya da VS Code (REST Client eklentisi) ile tek tıkla çalıştırabilirsin.

### "Login failed for user 'sa'" (Error 18456)
Bağlantı cümlesindeki şifre, container ilk kurulduğunda verilen şifreyle uyuşmuyor demektir. SQL Server `sa` şifresini ilk açılışta volume'a yazar. Sonradan `MSSQL_SA_PASSWORD` değerini değiştirmek mevcut bir volume'u etkilemez.

Başka bir projenin SQL container'ına dokunmamak için bu proje ayrı bir container (`apiproject-sql-1434`), ayrı bir volume (`apiproject-sql-1434-data`) ve ayrı bir port (1434) kullanır. Bilgisayarındaki başka bir SQL Server 1433'te çalışmaya devam edebilir. Uygulamanın doğru container'a bağlandığından emin olmak için bağlantı cümlesinde `localhost,1434` yazdığını kontrol et.

### Hesap kilitlenirse
5 hatalı denemeden sonra kullanıcı 15 dakika kilitlenir. Beklemek istemezsen SQL'de şunu çalıştır:
```sql
UPDATE Kullanici SET HataliGirisSayisi = 0, KilitBitisTarihi = NULL;
```

## Notlar
- JWT anahtarı geliştirme için `appsettings.Development.json` içinde duruyor. Canlı ortamda bu anahtarı environment variable (`Jwt__Key`) ya da secret store ile ver.
- Yeni firma eklemek için `Firma`, `FirmaDomain` ve `FirmaAyar` tablolarına satır eklemen yeterli. Domain küçük harfle ve `www.` olmadan yazılmalı. Domain sorguları 5 dakika cache'lendiği için değişiklikler en geç 5 dakika içinde yansır.
- Şema değişince yeni migration oluştur: `dotnet ef migrations add <Ad> -o Data/Migrations` (önce `dotnet tool install -g dotnet-ef --version 8.0.31`).
