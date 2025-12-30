# LifeOS - Geliştirme Notları

Bu dosya proje geliştirme sürecinde tutulan notları içerir.

---

## 📝 Yeni Modül Ekleme Notları

### Bookshelf Modülü

**Tarih:** Eski notlar  
**Durum:** Tamamlandı (referans için tutuluyor)

Yeni bir admin modülü eklemek istiyorum.
- Modül adı: Bookshelf
- Amaç: Okunan kitap kayıtlarını (kitap adı, yazar, yayınevi, sayfa sayısı, okundu/okunmadı, not, okunma tarihi) yönetmek.
- Sadece admin rolü erişebilsin.
- API'de CRUD uçları, DTO'lar, validasyonlar ve filtreleme destekli listeleme olsun.
- Admin panelinde menüye yeni sayfa ekle (clients/lifeos-client içindeki mevcut admin arayüzü kullanılıyor).
- Gerekli izinleri (Permissions.*) ve seed'lerini güncelle.
- Gerekli migration, repository, service ve controller katmanlarını projedeki mevcut mimariye sadık kalarak oluştur.
- Outbox pattern yapısına dahil et.
- Ünite testleri ekleyip çalıştır.

Her adımda dosya yollarını ve yapılan değişiklikleri detaylı anlat, gerekli komutları da paylaş.

---

## 🐳 Docker Komutları

### Temel Docker Compose Komutları

```bash
# Tüm servisleri durdur
docker compose -f docker-compose.yml -f docker-compose.local.yml down

# Tüm servisleri başlat (build ile)
docker compose -f docker-compose.yml -f docker-compose.local.yml up --build
```

**Not:** Detaylı Docker kullanımı için [README_MAKEFILE.md](./README_MAKEFILE.md) dosyasına bakın.

---

## 🗄️ Migration Komutları

### Entity Framework Core Migration

#### Paket Yöneticisi Konsolu (Visual Studio)

1. Package Manager Console üzerinde **Default Project** → `src\LifeOS.Persistence` seçili olmalıdır.

2. Migration ekleme:
```powershell
add-migration Init -C LifeOSDbContext -O Migrations/PostgreSql
```

3. Migration uygulama:
```powershell
update-database -C LifeOSDbContext
```

#### .NET CLI

```bash
# Migration ekleme
dotnet ef migrations add Init -c LifeOSDbContext -o Migrations/PostgreSql -p src/LifeOS.Persistence -s src/LifeOS.API

# Migration uygulama
dotnet ef database update -c LifeOSDbContext -p src/LifeOS.Persistence -s src/LifeOS.API
```

**Not:** Detaylı migration komutları için [README_MAKEFILE.md](./README_MAKEFILE.md) dosyasına bakın.

---

**Son Güncelleme:** Aralık 2025

