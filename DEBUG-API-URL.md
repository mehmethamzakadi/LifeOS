# API URL Debug

## Sorun
İstek `http://45.143.4.244/api/api/auth/login` şeklinde gidiyor (iki tane `/api`).

## Neden Oluyor?

Axios config'de zaten `/api` ekleniyor:
```typescript
baseURL: `${import.meta.env.VITE_API_URL || 'http://localhost:6060'}/api`,
```

Yani:
- Eğer `VITE_API_URL=http://45.143.4.244/api` ise
- baseURL = `http://45.143.4.244/api/api` ❌ (YANLIŞ!)

- Eğer `VITE_API_URL=http://45.143.4.244` ise  
- baseURL = `http://45.143.4.244/api` ✅ (DOĞRU!)

## Çözüm

`.env` dosyasındaki `VITE_API_URL` değerini kontrol edin:

```bash
grep "^VITE_API_URL" .env
```

**DOĞRU DEĞER:** `VITE_API_URL=http://45.143.4.244` (SONUNDA `/api` OLMAMALI!)

**YANLIŞ DEĞER:** `VITE_API_URL=http://45.143.4.244/api` (BU İKİ TANE `/api/api` YAPAR!)

## Düzeltme

```bash
cd /opt/lifeos

# .env dosyasını düzenleyin
nano .env

# VITE_API_URL satırını şu şekilde ayarlayın:
# VITE_API_URL=http://45.143.4.244

# Client container'ını yeniden build edin
docker-compose -f docker-compose.yml -f docker-compose.prod.yml build --no-cache lifeos.client

# Container'ı yeniden başlatın
docker stop lifeos_client_prod
docker rm lifeos_client_prod
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d lifeos.client
```

