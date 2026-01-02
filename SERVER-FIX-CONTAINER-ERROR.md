# ContainerConfig Hatası - Çözüm

## Sorun

Docker Compose'un `ContainerConfig` hatası alıyorsunuz. Bu hata, eski container'ların veya bozuk image metadata'sının temizlenmesi gerektiğini gösterir.

## Çözüm Adımları

Sunucuda şu komutları **sırayla** çalıştırın:

### 1. Mevcut Container'ları Durdurun ve Silin

```bash
cd /opt/lifeos

# Tüm production container'ları durdur ve sil
docker-compose -f docker-compose.yml -f docker-compose.prod.yml down

# Eğer yukarıdaki komut çalışmazsa, container'ları manuel olarak silin:
docker stop lifeos_api_prod lifeos_client_prod lifeos_nginx lifeos_postgres_prod lifeos_redis_prod lifeos_seq_prod 2>/dev/null || true
docker rm lifeos_api_prod lifeos_client_prod lifeos_nginx lifeos_postgres_prod lifeos_redis_prod lifeos_seq_prod 2>/dev/null || true
```

### 2. Bozuk Image'ları Temizleyin (Opsiyonel)

Eğer sorun devam ederse:

```bash
# API image'ını sil ve yeniden build et
docker rmi lifeosapi:latest 2>/dev/null || true

# Client image'ını sil ve yeniden build et
docker rmi lifeosclient:latest 2>/dev/null || true
```

### 3. Yeniden Build ve Başlatma

```bash
# Temiz bir build yapın
make prod-build

# Container'ları başlatın
make prod-up
```

## Hızlı Çözüm (Tek Komut)

Eğer hepsini tek seferde yapmak istiyorsanız:

```bash
cd /opt/lifeos

# Tüm container'ları durdur ve sil
docker-compose -f docker-compose.yml -f docker-compose.prod.yml down

# Bozuk image'ları temizle (opsiyonel, sadece sorun devam ederse)
# docker rmi lifeosapi:latest lifeosclient:latest 2>/dev/null || true

# Yeniden build ve başlat
make prod
```

## Alternatif Çözüm: Zorla Yeniden Oluşturma

Eğer yukarıdaki adımlar işe yaramazsa:

```bash
cd /opt/lifeos

# Container'ları zorla yeniden oluştur
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d --force-recreate --no-deps lifeos.api

# Sonra diğer servisleri başlat
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

## Volume'ları Koruyarak Temizleme

Eğer database verilerini korumak istiyorsanız (volume'ları silmeden):

```bash
cd /opt/lifeos

# Container'ları durdur (volume'lar korunur)
docker-compose -f docker-compose.yml -f docker-compose.prod.yml stop

# Container'ları sil (volume'lar korunur)
docker-compose -f docker-compose.yml -f docker-compose.prod.yml rm -f

# Yeniden başlat
make prod-up
```

## Volume'ları da Silerek Tam Temizleme (DİKKAT!)

**UYARI:** Bu işlem tüm database verilerini siler! Sadece tamamen sıfırdan başlamak istiyorsanız:

```bash
cd /opt/lifeos

# Container'ları ve volume'ları sil
docker-compose -f docker-compose.yml -f docker-compose.prod.yml down -v

# Yeniden build ve başlat
make prod
```

## Önerilen Sıralama

1. **Önce Volume'ları Koruyarak Temizleme** yöntemini deneyin
2. Eğer işe yaramazsa **Hızlı Çözüm** yöntemini kullanın
3. Son çare olarak **Volume'ları da Silerek Tam Temizleme** yöntemini kullanın (veriler silinecek!)

