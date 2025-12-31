# 🔥 Hot Reload Kurulumu - LifeOS Backend

## 📋 Genel Bakış

LifeOS backend'i Docker container'ında çalışırken hot reload desteği eklenmiştir. Bu sayede kod değişiklikleri yapıldığında container'ı yeniden başlatmaya gerek kalmadan değişiklikler otomatik olarak algılanır ve uygulama yeniden başlatılır.

## 🛠️ Nasıl Çalışır?

### Teknoloji
- **.NET 9** `dotnet watch` komutu kullanılıyor
- **Docker Volume Mounts** ile source code container'a mount ediliyor
- **File Watching** ile değişiklikler otomatik algılanıyor

### Yapılandırma

1. **Dockerfile.dev**: Development için özel Dockerfile
   - `dotnet/sdk:9.0` image kullanılıyor (runtime değil, SDK gerekli)
   - `dotnet watch run` ile başlatılıyor
   - `DOTNET_USE_POLLING_FILE_WATCHER=true` - Docker volume'larında file watching için

2. **docker-compose.local.yml**: Volume mount yapılandırması
   - `./src:/src/src` - Tüm source code mount ediliyor
   - `bin/obj` klasörleri named volume'da tutuluyor (performans için)

## 🚀 Kullanım

### İlk Kurulum

```bash
# Development ortamını başlat (hot reload ile)
make dev

# Veya sadece API container'ını rebuild et
make dev-rebuild
```

### Normal Kullanım

1. **Container'ı başlat:**
   ```bash
   make dev-up
   ```

2. **Kod değişikliği yap:**
   - Herhangi bir `.cs` dosyasını düzenle
   - `dotnet watch` otomatik olarak değişikliği algılar
   - Uygulama otomatik olarak yeniden başlatılır

3. **Logları izle:**
   ```bash
   make dev-logs
   # veya sadece API logları
   make logs-api
   ```

## ⚙️ Yapılandırma Detayları

### Environment Variables

```yaml
DOTNET_WATCH_RESTART_ON_RUDE_EDIT=true  # Tüm değişikliklerde restart
DOTNET_USE_POLLING_FILE_WATCHER=true    # Docker volume'larında file watching
ASPNETCORE_URLS=http://0.0.0.0:8080     # API endpoint
```

### Volume Mounts

```yaml
volumes:
  # Source code (hot reload için)
  - ./src:/src/src
  
  # Build output'ları (performans için named volume)
  - lifeos_api_bin:/src/src/LifeOS.API/bin
  - lifeos_api_obj:/src/src/LifeOS.API/obj
  
  # Diğer projelerin build output'ları
  - lifeos_infrastructure_bin:/src/src/LifeOS.Infrastructure/bin
  # ... vb.
```

## 🔍 Sorun Giderme

### Hot Reload Çalışmıyor

1. **Container'ı kontrol et:**
   ```bash
   docker ps | grep lifeos_api_dev
   ```

2. **Logları kontrol et:**
   ```bash
   make logs-api
   ```
   `dotnet watch` çıktısını görmelisiniz.

3. **Container'ı yeniden başlat:**
   ```bash
   make dev-rebuild
   ```

### File Watching Çalışmıyor

- Windows'ta Docker Desktop kullanıyorsanız, WSL2 backend'i kullanın
- Volume mount'ların doğru olduğundan emin olun
- `DOTNET_USE_POLLING_FILE_WATCHER=true` environment variable'ının set olduğunu kontrol edin

### Performans Sorunları

- İlk build biraz uzun sürebilir (normal)
- `bin/obj` klasörleri named volume'da tutuluyor (performans için)
- Çok fazla dosya değişikliği yaparsanız, container'ı yeniden başlatmak daha hızlı olabilir

## 📝 Notlar

- **Hot Reload** sadece development ortamında aktif
- Production'da normal Dockerfile kullanılır (hot reload yok)
- Bazı değişiklikler (ör. Program.cs, appsettings.json) tam restart gerektirebilir
- Migration değişiklikleri için container'ı yeniden başlatmak gerekebilir

## 🎯 Desteklenen Değişiklikler

✅ **Otomatik Algılanır:**
- Handler'lar (.cs dosyaları)
- Endpoint'ler
- Service'ler
- Entity'ler
- Configuration'lar

⚠️ **Manuel Restart Gerekebilir:**
- Program.cs değişiklikleri
- appsettings.json değişiklikleri
- Migration'lar
- NuGet package ekleme/çıkarma

## 🔗 İlgili Dosyalar

- `src/LifeOS.API/Dockerfile.dev` - Development Dockerfile
- `docker-compose.local.yml` - Docker Compose yapılandırması
- `Makefile` - Komutlar

