# 🔍 Cron Job Kontrol Rehberi

## Hızlı Kontrol

### 1. Root Cron Job'larını Kontrol Et (Önerilen)

SSL sertifikası yenileme cron job'u genellikle root kullanıcısı için eklenir:

```bash
sudo crontab -l
```

**Beklenen çıktı (eğer eklenmişse):**
```
0 3 * * * certbot renew --quiet --deploy-hook "cd /opt/lifeos && docker-compose -f docker-compose.yml -f docker-compose.prod.yml restart nginx"
```

### 2. Mevcut Kullanıcının Cron Job'larını Kontrol Et

```bash
crontab -l
```

### 3. Otomatik Kontrol Script'i Kullan

Sunucuda çalıştırın:

```bash
cd /opt/lifeos
bash scripts/check-cron.sh
```

Bu script şunları kontrol eder:
- Mevcut kullanıcının cron job'ları
- Root cron job'ları
- Sistem cron job'ları
- Certbot ile ilgili tüm cron job'ları

---

## Manuel Kontrol Yöntemleri

### Root Crontab Kontrolü

```bash
# Root cron job'larını listele
sudo crontab -l

# Root cron job'larını düzenle
sudo crontab -e
```

### Sistem Cron Dosyaları

```bash
# Sistem crontab dosyası
cat /etc/crontab

# Sistem cron.d klasörü
ls -la /etc/cron.d/
cat /etc/cron.d/* 2>/dev/null | grep -v "^#" | grep -v "^$"
```

### Certbot Özel Kontrolü

```bash
# Root crontab'ta certbot ara
sudo crontab -l | grep certbot

# Tüm cron dosyalarında certbot ara
sudo grep -r "certbot" /etc/cron* 2>/dev/null
```

---

## Cron Job Ekleme

Eğer cron job eklenmemişse, şu komutlarla ekleyebilirsiniz:

```bash
# Root crontab'ı düzenle
sudo crontab -e

# Şu satırı ekleyin:
0 3 * * * certbot renew --quiet --deploy-hook "cd /opt/lifeos && docker-compose -f docker-compose.yml -f docker-compose.prod.yml restart nginx"
```

**Açıklama:**
- `0 3 * * *` - Her gün saat 03:00'te çalışır
- `certbot renew` - Sertifikaları yeniler (sadece 30 gün kala)
- `--quiet` - Sessiz mod (sadece hata durumunda çıktı)
- `--deploy-hook` - Yenileme başarılı olduğunda çalışacak komut

---

## Cron Job Test Etme

### 1. Dry-Run Test

Certbot'un yenileme komutunu test edin:

```bash
sudo certbot renew --dry-run
```

### 2. Cron Job'un Çalışıp Çalışmadığını Kontrol Et

Cron log'larını kontrol edin:

```bash
# Ubuntu/Debian sistemlerde
sudo grep CRON /var/log/syslog | tail -20

# CentOS/RHEL sistemlerde
sudo grep CRON /var/log/cron | tail -20

# Systemd journal kullanan sistemlerde
sudo journalctl -u cron | tail -20
```

### 3. Manuel Çalıştırma

Cron job'u manuel olarak test edin:

```bash
# Sertifikaları kontrol et (yenileme gerekiyorsa yeniler)
sudo certbot renew

# Nginx'i yeniden başlat
cd /opt/lifeos
docker-compose -f docker-compose.yml -f docker-compose.prod.yml restart nginx
```

---

## Cron Job Kaldırma

Eğer cron job'u kaldırmak isterseniz:

```bash
# Root crontab'ı düzenle
sudo crontab -e

# İlgili satırı silin veya yorum satırı yapın (# ekleyin)
```

Veya direkt olarak:

```bash
# Mevcut cron job'ları al, certbot satırını çıkar, tekrar yaz
sudo crontab -l | grep -v certbot | sudo crontab -
```

---

## Sık Karşılaşılan Sorunlar

### Problem: "crontab: command not found"

**Çözüm:**
```bash
# Cron servisini yükle (Debian/Ubuntu)
sudo apt update
sudo apt install cron

# Cron servisini başlat
sudo systemctl start cron
sudo systemctl enable cron
```

### Problem: Cron Job Çalışmıyor

**Kontrol Listesi:**
1. Cron servisinin çalıştığından emin olun:
   ```bash
   sudo systemctl status cron
   # veya
   sudo systemctl status crond
   ```

2. Cron job'un doğru formatta olduğundan emin olun:
   ```bash
   sudo crontab -l
   ```

3. Log dosyalarını kontrol edin:
   ```bash
   sudo grep CRON /var/log/syslog | grep certbot
   ```

4. Komutu manuel olarak test edin:
   ```bash
   sudo certbot renew --dry-run
   ```

### Problem: "Permission denied" Hatası

**Çözüm:**
- Certbot komutu için `sudo` kullanın (root crontab'ında çalıştırın)
- Docker komutları için kullanıcının docker grubunda olduğundan emin olun:
  ```bash
  sudo usermod -aG docker $USER
  ```

---

## Özet Komutlar

```bash
# Hızlı kontrol
sudo crontab -l | grep certbot

# Otomatik kontrol script'i
bash scripts/check-cron.sh

# Cron job ekleme
sudo crontab -e

# Test etme
sudo certbot renew --dry-run

# Log kontrolü
sudo grep CRON /var/log/syslog | tail -20
```

