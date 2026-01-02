# Authorization Handler Fix - Deploy

## Sorun
JWT token'da permission claim'leri array olarak serialize edilmiş, ama authorization handler bunları düzgün parse edemiyordu.

## Çözüm
Authorization handler güncellendi - artık hem array hem de ayrı claim'leri destekliyor.

## Deploy Adımları

```bash
cd /opt/lifeos

# 1. API'yi yeniden build edin
docker-compose -f docker-compose.yml -f docker-compose.prod.yml build --no-cache lifeos.api

# 2. API container'ını yeniden başlatın
docker stop lifeos_api_prod
docker rm lifeos_api_prod
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d lifeos.api

# 3. Container durumunu kontrol edin
docker ps | grep api

# 4. Logları izleyin
docker logs -f lifeos_api_prod
```

## Test
1. Browser'da login olun
2. Kategoriler veya roller sayfasına gidin
3. 401 hatası almamalısınız

