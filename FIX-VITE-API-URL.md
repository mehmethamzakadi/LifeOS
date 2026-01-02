# VITE_API_URL Düzeltme

## Sorun
Frontend build sırasında `VITE_API_URL` yanlış ayarlanmış (`https://api.yourdomain.com`). Bu yüzden API istekleri başarısız oluyor.

## Çözüm

### 1. .env Dosyasını Güncelleyin

Sunucuda `.env` dosyasındaki `VITE_API_URL` değerini güncelleyin:

```bash
cd /opt/lifeos

# .env dosyasını düzenleyin
nano .env

# VITE_API_URL değerini şu şekilde ayarlayın:
# VITE_API_URL=http://45.143.4.244/api
```

### 2. Client Container'ını Yeniden Build Edin

`VITE_API_URL` build-time environment variable olduğu için client container'ını yeniden build etmeniz gerekiyor:

```bash
cd /opt/lifeos

# Sadece client container'ını yeniden build edin
docker-compose -f docker-compose.yml -f docker-compose.prod.yml build --no-cache lifeos.client

# Client container'ını yeniden başlatın
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d --no-deps lifeos.client

# Container durumunu kontrol edin
docker ps | grep client
```

### 3. Test Edin

```bash
# Web sitesini açın
curl http://45.143.4.244/

# Browser console'da CORS hatası olmamalı
```

**Not:** `docker-compose.prod.yml` dosyasındaki default değeri de güncelledim. Ancak `.env` dosyasındaki değer önceliklidir.

