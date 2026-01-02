# JWT Token Audience/Issuer Hatası - Çözüm

## Sorun
```
Bearer was not authenticated. Failure message: IDX10208: Unable to validate audience. validationParameters.ValidAudience is null or whitespace and validationParameters.ValidAudiences is null.
```

## Neden
`docker-compose.prod.yml` dosyasında `TokenOptions__Audience` ve `TokenOptions__Issuer` environment variable'ları eksikti. Bu yüzden JWT token validation başarısız oluyordu.

## Çözüm
`docker-compose.prod.yml` dosyasına şu environment variable'lar eklendi:
- `TokenOptions__Audience: ${APP_URL:-http://45.143.4.244}`
- `TokenOptions__Issuer: ${APP_URL:-http://45.143.4.244}`

## Deploy Adımları

```bash
cd /opt/lifeos

# 1. API container'ını durdur
docker stop lifeos_api_prod
docker rm lifeos_api_prod

# 2. Container'ı yeniden başlat (yeni environment variable'lar ile)
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d lifeos.api

# 3. Container durumunu kontrol et
docker ps | grep api

# 4. Logları kontrol et
docker logs -f lifeos_api_prod | grep -i "authentication\|jwt\|bearer"
```

## Test
1. Browser'da login olun
2. Herhangi bir sayfaya gidin (kategoriler, roller, vs.)
3. 401 hatası almamalısınız

