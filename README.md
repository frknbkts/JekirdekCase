# Projesi

Bu proje, m��teri verilerinin y�netilmesini sa�layan temel bir M��teri �li�kileri Y�netimi (CRM) sistemi olarak tasarlanm��t�r.  
Kullan�c�lar�n kimlik do�rulamas� ve rol tabanl� yetkilendirme (Admin/User) �zelliklerini i�erir. M��teriler listelenebilir, filtrelenebilir ve CRUD (Olu�turma, Okuma, G�ncelleme, Silme) i�lemleri ger�ekle�tirilebilir.

---

## Kullan�lan Teknolojiler

### Backend
- .NET 8 (ASP.NET Core), C#
- RESTful API Tasar�m�
- Entity Framework Core (PostgreSQL i�in ORM)
- JWT (JSON Web Tokens) ile Kimlik Do�rulama
- Rol Tabanl� Yetkilendirme
- ASP.NET Core Identity PasswordHasher (G�venli �ifreleme)
- Katmanl� Mimari (Repository Pattern, Servis Katman�)
- DTO'lar (Data Transfer Objects) ve AutoMapper ile Veri Transferi
- `ILogger` ile Loglama

### Veritaban�
- PostgreSQL

### Frontend
- ASP.NET Core Razor Pages
- Cookie Tabanl� Oturum Y�netimi (Razor Pages taraf�nda)

---

## Geli�tirme S�reci

�lk olarak veritaban� �emas� ve entity'ler tasarlanm��, ard�ndan Repository ve Servis katmanlar� olu�turularak backend API'leri (m��teri CRUD, kullan�c� kay�t ve giri�) implemente edilmi�tir.  

Kimlik do�rulama i�in JWT, �ifreleme i�in ASP.NET Core Identity PasswordHasher kullan�lm��t�r. Rol tabanl� yetkilendirme ile farkl� kullan�c� tiplerinin (Admin, User) yetkileri belirlenmi�tir.  

Son a�amada ise ASP.NET Core Razor Pages kullan�larak kullan�c� aray�z� olu�turulmu�, backend API'leri ile entegrasyon sa�lanm�� ve temel CRM i�levleri (m��teri listeleme, filtreleme, ekleme, d�zenleme, silme) kullan�c�ya sunulmu�tur.

---

