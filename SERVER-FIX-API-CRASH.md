# API Container Crash (Exit Code 139) - Çözüm

## Sorun

API container'ı sürekli restart oluyor (Exit Code 139 = Segfault/Crash). Bu yüzden diğer servisler de başlamıyor.

## Hemen Yapılacaklar

### 1. API Loglarını Kontrol Edin

```bash
cd /opt/lifeos

# API container'ının loglarını görün
docker logs lifeos_api_prod

# Son 100 satır log
docker logs --tail 100 lifeos_api_prod

# Real-time log takibi
docker logs -f lifeos_api_prod
```

**Logları bana gönderin, böylece tam hatayı görebilirim.**

### 2. Container Durumunu Kontrol Edin

```bash
# Container detaylarını görün
docker inspect lifeos_api_prod | grep -A 20 "State"

# Exit code'u görün
docker inspect lifeos_api_prod --format='{{.State.ExitCode}}'
```

### 3. Olası Nedenler ve Çözümler

#### A) Database Bağlantı Hatası

`.env` dosyasındaki database bilgilerini kontrol edin:

```bash
cd /opt/lifeos

# .env dosyasını kontrol edin (şifreler gizli kalır)
grep -E "^POSTGRES_" .env
```

Çözüm:
- Database container'ının çalıştığından emin olun: `docker ps | grep postgres`
- `.env` dosyasındaki `POSTGRES_PASSWORD`, `POSTGRES_USER`, `POSTGRES_DB` değerlerinin doğru olduğundan emin olun
- Database container'ın healthy olduğundan emin olun: `docker ps` komutunda `(healthy)` yazması gerekir

#### B) Redis Bağlantı Hatası

Redis şifresini kontrol edin:

```bash
grep -E "^REDIS_PASSWORD" .env
```

Çözüm:
- Redis container'ının çalıştığından emin olun: `docker ps | grep redis`
- `.env` dosyasındaki `REDIS_PASSWORD` değerinin doğru olduğundan emin olun

#### C) JWT Secret Key Eksik veya Hatalı

```bash
grep -E "^TOKEN_SECURITY_KEY" .env
```

Çözüm:
- `TOKEN_SECURITY_KEY` en az 32 karakter olmalı
- Değer boş veya çok kısa ise güncelleyin

#### D) Environment Variable Format Hatası

`.env` dosyasında özel karakterler veya yanlış format olabilir:

```bash
# .env dosyasını kontrol edin
cat .env | head -20
```

Çözüm:
- `.env` dosyasında özel karakterler (`$`, `"`, `'` vb.) varsa escape edin veya tırnak içine alın
- Değerlerde satır sonu veya gereksiz boşluk olmamalı

## Hızlı Tanılama Komutu

Şu komutları çalıştırıp çıktıları paylaşın:

```bash
cd /opt/lifeos

echo "=== API Container Logs (Last 50 lines) ==="
docker logs --tail 50 lifeos_api_prod

echo ""
echo "=== Container Status ==="
docker ps -a | grep lifeos_api

echo ""
echo "=== Environment Variables Check ==="
docker inspect lifeos_api_prod --format='{{range .Config.Env}}{{println .}}{{end}}' | grep -E "(POSTGRES|REDIS|TOKEN)" | head -10

echo ""
echo "=== Database Container ==="
docker ps | grep postgres
docker logs --tail 20 lifeos_postgres_prod

echo ""
echo "=== Redis Container ==="
docker ps | grep redis
docker logs --tail 20 lifeos_redis_prod
```

## Geçici Çözüm: API Container'ını Manuel Başlatma

Eğer yukarıdaki kontrolleri yaptıktan sonra hala sorun varsa, API container'ını bağımlılıklar olmadan manuel başlatmayı deneyin:

```bash
# Önce tüm container'ları durdurun
docker-compose -f docker-compose.yml -f docker-compose.prod.yml stop

# Sadece database ve redis'i başlatın
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d postgresdb redis.cache

# 10 saniye bekleyin
sleep 10

# API'yi tek başına başlatın
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d lifeos.api

# Logları izleyin
docker logs -f lifeos_api_prod
```

## En Sık Karşılaşılan Sorunlar

### 1. Database Henüz Hazır Değil

API database'e bağlanmaya çalışıyor ama database henüz hazır değil.

Çözüm: Health check'lerin çalışmasını bekleyin veya `depends_on` ayarlarını kontrol edin.

### 2. Connection String Format Hatası

`.env` dosyasındaki connection string'de özel karakterler düzgün escape edilmemiş olabilir.

Çözüm: `.env` dosyasını kontrol edin, özellikle şifrelerde özel karakterler varsa.

### 3. Migration Eksik

Database'de migration'lar uygulanmamış olabilir.

Çözüm: API container'ı içinde migration'ları çalıştırın (ama önce API'nin başlaması gerekiyor).

## Önemli Not

**Lütfen önce API loglarını kontrol edin ve paylaşın**, böylece tam hatayı görebilirim ve spesifik bir çözüm sunabilirim.

