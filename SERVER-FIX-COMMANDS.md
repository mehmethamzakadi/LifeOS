# Sunucu Sorun Giderme Komutları

Sunucunuzda aşağıdaki komutları sırayla çalıştırın:

## 1. Sunucuya Bağlanın

```bash
ssh lifeos@45.143.4.244
```

## 2. Proje Dizinine Gidin

```bash
cd /opt/lifeos
# veya projenizin bulunduğu dizin
pwd  # Mevcut dizini kontrol edin
```

## 3. .env Dosyasını Kontrol Edin ve Düzeltin

```bash
# .env dosyasının var olup olmadığını kontrol edin
ls -la .env

# Eğer yoksa, oluşturun
cp .env.example .env

# İzinleri düzeltin
chmod 600 .env

# Dosya sahibini kontrol edin
ls -l .env

# Eğer sahip yanlışsa (örn: root), düzeltin:
sudo chown lifeos:lifeos .env

# Okuma iznini test edin
cat .env | head -5
```

## 4. Docker Compose Komutunu Kontrol Edin

```bash
# Hangi Docker Compose komutu çalışıyor?
docker compose version 2>/dev/null && echo "Plugin kullanılıyor" || echo "Plugin yok"
docker-compose --version 2>/dev/null && echo "Standalone kullanılıyor" || echo "Standalone yok"

# Hangisi çalışıyorsa onu kullanın
# Plugin için: docker compose
# Standalone için: docker-compose
```

## 5. Docker Compose Dosyalarını Test Edin

```bash
# Önce hangi komut çalışıyorsa onu kullanın
# Plugin versiyonu için:
docker compose -f docker-compose.yml -f docker-compose.prod.yml config

# VEYA Standalone versiyonu için:
docker-compose -f docker-compose.yml -f docker-compose.prod.yml config
```

## 6. Tanılama Scriptini Çalıştırın (Opsiyonel)

Eğer script'i sunucuya kopyaladıysanız:

```bash
# Script'i çalıştırılabilir yapın
chmod +x scripts/diagnose-production.sh

# Çalıştırın
bash scripts/diagnose-production.sh
```

## 7. Build Komutunu Çalıştırın

**ÖNEMLİ:** Hangi Docker Compose komutu çalışıyorsa onu kullanın:

### Plugin versiyonu (docker compose) kullanıyorsanız:

```bash
docker compose -f docker-compose.yml -f docker-compose.prod.yml build --no-cache
```

### Standalone versiyonu (docker-compose) kullanıyorsanız:

```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml build --no-cache
```

## Olası Sorunlar ve Çözümleri

### Sorun 1: Permission denied: './.env'

**Çözüm:**
```bash
# Dosya izinlerini düzelt
chmod 600 .env

# Dosya sahibini kontrol et
ls -l .env

# Eğer root sahipliyse:
sudo chown $USER:$USER .env
```

### Sorun 2: Docker Compose komutu bulunamadı

**Çözüm:**
```bash
# Plugin versiyonunu kurun (önerilen)
sudo apt update
sudo apt install -y docker-compose-plugin

# VEYA Standalone versiyonunu kurun
sudo apt update
sudo apt install -y docker-compose
```

### Sorun 3: Docker izinleri yok

**Çözüm:**
```bash
# Docker grubuna ekleyin
sudo usermod -aG docker $USER

# Oturumu yenileyin (birini seçin):
# Seçenek 1: Sunucudan çıkıp tekrar girin
exit
ssh lifeos@45.143.4.244

# Seçenek 2: Grubu aktif hale getirin
newgrp docker
```

### Sorun 4: .env dosyası yok

**Çözüm:**
```bash
# .env.example'dan kopyalayın
cp .env.example .env

# İzinleri düzeltin
chmod 600 .env

# İçeriği düzenleyin (TÜM DEĞERLERİ GÜNCELLEYİN!)
nano .env
```

## Hızlı Düzeltme (Tüm Adımlar)

Eğer tüm adımları tek seferde yapmak istiyorsanız:

```bash
cd /opt/lifeos  # veya proje dizininiz

# .env dosyasını oluştur ve izinleri düzelt
if [ ! -f .env ]; then
    cp .env.example .env
fi
chmod 600 .env
sudo chown $USER:$USER .env 2>/dev/null || chown $USER:$USER .env

# Docker Compose komutunu belirle
if docker compose version &>/dev/null; then
    CMD="docker compose"
elif docker-compose --version &>/dev/null; then
    CMD="docker-compose"
else
    echo "Docker Compose kurulu değil!"
    exit 1
fi

# Build komutunu çalıştır
$CMD -f docker-compose.yml -f docker-compose.prod.yml build --no-cache
```

