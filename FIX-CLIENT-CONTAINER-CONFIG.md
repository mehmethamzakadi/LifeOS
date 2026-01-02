# Client Container ContainerConfig Hatası - Çözüm

## Sorun
Client container'ını yeniden başlatırken `ContainerConfig` hatası alıyorsunuz.

## Çözüm

Sunucuda şu komutları çalıştırın:

```bash
cd /opt/lifeos

# 1. Client container'ını durdur ve sil
docker-compose -f docker-compose.yml -f docker-compose.prod.yml stop lifeos.client
docker-compose -f docker-compose.yml -f docker-compose.prod.yml rm -f lifeos.client

# VEYA manuel olarak:
docker stop lifeos_client_prod
docker rm lifeos_client_prod

# 2. .env dosyasındaki VITE_API_URL değerini kontrol edin (DOĞRU DEĞER: http://45.143.4.244/api)
grep "^VITE_API_URL" .env

# Eğer yanlışsa düzeltin:
# nano .env
# VITE_API_URL=http://45.143.4.244/api  (ÖNEMLİ: /api path'i olmalı!)

# 3. Client container'ını yeniden build edin
docker-compose -f docker-compose.yml -f docker-compose.prod.yml build --no-cache lifeos.client

# 4. Client container'ını başlatın
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d lifeos.client

# 5. Container durumunu kontrol edin
docker ps | grep client
```

**ÖNEMLİ NOT:** 
- `VITE_API_URL=http://45.143.4.244/api` şeklinde olmalı (sonunda `/api` olmalı!)
- Çünkü nginx üzerinden `/api` path'i API'ye yönlendiriliyor
- `http://45.143.4.244/api/auth/login` -> nginx -> `lifeos.api:8080/api/auth/login`

