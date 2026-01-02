# 🔧 Docker Compose Komut Hatası - Çözüm

## Sorun

`docker compose -f docker-compose.prod.yml up -d --build` komutunu çalıştırdığınızda şu hatayı alıyorsunuz:

```
unknown shorthand flag: 'f' in -f
Usage docker [OPTIONS] COMMAND [ARG...]
```

## Neden Oluyor?

Bu hata, sisteminizde **Docker Compose plugin** (`docker compose`) kurulu değil, **Docker Compose standalone** (`docker-compose`) kurulu olduğunu gösterir.

## Çözüm

### Hızlı Çözüm

Komutlarda `docker compose` (iki kelime) yerine `docker-compose` (tire ile tek kelime) kullanın:

```bash
# ❌ Yanlış (plugin versiyonu)
docker compose -f docker-compose.prod.yml up -d --build

# ✅ Doğru (standalone versiyonu)
docker-compose -f docker-compose.prod.yml up -d --build
```

### Hangisini Kullanmalıyım?

Önce sisteminizde hangi komutun çalıştığını kontrol edin:

```bash
# Plugin versiyonunu kontrol et
docker compose version

# Standalone versiyonunu kontrol et
docker-compose --version

# Veya kontrol script'ini kullan
bash scripts/check-docker-compose.sh
```

### Docker Compose Plugin Kurulumu (Önerilen)

Eğer plugin versiyonunu kurmak istiyorsanız:

```bash
sudo apt update
sudo apt install -y docker-compose-plugin

# Kontrol et
docker compose version
```

### Docker Compose Standalone Kurulumu

Eğer standalone versiyonu kurmak istiyorsanız:

```bash
sudo apt update
sudo apt install -y docker-compose

# Kontrol et
docker-compose --version
```

## Tüm Komutlar İçin Dönüşüm Tablosu

| Plugin Versiyonu | Standalone Versiyonu |
|-----------------|---------------------|
| `docker compose -f docker-compose.prod.yml up -d` | `docker-compose -f docker-compose.prod.yml up -d` |
| `docker compose -f docker-compose.prod.yml down` | `docker-compose -f docker-compose.prod.yml down` |
| `docker compose -f docker-compose.prod.yml ps` | `docker-compose -f docker-compose.prod.yml ps` |
| `docker compose -f docker-compose.prod.yml logs -f` | `docker-compose -f docker-compose.prod.yml logs -f` |
| `docker compose -f docker-compose.prod.yml build` | `docker-compose -f docker-compose.prod.yml build` |

## Otomatik Script'ler

Tüm script'ler (`deploy-production.sh`, `update-production.sh`) artık otomatik olarak doğru komutu algılar. Sadece script'leri çalıştırmanız yeterli:

```bash
bash scripts/deploy-production.sh
bash scripts/update-production.sh
```

## Daha Fazla Bilgi

- [Docker Compose Plugin Dokümantasyonu](https://docs.docker.com/compose/)
- [Docker Compose Standalone Kurulum](https://docs.docker.com/compose/install/standalone/)

