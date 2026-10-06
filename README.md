# ApiProject

C1Soft eğitimi ödevi: JWT login'li, çok firmalı (SaaS) bir .NET 8 Web API ve bu API'yi kullanan basit bir yönetim paneli.

- 6. gün: domain'e göre firma config'i, firmaya göre login.
- 7. gün: GUID ID'ler, ürün, sepet, sipariş, firma bazında kullanıcı kaydı ve yönetim paneli.

## Proje yapısı
```
src/
  ApiProject.Api/              Web API (Swagger: http://localhost:5000/swagger)
    Controllers/               HTTP uç noktaları (Auth, Firma, Kullanici, Urun, Sepet, Siparis, Yonetim)
    Services/                  İş kuralları; controller'lar sadece bunları çağırır
    Entities/                  Veritabanı tabloları
    Dtos/                      İstek ve yanıt modelleri (Kullanicilar, Urunler, Sepet, Siparisler alt klasörleri)
    Data/                      AppDbContext, tablo ayarları (Configurations), migration'lar, örnek veri
    Tenancy/                   Firma çözümleme ve firma filtresi (IFirmayaAit, FirmaBaglami)
    Middleware/                İsteğin firmasını Host / X-Client header'ından bulan middleware
    Yetki/                     Yönetici yetkisi ve token'dan kullanıcı Id'si okuma
  ApiProject.YonetimPaneli/    Yönetim paneli (http://localhost:5100), sadece wwwroot altındaki HTML/CSS/JS
```

## Ödevde istenenler
**6. gün**
- Tüm firmalar tek veritabanında, tek tablo mantığıyla tutulsun.
- İsteğin geldiği domainden firma bulunsun ve o firmanın config'i dönsün.
- JWT ile giriş olsun. Aynı kullanıcı adı farklı firmalarda bulunabileceği için kullanıcı, firmasıyla birlikte ayırt edilsin.

**7. gün**
1. Tüm ID'ler int yerine GUID olsun.
2. Ürün ekleme ve ürün arama (firmaya özel).
3. Sepet: ürün ekleme, çıkarma, görüntüleme; stok ve pasif ürün kontrolü.
4. Sepetten sipariş oluşturma.
5. Aynı kullanıcı adı farklı firmada açılabilsin, aynı firmada tekrar açılmak istenirse uyarı dönsün.
6. Ayrı bir web sayfası olarak yönetim paneli: firma koduyla giriş, firmanın siparişleri ve kullanıcıları.
7. Her sorguda firma filtresi olsun; bir firma başka firmanın ürününü, sepetini, siparişini, kullanıcısını göremesin.

## Nasıl çözdüm
**Firma belirleme ve giriş**
- Tablolar: `Firma`, `FirmaDomain` (bir firmanın birden çok domaini olabilir), `FirmaAyar` (config), `Kullanici`, `Urun`, `SepetKalemi`, `Siparis`, `SiparisKalemi`.
- Firma önce isteğin Host bilgisinden bulunuyor. Bulunamazsa (localhost, mobil, panel gibi) `X-Client` header'ındaki firma kodu kullanılıyor. İkisi de yoksa ya da farklı firmaları gösteriyorsa 400 dönüyor.
- Token'a firma bilgisi yazılıyor. Bir firmanın token'ı başka firmanın domaininde ya da `X-Client`'ıyla reddediliyor (403).
- Domain sorguları 5 dakika cache'leniyor, her istekte veritabanına gidilmiyor.
- Çok katmanlı bir mimari yerine tek API projesi tercih ettim.

**Firma izolasyonu**
- Firmaya ait her tablo (`Kullanici`, `Urun`, `SepetKalemi`, `Siparis`, `SiparisKalemi`) `IFirmayaAit` arayüzünü taşıyor.
- `AppDbContext`, bu arayüzü taşıyan her tabloya otomatik olarak `FirmaId == aktif firma` filtresi (global query filter) ekliyor. Yeni bir tablo eklenince filtreyi ayrıca yazmak gerekmiyor. Firma belirlenmemişse hiçbir kayıt dönmüyor.
- Kayıt sırasında (`SaveChanges`) `FirmaId` aktif firmadan otomatik yazılıyor. Başka firmaya ait bir kaydı değiştirme denemesi hata veriyor.
- Sepet ve siparişler ayrıca giriş yapan kullanıcıyla sınırlı: kullanıcı sadece kendi sepetini ve siparişlerini görüyor. Yönetici ise firmasının tüm siparişlerini görüyor.

**GUID**
- Tüm `Id` ve `FirmaId` alanları `Guid`. Yeni kayıtların Id'lerini EF Core üretiyor (SQL Server'da index'e uygun sıralı GUID). Örnek veriler sabit GUID'lerle (`SeedData.cs`).

**Kullanıcı kaydı**
- Kullanıcı adı firmasıyla birlikte benzersiz: `(FirmaId, NormalizeKullaniciAdi)` unique index. İki firmada `ayse` olabilir, aynı firmada ikinci `ayse` açılmak istenirse `409` ve "Bu kullanıcı adı bu firmada zaten kayıtlı." dönüyor. Büyük/küçük harf farkı sayılmıyor.

**Ürün, sepet, sipariş**
- Ürün kodu firma içinde benzersiz. Ürün ekleme ve güncelleme sadece yöneticiye açık, arama her giriş yapmış kullanıcıya açık. Arama ad ve koda göre yapılıyor, pasif ürünler varsayılan olarak listelenmiyor.
- Sepete pasif ürün eklenemiyor, sepetteki toplam adet stoğu aşamıyor. Aynı ürün tekrar eklenince adedi artıyor.
- Sipariş tek bir transaction içinde oluşturuluyor: pasiflik ve stok tekrar kontrol ediliyor (ürün sepete eklendikten sonra değişmiş olabilir), stok "yeterliyse düş" şeklinde tek bir UPDATE ile azaltılıyor, ürün adı ve fiyatı siparişe kopyalanıyor, sepet boşaltılıyor. Aynı anda gelen iki sipariş aynı stoğu kullanamıyor.

**Yetki**
- Rol tablosu yerine `Kullanici.YoneticiMi` alanı var. Token'a `yonetici` claim'i olarak yazılıyor. Örnek `admin` kullanıcıları yönetici, kayıtla açılan kullanıcılar değil.

**Yönetim paneli**
- `ApiProject.YonetimPaneli` ayrı bir proje; sadece düz HTML, CSS ve JavaScript sunuyor (framework yok). Veriler tarayıcıdan `fetch` ile doğrudan API'den alınıyor.
- Firma kodu her istekte `X-Client` header'ı ile, token `Authorization` header'ı ile gidiyor. Tarayıcıda F12 > Network sekmesinden bütün istekler ve header'lar izlenebiliyor.
- Panel farklı bir adreste (5100) çalıştığı için API'nin CORS listesine `http://localhost:5100` eklendi.

## Uç noktalar
Firma her istekte Host veya `X-Client` header'ı ile belirlenir. "Token" yazanlar `Authorization: Bearer <token>` ister.

| Metot ve adres | Erişim | Açıklama |
|---|---|---|
| `GET /api/firma/config` | herkes | Firmanın ayarları |
| `POST /api/auth/login` | herkes | Giriş, token döner |
| `GET /api/auth/ben` | token | Giriş yapan kullanıcı |
| `POST /api/kullanicilar` | herkes | Kullanıcı kaydı, aynı firmada aynı adla 409 |
| `GET /api/urunler?arama=&sadeceAktif=true` | token | Ürün arama |
| `POST /api/urunler` | yönetici | Ürün ekleme |
| `PUT /api/urunler/{id}` | yönetici | Ürün güncelleme (fiyat, stok, aktif/pasif) |
| `GET /api/sepet` | token | Sepeti görüntüleme |
| `POST /api/sepet/urunler` | token | Sepete ürün ekleme `{ "urunId": "...", "adet": 1 }` |
| `DELETE /api/sepet/urunler/{urunId}` | token | Ürünü sepetten çıkarma |
| `POST /api/siparisler` | token | Sepetten sipariş oluşturma |
| `GET /api/siparisler` | token | Kendi siparişlerim |
| `GET /api/yonetim/siparisler` | yönetici | Firmanın tüm siparişleri |
| `GET /api/yonetim/kullanicilar` | yönetici | Firmanın kullanıcıları |

## Çalıştırma
1. SQL Server'ı Docker'da başlat (container zaten varsa sadece `docker start apiproject-sql-1434`):
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

   Bu dosya `.gitignore`'da olduğu için repoya gitmez. Dosyayı daha önce oluşturduysan `Cors:AllowedOrigins` listesine `"http://localhost:5100"` satırını eklemeyi unutma, yoksa panel API'ye bağlanamaz.
4. Eski (int ID'li) veritabanı duruyorsa önce aşağıdaki "Veritabanını sıfırlama" adımlarını uygula.
5. API'yi çalıştır. Veritabanı, tablolar ve örnek veri otomatik oluşur:
```bash
cd src/ApiProject.Api
dotnet run
```
6. Yeni bir terminalde paneli çalıştır:
```bash
cd src/ApiProject.YonetimPaneli
dotnet run
```
7. API için http://localhost:5000/swagger, panel için http://localhost:5100 adresini aç.

## Veritabanını sıfırlama
Tüm ID'ler `int`'ten `Guid`'e çevrildiği için eski migration silindi ve yerine tek bir yeni `IlkKurulum` migration'ı geldi. Eski veritabanı bu migration ile uyumlu değil, bu yüzden bir kez silinip yeniden kurulmalı. Sadece bu projenin `apiproject-sql-1434` container'ındaki `ApiProjectDb` veritabanı silinir, diğer container'lara dokunulmaz.

1. Uygulama çalışıyorsa durdur.
2. Veritabanını sil (iki yoldan biri yeterli):
```bash
# a) EF Core aracıyla (bir kez kurmak gerekir: dotnet tool install -g dotnet-ef --version 8.*)
cd src/ApiProject.Api
dotnet ef database drop -f

# b) Container içindeki sqlcmd ile
docker exec -it apiproject-sql-1434 /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'ApiProject_Sql_2026' -C \
  -Q "ALTER DATABASE ApiProjectDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE ApiProjectDb;"
```
3. `dotnet run` ile API'yi başlat. Veritabanı, tablolar ve örnek veri yeniden oluşur.

## Örnek veriler
| Firma | Domain | Kullanıcı | Şifre | Yönetici |
|---|---|---|---|---|
| ABC | abc.localhost | admin | Abc123! | Evet |
| XYZ | xyz.localhost | admin | Xyz123! | Evet |

Ürün ve sipariş örnek verisi yok; aşağıdaki test adımlarıyla oluşturulur.

## Test adımları
Swagger'da her isteğin `X-Client` alanına firma kodunu yaz. Token isteyen uç noktalar için login yanıtındaki `token`'ı sağ üstteki **Authorize** butonuna yapıştır. Aynı istekler `src/ApiProject.Api/ApiProject.Api.http` dosyasında da var.

**Kullanıcı kaydı**
1. `X-Client: ABC` ile `POST /api/kullanicilar` → `{ "kullaniciAdi": "ayse", "sifre": "Ayse123!" }` → 201.
2. Aynısını `X-Client: XYZ` ile, şifre `Ayse456!` → 201 (farklı firmada aynı ad açılabilir).
3. Tekrar `X-Client: ABC` ile `ayse` → 409 "Bu kullanıcı adı bu firmada zaten kayıtlı."

**Ürün**
4. ABC admin token'ıyla `POST /api/urunler` → `{ "kod": "KLM-1", "ad": "Mavi Kalem", "fiyat": 12.5, "stok": 10 }` → 201. Bir de `{ "kod": "SLG-1", "ad": "Silgi", "fiyat": 5, "stok": 100, "aktifMi": false }` ekle.
5. Aynı kodu tekrar eklemek → 409. XYZ admin aynı kodu ekleyebilir → 201.
6. ABC `ayse` token'ıyla ürün eklemek → 403 (yönetici değil).
7. `GET /api/urunler?arama=Kalem` ABC ile sadece ABC'nin kalemini, XYZ ile sadece XYZ'ninkini döner.

**Sepet**
8. ABC `ayse` token'ıyla `POST /api/sepet/urunler` → `{ "urunId": "<KLM-1 id>", "adet": 3 }` → 200, sepet döner.
9. Stoktan fazla adet → 400 "Yeterli stok yok". Pasif silgi → 400 "satışta değil". XYZ'nin ürün Id'si → 404.
10. `GET /api/sepet` sepeti gösterir. `DELETE /api/sepet/urunler/{urunId}` ürünü çıkarır.

**Sipariş**
11. Sepete tekrar ürün ekleyip `POST /api/siparisler` → 201. Sepet boşalır, ürün stoğu düşer.
12. Boş sepetle sipariş → 400. Ürün sepete eklendikten sonra admin stoğunu düşürür ya da pasif yaparsa sipariş → 400.
13. `GET /api/siparisler` kullanıcının kendi siparişlerini döner; XYZ `ayse` ABC'deki siparişi göremez.

**Yönetim paneli**
14. http://localhost:5100 → firma kodu `ABC`, `admin` / `Abc123!` ile giriş. Siparişler ve Kullanıcılar sekmeleri sadece ABC'nin verisini gösterir.
15. `XYZ`, `admin` / `Xyz123!` ile giriş: ABC'nin siparişleri görünmez.
16. `ayse` ile giriş → "Bu panele sadece firma yöneticileri girebilir."
17. F12 > Network: `login`, `siparisler`, `kullanicilar` isteklerinde `X-Client` ve `Authorization` header'ları görünür.

## 6. gün testleri
| Giriş | Sonuç |
|---|---|
| `X-Client: ABC`, admin / Abc123! | 200, token döndü |
| `X-Client: XYZ`, admin / Abc123! | 401, XYZ'deki admin farklı kullanıcı |
| `X-Client: XYZ`, admin / Xyz123! | 200, token döndü |
| Host ve X-Client olmadan login | 400, firma belirlenemedi |
| abc.localhost üzerinden config isteği | 200, ABC'nin ayarları döndü |
| xyz.localhost üzerinden config isteği | 200, XYZ'nin ayarları döndü |

## Geliştirme süreci
Projeyi Claude Code ile geliştirdim. Kod yazılmadan önce tablo yapısını ve iskeleti planladık. Bu aşamada FirmaId ile bağlama, tek proje yapısı, refresh token ve rollerin şimdilik eklenmemesi gibi kararları verdim. 6. gün senaryolarını Mac'te, Docker üzerindeki SQL Server ile çalıştırıp Swagger'da test ettim. 7. gün için de önce planı çıkarıp onayladım, ardından her adım ayrı commit olarak eklendi.
