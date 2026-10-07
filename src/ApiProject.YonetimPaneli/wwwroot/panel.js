// API'nin adresi. ApiProject.Api farklı bir portta çalışıyorsa burayı değiştirin.
const API_ADRESI = "http://localhost:5000";

// Giriş bilgileri sekme kapanana kadar sessionStorage'da tutulur, sayfa yenilenince tekrar giriş gerekmez.
const OTURUM_ANAHTARI = "yonetimOturumu";

function oturumuOku() {
  const kayit = sessionStorage.getItem(OTURUM_ANAHTARI);
  return kayit ? JSON.parse(kayit) : null;
}

function oturumuKaydet(oturum) {
  sessionStorage.setItem(OTURUM_ANAHTARI, JSON.stringify(oturum));
}

function oturumuSil() {
  sessionStorage.removeItem(OTURUM_ANAHTARI);
}

// Tüm istekler buradan geçer. Firma kodu X-Client header'ıyla, token Authorization header'ıyla gönderilir.
// Tarayıcıda F12 > Network sekmesinde bu istekler ve header'ları görülebilir.
async function apiIstegi(yol, secenekler = {}) {
  const oturum = oturumuOku();
  const headerlar = { "Content-Type": "application/json" };
  const firmaKodu = secenekler.firmaKodu ?? oturum?.firmaKodu;
  if (firmaKodu) headerlar["X-Client"] = firmaKodu;
  if (oturum?.token) headerlar["Authorization"] = "Bearer " + oturum.token;

  const yanit = await fetch(API_ADRESI + yol, {
    method: secenekler.method ?? "GET",
    headers: headerlar,
    body: secenekler.govde ? JSON.stringify(secenekler.govde) : undefined,
  });

  const veri = await yanit.json().catch(() => null);
  if (!yanit.ok) {
    const mesaj = veri?.hata ?? hataMesaji(yanit.status);
    throw new Error(mesaj);
  }
  return veri;
}

function hataMesaji(durumKodu) {
  if (durumKodu === 401) return "Oturum süresi doldu, tekrar giriş yapın.";
  if (durumKodu === 403) return "Bu işlem için yetkiniz yok.";
  return "Beklenmeyen bir hata oluştu (" + durumKodu + ").";
}

// ---------- Giriş ----------

document.getElementById("girisFormu").addEventListener("submit", async (olay) => {
  olay.preventDefault();
  const hataAlani = document.getElementById("girisHatasi");
  hataAlani.textContent = "";

  const firmaKodu = document.getElementById("firmaKodu").value.trim().toUpperCase();
  const kullaniciAdi = document.getElementById("kullaniciAdi").value.trim();
  const sifre = document.getElementById("sifre").value;

  try {
    oturumuSil();
    const yanit = await apiIstegi("/api/auth/login", {
      method: "POST",
      firmaKodu: firmaKodu,
      govde: { kullaniciAdi: kullaniciAdi, sifre: sifre },
    });

    if (!yanit.yoneticiMi) {
      hataAlani.textContent = "Bu panele sadece firma yöneticileri girebilir.";
      return;
    }

    oturumuKaydet({ token: yanit.token, firmaKodu: yanit.firmaKodu, kullaniciAdi: yanit.kullaniciAdi });
    paneliGoster();
  } catch (hata) {
    hataAlani.textContent = hata.message;
  }
});

document.getElementById("cikisButonu").addEventListener("click", () => {
  oturumuSil();
  girisiGoster();
});

// ---------- Ekranlar ----------

function girisiGoster() {
  document.getElementById("panelEkrani").hidden = true;
  document.getElementById("girisEkrani").hidden = false;
}

function paneliGoster() {
  const oturum = oturumuOku();
  document.getElementById("girisEkrani").hidden = true;
  document.getElementById("panelEkrani").hidden = false;
  document.getElementById("firmaBasligi").textContent = "Firma: " + oturum.firmaKodu;
  document.getElementById("kullaniciBasligi").textContent = oturum.kullaniciAdi;
  sekmeyiAc("siparisler");
}

document.querySelectorAll(".sekmeler button").forEach((buton) => {
  buton.addEventListener("click", () => sekmeyiAc(buton.dataset.sekme));
});

function sekmeyiAc(sekme) {
  document.querySelectorAll(".sekmeler button").forEach((b) => b.classList.toggle("aktif", b.dataset.sekme === sekme));
  document.querySelectorAll(".sekme").forEach((s) => (s.hidden = s.id !== sekme));
  document.getElementById("panelHatasi").textContent = "";

  if (sekme === "siparisler") siparisleriYukle();
  else if (sekme === "kullanicilar") kullanicilariYukle();
  else document.getElementById("aramaMetni").focus();
}

// ---------- Siparişler ----------

async function siparisleriYukle() {
  const tablo = document.getElementById("siparisTablosu");
  try {
    const siparisler = await apiIstegi("/api/yonetim/siparisler");
    tablo.innerHTML = "";
    if (siparisler.length === 0) {
      tablo.innerHTML = '<tr><td colspan="4" class="bos">Henüz sipariş yok.</td></tr>';
      return;
    }
    for (const siparis of siparisler) {
      tablo.appendChild(satir([
        tarih(siparis.olusturmaTarihi),
        yazi(siparis.kullaniciAdi),
        urunlerHucresi(siparis.kalemler),
        para(siparis.toplamTutar),
      ]));
    }
  } catch (hata) {
    hataGoster(hata);
  }
}

function urunlerHucresi(kalemler) {
  const hucre = document.createElement("td");
  for (const kalem of kalemler) {
    const div = document.createElement("div");
    div.textContent = kalem.adet + " x " + kalem.urunAdi + " (" + paraYazisi(kalem.birimFiyat) + ")";
    hucre.appendChild(div);
  }
  return hucre;
}

// ---------- Kullanıcılar ----------

async function kullanicilariYukle() {
  const tablo = document.getElementById("kullaniciTablosu");
  try {
    const kullanicilar = await apiIstegi("/api/yonetim/kullanicilar");
    tablo.innerHTML = "";
    for (const k of kullanicilar) {
      tablo.appendChild(satir([
        yazi(k.kullaniciAdi),
        yazi(k.adSoyad ?? "-"),
        yazi(k.email ?? "-"),
        yazi(k.yoneticiMi ? "Evet" : "Hayır"),
        yazi(k.aktifMi ? "Evet" : "Hayır"),
        tarih(k.olusturmaTarihi),
      ]));
    }
  } catch (hata) {
    hataGoster(hata);
  }
}

// ---------- Ürün arama ----------

document.getElementById("aramaFormu").addEventListener("submit", async (olay) => {
  olay.preventDefault();
  const metin = document.getElementById("aramaMetni").value.trim();
  const tablo = document.getElementById("urunTablosu");
  document.getElementById("panelHatasi").textContent = "";

  try {
    const sonuc = await apiIstegi("/api/urunler?sadeceAktif=false&arama=" + encodeURIComponent(metin));

    document.getElementById("aramaToplam").textContent = sonuc.toplamKayit.toLocaleString("tr-TR");
    document.getElementById("aramaGosterilen").textContent =
      sonuc.toplamKayit > sonuc.sonuclar.length ? " (ilk " + sonuc.sonuclar.length + " gösteriliyor)" : "";
    document.getElementById("aramaSure").textContent = sonuc.sureMs;
    const kaynak = document.getElementById("aramaKaynak");
    kaynak.textContent = sonuc.kaynak;
    kaynak.className = "etiket " + sonuc.kaynak;
    document.getElementById("aramaBilgisi").hidden = false;

    tablo.innerHTML = "";
    if (sonuc.sonuclar.length === 0) {
      tablo.innerHTML = '<tr><td colspan="5" class="bos">Ürün bulunamadı.</td></tr>';
      return;
    }
    for (const u of sonuc.sonuclar) {
      const stok = yazi(u.stok.toLocaleString("tr-TR"));
      stok.className = "sayi";
      tablo.appendChild(satir([yazi(u.kod), yazi(u.ad), para(u.fiyat), stok, yazi(u.aktifMi ? "Evet" : "Hayır")]));
    }
  } catch (hata) {
    hataGoster(hata);
  }
});

// ---------- Yardımcılar ----------

function hataGoster(hata) {
  document.getElementById("panelHatasi").textContent = hata.message;
  if (hata.message.startsWith("Oturum")) {
    oturumuSil();
    girisiGoster();
  }
}

// Veriler textContent ile yazılır, böylece gelen metin HTML olarak çalıştırılmaz.
function satir(hucreler) {
  const tr = document.createElement("tr");
  hucreler.forEach((h) => tr.appendChild(h));
  return tr;
}

function yazi(metin) {
  const td = document.createElement("td");
  td.textContent = metin;
  return td;
}

// Tarihler veritabanında UTC tutulur ama API saat dilimi eki olmadan döner; "Z" ekleyip yerel saate çeviriyoruz.
function tarih(deger) {
  const utc = deger.endsWith("Z") ? deger : deger + "Z";
  return yazi(new Date(utc).toLocaleString("tr-TR"));
}

function para(deger) {
  const td = yazi(paraYazisi(deger));
  td.className = "sayi";
  return td;
}

function paraYazisi(deger) {
  return deger.toLocaleString("tr-TR", { style: "currency", currency: "TRY" });
}

// ---------- Başlangıç ----------

if (oturumuOku()) paneliGoster();
else girisiGoster();
