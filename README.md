# Projesi

Bu proje, müþteri verilerinin yönetilmesini saðlayan temel bir Müþteri Ýliþkileri Yönetimi (CRM) sistemi olarak tasarlanmýþtýr.  
Kullanýcýlarýn kimlik doðrulamasý ve rol tabanlý yetkilendirme (Admin/User) özelliklerini içerir. Müþteriler listelenebilir, filtrelenebilir ve CRUD (Oluþturma, Okuma, Güncelleme, Silme) iþlemleri gerçekleþtirilebilir.

---

## Kullanýlan Teknolojiler

### Backend
- .NET 8 (ASP.NET Core), C#
- RESTful API Tasarýmý
- Entity Framework Core (PostgreSQL için ORM)
- JWT (JSON Web Tokens) ile Kimlik Doðrulama
- Rol Tabanlý Yetkilendirme
- ASP.NET Core Identity PasswordHasher (Güvenli Þifreleme)
- Katmanlý Mimari (Repository Pattern, Servis Katmaný)
- DTO'lar (Data Transfer Objects) ve AutoMapper ile Veri Transferi
- `ILogger` ile Loglama

### Veritabaný
- PostgreSQL

### Frontend
- ASP.NET Core Razor Pages
- Cookie Tabanlý Oturum Yönetimi (Razor Pages tarafýnda)

---

## Geliþtirme Süreci

Ýlk olarak veritabaný þemasý ve entity'ler tasarlanmýþ, ardýndan Repository ve Servis katmanlarý oluþturularak backend API'leri (müþteri CRUD, kullanýcý kayýt ve giriþ) implemente edilmiþtir.  

Kimlik doðrulama için JWT, þifreleme için ASP.NET Core Identity PasswordHasher kullanýlmýþtýr. Rol tabanlý yetkilendirme ile farklý kullanýcý tiplerinin (Admin, User) yetkileri belirlenmiþtir.  

Son aþamada ise ASP.NET Core Razor Pages kullanýlarak kullanýcý arayüzü oluþturulmuþ, backend API'leri ile entegrasyon saðlanmýþ ve temel CRM iþlevleri (müþteri listeleme, filtreleme, ekleme, düzenleme, silme) kullanýcýya sunulmuþtur.

---

