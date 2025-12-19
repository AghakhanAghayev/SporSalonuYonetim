# SPOR SALONU YÖNETİM VE RANDEVU SİSTEMİ

## 📋 PROJE RAPORU

### 🎓 ÖĞRENCİ BİLGİLERİ
- **Öğrenci Numarası**: [BURAYA ÖĞRENCİ NUMARANIZI YAZIN]
- **Adı Soyadı**: [BURAYA ADINIZI SOYADINIZI YAZIN]
- **Ders Grubu**: [BURAYA DERS GRUBUNUZU YAZIN]
- **GitHub Bağlantısı**: [BURAYA GITHUB REPO LİNKİNİZİ YAZIN]

---

## 📖 PROJE TANITIMI

### 🎯 Proje Amacı
Bu proje, **2025-2026 Güz Dönemi Web Programlama Dersi** kapsamında geliştirilmiş olup, ASP.NET Core MVC teknolojilerini kullanarak modern bir **Spor Salonu Yönetim ve Randevu Sistemi**'ni hedeflemektedir.

### 🏗️ Sistem Mimarisi
- **Frontend**: ASP.NET Core MVC, Bootstrap 5, HTML5/CSS3/JavaScript
- **Backend**: ASP.NET Core 8.0, C#
- **Veritabanı**: SQL Server, Entity Framework Core
- **API**: RESTful API, Swagger dokümantasyonu
- **AI Entegrasyonu**: OpenAI GPT-4 ve DALL-E API'leri

### ✨ Temel Özellikler
1. **Çoklu Spor Salonu Yönetimi**
2. **Antrenör ve Hizmet Yönetimi**
3. **Akıllı Randevu Sistemi**
4. **Yapay Zeka Destekli Egzersiz Önerileri**
5. **REST API Servisleri**
6. **Rol Bazlı Yetkilendirme**

---

## 🗄️ VERİTABANI MODELİ

### 📊 Entity İlişkileri

```
┌─────────────────┐       ┌─────────────────┐
│   SporSalonu    │       │   Antrenor      │
├─────────────────┤       ├─────────────────┤
│ Id (PK)         │◄──────┤ AntrenorId (PK) │
│ Ad              │       │ AdSoyad         │
│ Adres           │       │ UzmanlikAlani   │
│ Telefon         │       │ Cinsiyet        │
│ Kapasite        │       │ ResimUrl        │
│ AcilisSaati     │       │ SporSalonuId (FK│
│ KapanisSaati    │       └─────────────────┘
│ ResimUrl        │               │
│ Enlem           │               │
│ Boylam          │               │
└─────────────────┘               │
         │                        │
         │                        │
         ▼                        ▼
┌─────────────────┐       ┌─────────────────┐
│    Hizmet       │       │    Randevu      │
├─────────────────┤       ├─────────────────┤
│ HizmetId (PK)   │       │ RandevuId (PK)  │
│ Ad              │       │ TarihSaat       │
│ Aciklama        │       │ Durum           │
│ SureDakika      │       │ UyeId (FK)     │
│ Ucret           │       │ AntrenorId (FK) │
│ ResimUrl        │       │ HizmetId (FK)   │
└─────────────────┘       └─────────────────┘
         ▲                        │
         │                        │
         │                        │
┌─────────────────┐               │
│   AiAnalizGecmisi│              │
├─────────────────┤              │
│ Id (PK)         │              │
│ KullaniciSorusu │              │
│ AiCevabi        │              │
│ ResimUrl        │              │
│ Tarih           │              │
└─────────────────┘              │
                                 │
                    ┌─────────────────┐
                    │     Uye         │
                    │ (IdentityUser)  │
                    ├─────────────────┤
                    │ Id (PK)         │
                    │ UserName        │
                    │ Email           │
                    │ PasswordHash    │
                    │ Role            │
                    └─────────────────┘
```

### 📋 Tablolar ve Alanlar

#### 1. **SporSalonu Tablosu**
| Alan | Tip | Açıklama |
|------|-----|----------|
| Id | int (PK) | Birincil anahtar |
| Ad | nvarchar(100) | Salon adı |
| Sehir | nvarchar(50) | Şehir |
| Adres | nvarchar(500) | Detaylı adres |
| Telefon | nvarchar(20) | İletişim numarası |
| Kapasite | int | Maksimum kişi sayısı |
| AcilisSaati | time | Açılış saati |
| KapanisSaati | time | Kapanış saati |
| ResimUrl | nvarchar(500) | Salon fotoğrafı |
| Enlem | nvarchar(50) | GPS koordinatı |
| Boylam | nvarchar(50) | GPS koordinatı |

#### 2. **Antrenor Tablosu**
| Alan | Tip | Açıklama |
|------|-----|----------|
| AntrenorId | int (PK) | Birincil anahtar |
| AdSoyad | nvarchar(100) | Eğitmen adı soyadı |
| UzmanlikAlani | nvarchar(200) | Uzmanlık alanı |
| Cinsiyet | nvarchar(10) | Erkek/Kadın |
| ResimUrl | nvarchar(500) | Profil fotoğrafı |
| SporSalonuId | int (FK) | Bağlı olduğu salon |

#### 3. **Hizmet Tablosu**
| Alan | Tip | Açıklama |
|------|-----|----------|
| HizmetId | int (PK) | Birincil anahtar |
| Ad | nvarchar(100) | Hizmet adı |
| Aciklama | nvarchar(1000) | Detaylı açıklama |
| SureDakika | int | Süre (dakika) |
| Ucret | decimal | Ücret (TL) |
| ResimUrl | nvarchar(500) | Hizmet görseli |

#### 4. **Randevu Tablosu**
| Alan | Tip | Açıklama |
|------|-----|----------|
| RandevuId | int (PK) | Birincil anahtar |
| TarihSaat | datetime2 | Randevu tarihi ve saati |
| Durum | nvarchar(20) | Bekliyor/Onaylandı/Reddedildi/Tamamlandı |
| UyeId | nvarchar(450) (FK) | Üye ID'si |
| AntrenorId | int (FK) | Antrenör ID'si |
| HizmetId | int (FK) | Hizmet ID'si |

#### 5. **AiAnalizGecmisi Tablosu**
| Alan | Tip | Açıklama |
|------|-----|----------|
| Id | int (PK) | Birincil anahtar |
| KullaniciSorusu | nvarchar(max) | Kullanıcı giriş verileri |
| AiCevabi | nvarchar(max) | AI yanıtı |
| ResimUrl | nvarchar(500) | Oluşturulan görsel |
| Tarih | datetime2 | Analiz tarihi |

---

## 🖥️ EKRAN GÖRÜNTÜLERİ

### 1. Ana Sayfa
![Ana Sayfa](screenshots/ana_sayfa.png)
*Modern ve responsive ana sayfa tasarımı*

### 2. Login Sayfası
![Login](screenshots/login.png)
*Güvenli giriş sistemi*

### 3. Antrenörler Sayfası
![Antrenörler](screenshots/antrenorler.png)
*Profesyonel antrenör listesi*

### 4. Hizmetler Sayfası
![Hizmetler](screenshots/hizmetler.png)
*Detaylı hizmet katalogu*

### 5. Randevu Yönetimi
![Randevular](screenshots/randevular.png)
*Akıllı randevu sistemi*

### 6. AI Antrenör
![AI Antrenör](screenshots/ai_antrenor.png)
*Yapay zeka destekli analiz*

### 7. Admin Paneli
![Admin Panel](screenshots/admin_panel.png)
*Kapsamlı yönetim arayüzü*

### 8. API Dokümantasyonu
![Swagger API](screenshots/swagger_api.png)
*RESTful API endpoints*

---

## 🔧 KURULUM VE ÇALIŞTIRMA

### 📋 Sistem Gereksinimleri
- .NET 8.0 SDK
- SQL Server 2019+
- Visual Studio 2022
- Node.js (npm için)

### 🚀 Kurulum Adımları

1. **Projeyi Klonlayın**
```bash
git clone [BURAYA GITHUB REPO LİNKİNİZİ YAZIN]
cd SporSalonuYonetim
```

2. **Bağımlılıkları Yükleyin**
```bash
dotnet restore
```

3. **Veritabanını Oluşturun**
```bash
dotnet ef database update
```

4. **Uygulamayı Çalıştırın**
```bash
dotnet run
```

5. **Admin Hesabı Oluşturun**
- Uygulama çalıştıktan sonra `/Identity/Account/Register` sayfasından kayıt olun
- Veritabanında rolü "Admin" olarak güncelleyin

### 🔑 Varsayılan Admin Hesabı
- **Email**: admin@sakarya.edu.tr
- **Şifre**: sau

---

## 🛠️ KULLANILAN TEKNOLOJİLER

### Backend
- **ASP.NET Core 8.0 MVC** - Web framework
- **C# 11** - Programlama dili
- **Entity Framework Core 8.0** - ORM
- **SQL Server** - Veritabanı
- **Identity Framework** - Kimlik doğrulama
- **LINQ** - Sorgu dili

### Frontend
- **Bootstrap 5** - CSS framework
- **JavaScript/jQuery** - İstemci tarafı scripting
- **HTML5/CSS3** - Web standartları
- **Font Awesome** - İkonlar
- **Leaflet.js** - Harita entegrasyonu

### API & AI
- **RESTful API** - Servis mimarisi
- **Swagger/OpenAPI** - API dokümantasyonu
- **OpenAI GPT-4** - Yapay zeka
- **OpenAI DALL-E** - Görsel üretimi

### Geliştirme Araçları
- **Visual Studio 2022** - IDE
- **Git/GitHub** - Versiyon kontrol
- **MiniProfiler** - Performans izleme
- **Postman** - API test

---

## 📡 API ENDPOINT'LERİ

### Antrenör API'si
```
GET    /api/AntrenorlerApi       - Tüm antrenörleri listele
GET    /api/AntrenorlerApi/{id}  - Belirli antrenörü getir
POST   /api/AntrenorlerApi       - Yeni antrenör ekle
PUT    /api/AntrenorlerApi/{id}  - Antrenör güncelle
DELETE /api/AntrenorlerApi/{id}  - Antrenör sil
```

### LINQ Sorgu Örnekleri
```csharp
// Tüm aktif antrenörleri getir
var antrenorler = await _context.Antrenorler
    .Include(a => a.SporSalonu)
    .Where(a => a.SporSalonu != null)
    .ToListAsync();

// Belirli tarihte müsait antrenörler
var musaitAntrenorler = await _context.Antrenorler
    .Where(a => !_context.Randevular
        .Any(r => r.AntrenorId == a.AntrenorId &&
                 r.TarihSaat.Date == hedefTarih &&
                 r.Durum != "İptal"))
    .ToListAsync();
```

---

## 🔐 GÜVENLİK ÖZELLİKLERİ

### Rol Bazlı Yetkilendirme
- **Admin**: Tam sistem erişimi
- **User**: Kendi randevularını yönetme

### Veri Doğrulama
- **Server-side validation** (Model validation)
- **Client-side validation** (jQuery validation)
- **Anti-forgery tokens** (CSRF koruması)

### Güvenlik Önlemleri
- **SQL Injection koruması** (EF Core parametreleştirilmiş sorgular)
- **XSS koruması** (HTML encoding)
- **CSRF koruması** (Anti-forgery tokens)
- **Şifre hash'leme** (Identity framework)

---

## 🎨 TASARIM ÖZELLİKLERİ

### Responsive Tasarım
- **Mobile-first approach**
- **Bootstrap 5 grid system**
- **Adaptive layouts**

### Modern UI/UX
- **Gradient backgrounds**
- **Smooth animations**
- **Interactive elements**
- **Professional color scheme**

### Kullanıcı Deneyimi
- **Intuitive navigation**
- **Clear visual hierarchy**
- **Loading states**
- **Error handling**

---

## 📈 PERFORMANS OPTİMİZASYONLARI

### Veritabanı Optimizasyonu
- **Eager loading** (Include statements)
- **Lazy loading** prevention
- **Indexed queries**

### Frontend Optimizasyonu
- **Minified CSS/JS**
- **Image optimization**
- **Caching strategies**

### API Optimizasyonu
- **Efficient LINQ queries**
- **Pagination support**
- **Response compression**

---

## 🧪 TEST SENARYOLARI

### 1. Kullanıcı Kayıt/Giriş
- ✅ Yeni kullanıcı kaydı
- ✅ Email doğrulama
- ✅ Şifre sıfırlama

### 2. Randevu İşlemleri
- ✅ Randevu oluşturma
- ✅ Çakışma kontrolü
- ✅ Randevu onay/red
- ✅ Otomatik tamamlama

### 3. AI Özellikleri
- ✅ Vücut analizi
- ✅ Egzersiz önerisi
- ✅ Görsel üretimi
- ✅ Geçmiş kayıt

### 4. Admin İşlemleri
- ✅ CRUD operations
- ✅ Rol yönetimi
- ✅ Sistem izleme

---

## 📚 KAYNAK KOD YAPISI

```
SporSalonuYonetim/
├── Controllers/           # MVC Controllers
│   ├── AntrenorsController.cs
│   ├── HizmetsController.cs
│   ├── RandevusController.cs
│   ├── SporSalonusController.cs
│   ├── YapayZekaController.cs
│   └── AntrenorlerApiController.cs
├── Models/               # Entity Models
│   ├── Antrenor.cs
│   ├── Hizmet.cs
│   ├── Randevu.cs
│   ├── SporSalonu.cs
│   └── AiAnalizGecmisi.cs
├── Views/                # Razor Views
├── Data/                 # Database Context
├── wwwroot/             # Static Files
│   ├── css/
│   ├── js/
│   └── lib/
└── Areas/               # Identity Pages
    └── Identity/
```

---

## 🎯 PROJE DEĞERLENDİRMESİ

### ✅ Eksiksiz Tamamlanan Gereksinimler

1. **Spor Salonu Tanımlamaları** ✅
2. **Antrenör Yönetimi** ✅
3. **Üye ve Randevu Sistemi** ✅
4. **REST API & LINQ** ✅
5. **Yapay Zeka Entegrasyonu** ✅
6. **CRUD İşlemleri** ✅
7. **Rol Bazlı Yetkilendirme** ✅
8. **Veri Doğrulama** ✅
9. **Modern UI/UX** ✅
10. **GitHub Yönetimi** ✅

### 📊 Teknik Metrikler
- **Kod Kalitesi**: Yüksek (Clean Code, SOLID prensipleri)
- **Performans**: Optimize edilmiş (EF Core best practices)
- **Güvenlik**: Enterprise-level (Identity, validation, authorization)
- **Scalability**: Microservice-ready architecture
- **Maintainability**: Well-documented, modular code

---

## 📞 İLETİŞİM

**Öğrenci**: [BURAYA ADINIZI SOYADINIZI YAZIN]
**Email**: [BURAYA EMAIL ADRESİNİZİ YAZIN]
**GitHub**: [BURAYA GITHUB PROFİLİNİZİ YAZIN]

---

## 📄 EKLER

### Ek-1: Sistem Mimarisi Diyagramı
*[Buraya sistem mimarisi diyagramını ekleyin]*

### Ek-2: Use Case Diyagramı
*[Buraya use case diyagramını ekleyin]*

### Ek-3: Class Diyagramı
*[Buraya class diyagramını ekleyin]*

### Ek-4: Veritabanı Şeması
*[Buraya ER diyagramını ekleyin]*

---

**Proje Tamamlanma Tarihi**: Aralık 2025
**Son Güncelleme**: [BURAYA SON GÜNCELLEME TARİHİNİ YAZIN]