# Sunucuda Container'ları Başlatma Komutları

## Sorun
`make prod-build` sadece image'ları build eder, container'ları başlatmaz. Bu yüzden uygulama çalışmıyor.

## Çözüm - Container'ları Başlatın

Sunucuda şu komutları çalıştırın:

```bash
# Sunucuya bağlanın
ssh lifeos@45.143.4.244

# Proje dizinine gidin
cd /opt/lifeos

# Container'ları başlatın (build edilen image'ları kullanarak)
make prod-up

# VEYA hem build hem de başlatmak için:
make prod
```

## Container Durumunu Kontrol Edin

```bash
# Çalışan container'ları listeleyin
docker ps

# VEYA Makefile ile
make ps

# Container durumlarını detaylı görmek için
docker-compose -f docker-compose.yml -f docker-compose.prod.yml ps
```

## Logları Kontrol Edin

Eğer container'lar başlamazsa veya hata alırsanız:

```bash
# Tüm servislerin loglarını görün
docker-compose -f docker-compose.yml -f docker-compose.prod.yml logs -f

# VEYA Makefile ile
make prod-logs

# Sadece Nginx loglarını görün
docker-compose -f docker-compose.yml -f docker-compose.prod.yml logs -f nginx

# Sadece API loglarını görün
docker-compose -f docker-compose.yml -f docker-compose.prod.yml logs -f lifeos.api

# Sadece Client loglarını görün
docker-compose -f docker-compose.yml -f docker-compose.prod.yml logs -f lifeos.client
```

## Beklenen Çıktı

Başarılı bir başlatmada şunları görmelisiniz:

```bash
$ make prod-up
Production ortamı başlatılıyor...
Creating network "lifeos_prod_network" ... done
Creating volume "lifeos_postgres_prod" ... done
Creating volume "lifeos_redis_prod" ... done
Creating volume "lifeos_seq_prod" ... done
Creating volume "lifeos_uploads_prod" ... done
Creating lifeos_postgres_prod ... done
Creating lifeos_redis_prod ... done
Creating lifeos_seq_prod ... done
Creating lifeos_api_prod ... done
Creating lifeos_client_prod ... done
Creating lifeos_nginx ... done
✓ Production servisleri başlatıldı
```

## Container'lar Başladıktan Sonra

Container'lar başladıktan sonra:

1. **Container durumunu kontrol edin:**
   ```bash
   docker ps
   ```
   
   Tüm container'lar "Up" durumunda olmalı.

2. **Health check durumunu kontrol edin:**
   ```bash
   docker ps --format "table {{.Names}}\t{{.Status}}"
   ```
   
   Birkaç saniye sonra container'lar "healthy" durumuna geçmeli.

3. **Web sitesini test edin:**
   - http://45.143.4.244/ - LifeOS uygulaması görünmeli
   - http://45.143.4.244/api/health - API health check endpoint

## Sorun Giderme

### Container'lar başlamıyorsa:

```bash
# Container'ları durdur ve temizle
docker-compose -f docker-compose.yml -f docker-compose.prod.yml down

# Tekrar başlat
make prod-up
```

### Nginx hala default sayfası gösteriyorsa:

1. Nginx container'ının çalıştığından emin olun:
   ```bash
   docker ps | grep nginx
   ```

2. Nginx container'ının doğru konfigürasyonu kullandığını kontrol edin:
   ```bash
   docker exec lifeos_nginx nginx -t
   ```

3. Nginx loglarını kontrol edin:
   ```bash
   docker logs lifeos_nginx
   ```

4. Client container'ının çalıştığından emin olun:
   ```bash
   docker ps | grep client
   docker logs lifeos_client_prod
   ```

### Port çakışması varsa:

Eğer 80 portu zaten kullanılıyorsa (sistem Nginx'i çalışıyorsa):

```bash
# Sistem Nginx'ini durdurun
sudo systemctl stop nginx
sudo systemctl disable nginx

# VEYA docker-compose.prod.yml'de portu değiştirin (önerilmez)
```

