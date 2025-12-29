# Development Mode - Hot Reload

Bu dokümantasyon, Docker container'ında çalışan frontend uygulamasında hot reload (anlık değişiklik yansıtma) için gerekli yapılandırmayı açıklar.

## Özellikler

- ✅ **Hot Module Replacement (HMR)**: Kod değişiklikleri anında tarayıcıda görünür
- ✅ **Fast Refresh**: React component'leri hızlıca yenilenir
- ✅ **Volume Mount**: Source code host'tan container'a mount edilir
- ✅ **No Rebuild**: Kod değişikliklerinde container yeniden başlatılmaz

## Kullanım

### 1. Development Container'ını Başlat

```bash
# Local development için (sadece client)
docker-compose -f docker-compose.yml -f docker-compose.local.yml up -d lifeos.client

# Tüm servislerle birlikte
docker-compose -f docker-compose.yml -f docker-compose.local.yml up -d

# Makefile ile (önerilen)
make dev
```

### 2. Uygulamaya Erişim

- **Frontend**: http://localhost:5173
- **API**: http://localhost:6060

### 3. Kod Değişiklikleri

Artık `clients/lifeos-client/src` klasöründeki herhangi bir dosyayı değiştirdiğinizde:

1. Vite otomatik olarak değişikliği algılar
2. HMR devreye girer
3. Tarayıcı otomatik olarak yenilenir (Fast Refresh)
4. Container yeniden başlatılmaz ✅

### 4. Logları İzleme

```bash
# Client container loglarını izle
docker-compose -f docker-compose.yml -f docker-compose.local.yml logs -f lifeos.client

# Makefile ile
make logs-client
```

## Yapılandırma Detayları

### Dockerfile.dev

- Node.js 20 Alpine image kullanır
- Vite dev server çalıştırır (port 5173)
- Source code build edilmez, direkt dev server kullanılır

### Volume Mounts

- `./clients/lifeos-client:/app` - Tüm source code mount edilir
- `lifeos_client_node_modules:/app/node_modules` - node_modules container içinde kalır (daha hızlı)

### Vite Config

- `host: '0.0.0.0'` - Container'dan erişim için
- `watch.usePolling: true` - Docker volume mount için polling kullanır (1 saniye interval)
- `hmr.clientPort: 5173` - HMR için client port ayarı

## Sorun Giderme

### Değişiklikler Yansımıyor

1. Container'ın çalıştığından emin olun:
   ```bash
   docker-compose -f docker-compose.yml -f docker-compose.local.yml ps
   # veya
   make ps
   ```

2. Logları kontrol edin:
   ```bash
   docker-compose -f docker-compose.yml -f docker-compose.local.yml logs lifeos.client
   # veya
   make dev-logs
   ```

3. Container'ı yeniden başlatın:
   ```bash
   docker-compose -f docker-compose.yml -f docker-compose.local.yml restart lifeos.client
   # veya
   make restart
   ```

### Port Çakışması

Eğer 5173 portu kullanılıyorsa, `docker-compose.local.yml` dosyasında port mapping'i değiştirin:

```yaml
ports:
  - "5174:5173" # Farklı bir port kullan
```

Ve `vite.config.ts` dosyasında da HMR client port'unu güncelleyin:

```typescript
hmr: {
  clientPort: 5174 // Yeni port
}
```

### node_modules Sorunları

Eğer dependency sorunları yaşıyorsanız:

```bash
# Container içinde npm install çalıştır
docker-compose -f docker-compose.yml -f docker-compose.local.yml exec lifeos.client npm install

# Veya container'ı rebuild edin
docker-compose -f docker-compose.yml -f docker-compose.local.yml build lifeos.client
docker-compose -f docker-compose.yml -f docker-compose.local.yml up -d lifeos.client

# Makefile ile
make dev-build
```

## Production vs Development

- **Production**: `Dockerfile` kullanılır, nginx ile static build serve edilir
- **Development**: `Dockerfile.dev` kullanılır, Vite dev server çalışır

Production build için:
```bash
docker-compose -f docker-compose.prod.yml up -d
```

