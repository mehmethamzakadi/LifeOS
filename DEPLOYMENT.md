# LifeOS - Production Deployment Rehberi

Bu doküman, LifeOS uygulamasını Ubuntu 22.04 VDS sunucusunda production ortamında yayınlamak için adım adım rehber içerir.

## 📋 İçindekiler

1. [Ön Gereksinimler](#ön-gereksinimler)
2. [Sunucu Hazırlığı](#sunucu-hazırlığı)
3. [Güvenlik Ayarları](#güvenlik-ayarları)
4. [Domain ve SSL Sertifikası](#domain-ve-ssl-sertifikası)
5. [Proje Kurulumu](#proje-kurulumu)
6. [Environment Variables](#environment-variables)
7. [Deployment](#deployment)
8. [İlk Çalıştırma](#ilk-çalıştırma)
9. [Monitoring ve Backup](#monitoring-ve-backup)
10. [Sorun Giderme](#sorun-giderme)

---

## 🎯 Ön Gereksinimler

### Minimum Sistem Gereksinimleri

- **OS**: Ubuntu 22.04 LTS (64-bit)
- **RAM**: En az 4GB (önerilen: 8GB+)
- **CPU**: En az 2 core (önerilen: 4 core+)
- **Disk**: En az 50GB boş alan (SSD önerilir)
- **Network**: Statik IP adresi
- **Domain**: Kendi domain'iniz (SSL sertifikası için)

### Gereken Bilgiler

- VDS sunucu root/sudo erişimi
- Domain adınız (örn: `yourdomain.com`)
- Domain DNS ayarlarına erişim

---

## 🛠 Sunucu Hazırlığı

### 1. Sistem Güncellemesi

```bash
# Sunucuya SSH ile bağlanın
ssh root@your-server-ip

# Sistem güncellemesi
apt update && apt upgrade -y

# Gerekli temel paketler
apt install -y curl wget git ufw fail2ban htop nano
```

### 2. Docker Kurulumu

```bash
# Docker'ı kaldır (varsa eski sürüm)
apt remove -y docker docker-engine docker.io containerd runc

# Docker için gerekli paketler
apt install -y ca-certificates gnupg lsb-release

# Docker'ın resmi GPG key'ini ekle
mkdir -p /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg

# Docker repository ekle
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null

# Docker'ı yükle
apt update
apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# Docker servisini başlat ve otomatik başlatmayı etkinleştir
systemctl start docker
systemctl enable docker

# Docker Compose kurulumunu kontrol et
docker compose version
```

### 3. Non-Root User Oluşturma (Önerilen)

```bash
# Yeni kullanıcı oluştur
useradd -m -s /bin/bash lifeos
usermod -aG sudo lifeos
usermod -aG docker lifeos

# Şifre belirle
passwd lifeos

# Yeni kullanıcıya geç
su - lifeos
```

---

## 🔒 Güvenlik Ayarları

### 1. Firewall (UFW) Yapılandırması

```bash
# Firewall'u sıfırla
ufw --force reset

# Varsayılan politikaları ayarla
ufw default deny incoming
ufw default allow outgoing

# SSH erişimini aç (ÖNEMLİ: Önce bu portu açın!)
ufw allow 22/tcp comment 'SSH'

# HTTP ve HTTPS portlarını aç
ufw allow 80/tcp comment 'HTTP'
ufw allow 443/tcp comment 'HTTPS'

# Firewall'u etkinleştir
ufw --force enable

# Durumu kontrol et
ufw status verbose
```

### 2. Fail2Ban Kurulumu (Brute Force Koruması)

```bash
# Fail2Ban'ı yapılandır
cat > /etc/fail2ban/jail.local << EOF
[DEFAULT]
bantime = 3600
findtime = 600
maxretry = 5

[sshd]
enabled = true
port = 22
filter = sshd
logpath = /var/log/auth.log
maxretry = 3
bantime = 86400
EOF

# Fail2Ban'ı başlat
systemctl restart fail2ban
systemctl enable fail2ban

# Durumu kontrol et
fail2ban-client status
```

### 3. SSH Güvenliği (Önerilen)

```bash
# SSH yapılandırmasını düzenle
nano /etc/ssh/sshd_config

# Aşağıdaki ayarları yapın:
# PermitRootLogin no
# PasswordAuthentication yes  (SSH key kullanıyorsanız no yapın)
# Port 22  (Özel port kullanmak isterseniz değiştirin)

# SSH servisini yeniden başlat
systemctl restart sshd
```

---

## 🌐 Domain ve SSL Sertifikası

### 1. Domain DNS Ayarları

Domain sağlayıcınızın DNS yönetim panelinde aşağıdaki kayıtları ekleyin:

```
A Record:    @              -> YOUR_SERVER_IP
A Record:    www            -> YOUR_SERVER_IP
A Record:    api            -> YOUR_SERVER_IP (isteğe bağlı, API için subdomain)
```

DNS değişikliklerinin yayılması 24 saat kadar sürebilir. Kontrol için:

```bash
# DNS çözümlemesini test edin
dig yourdomain.com
nslookup yourdomain.com
```

### 2. SSL Sertifikası (Let's Encrypt) - Certbot

```bash
# Certbot'u yükle
apt install -y certbot python3-certbot-nginx

# Nginx'i durdur (geçici olarak)
docker compose -f docker-compose.prod.yml down

# Standalone mod ile sertifika al (Nginx çalışmıyorken)
certbot certonly --standalone -d yourdomain.com -d www.yourdomain.com

# Sertifikalar şu konumda olacak:
# /etc/letsencrypt/live/yourdomain.com/fullchain.pem
# /etc/letsencrypt/live/yourdomain.com/privkey.pem
```

**Alternatif Yöntem (Nginx ile birlikte çalışırken):**

Önce uygulamayı HTTP üzerinden başlatın, sonra:

```bash
# Certbot'u Nginx plugin ile kullan
certbot --nginx -d yourdomain.com -d www.yourdomain.com
```

### 3. SSL Otomatik Yenileme

```bash
# Certbot otomatik yenileme servisini kontrol et
systemctl status certbot.timer
systemctl enable certbot.timer

# Test yenileme
certbot renew --dry-run
```

---

## 📁 Proje Kurulumu

### 1. Proje Dosyalarını Sunucuya Aktarma

**Yöntem 1: Git ile (Önerilen)**

```bash
# Git kurulu değilse yükleyin
apt install -y git

# Proje klasörü oluştur
mkdir -p /opt/lifeos
cd /opt/lifeos

# Repository'yi klonlayın (private repo ise SSH key kullanın)
git clone https://github.com/yourusername/LifeOS.git .

# Veya belirli bir branch/commit'e geçin
git checkout main  # veya production branch
```

**Yöntem 2: SCP ile Dosya Aktarımı**

Yerel makinenizden:

```bash
# Proje klasörünü sıkıştır
tar -czf lifeos.tar.gz --exclude='node_modules' --exclude='bin' --exclude='obj' .

# Sunucuya aktar
scp lifeos.tar.gz lifeos@your-server-ip:/home/lifeos/

# Sunucuda aç
ssh lifeos@your-server-ip
cd /opt
sudo mkdir -p lifeos
sudo tar -xzf ~/lifeos.tar.gz -C /opt/lifeos
sudo chown -R lifeos:lifeos /opt/lifeos
```

### 2. Proje Klasör Yapısı

```
/opt/lifeos/
├── docker-compose.prod.yml
├── .env (oluşturulacak)
├── src/
├── clients/
└── deploy/
```

---

## ⚙️ Environment Variables

### 1. .env Dosyası Oluşturma

```bash
cd /opt/lifeos

# .env dosyasını oluştur
nano .env
```

### 2. .env Dosyası İçeriği

Aşağıdaki şablonu kullanın ve **tüm değerleri kendi güvenli değerlerinizle değiştirin**:

```bash
# ============================================
# LifeOS - Production Environment Variables
# ============================================

# Docker Image Tags
TAG=latest
DOCKER_REGISTRY=

# Application URL
APP_URL=https://yourdomain.com
VITE_API_URL=https://yourdomain.com/api

# PostgreSQL Database
POSTGRES_DB=LifeOSDb
POSTGRES_USER=lifeos_user
POSTGRES_PASSWORD=CHANGE_THIS_TO_STRONG_PASSWORD_AT_LEAST_32_CHARS

# Redis Cache
REDIS_PASSWORD=CHANGE_THIS_TO_STRONG_PASSWORD_AT_LEAST_32_CHARS

# JWT Security Key (Minimum 32 karakter, güçlü bir key kullanın!)
TOKEN_SECURITY_KEY=CHANGE_THIS_TO_VERY_STRONG_SECRET_KEY_MIN_32_CHARS_LONG

# Seq Logging
SEQ_ADMIN_PASSWORD=CHANGE_THIS_TO_STRONG_PASSWORD_AT_LEAST_32_CHARS
```

### 3. Güçlü Şifre Oluşturma

```bash
# Güçlü şifre oluşturmak için (64 karakter)
openssl rand -base64 48

# Veya Python ile
python3 -c "import secrets; print(secrets.token_urlsafe(48))"
```

**ÖNEMLİ GÜVENLİK KURALLARI:**

- ✅ Tüm şifreleri mutlaka değiştirin!
- ✅ `TOKEN_SECURITY_KEY` en az 32 karakter olmalı
- ✅ Şifreler rastgele ve karmaşık olmalı
- ✅ `.env` dosyasını git'e commit etmeyin
- ✅ Dosya izinlerini kısıtlayın: `chmod 600 .env`

---

## 🚀 Deployment

### 1. SSL Sertifikalarını Nginx Klasörüne Kopyalama

```bash
cd /opt/lifeos

# SSL klasörünü oluştur
mkdir -p deploy/nginx/ssl

# Sertifikaları kopyala (Let's Encrypt)
sudo cp /etc/letsencrypt/live/yourdomain.com/fullchain.pem deploy/nginx/ssl/
sudo cp /etc/letsencrypt/live/yourdomain.com/privkey.pem deploy/nginx/ssl/

# İzinleri ayarla
sudo chown -R $USER:$USER deploy/nginx/ssl
chmod 600 deploy/nginx/ssl/*.pem
```

### 2. Nginx Yapılandırmasını Güncelleme

`deploy/nginx/default.conf` dosyasında SSL ayarlarını aktif edin:

```bash
nano deploy/nginx/default.conf
```

Şu satırları yorum satırından çıkarın:
- SSL listen portu (443)
- SSL sertifika yolları
- HTTPS yönlendirmesi
- HSTS header'ı

### 3. Docker Compose ile Build ve Deploy

```bash
cd /opt/lifeos

# Docker Compose dosyasını kontrol et
docker compose -f docker-compose.prod.yml config

# İlk build ve deploy (tüm servisleri başlatır)
docker compose -f docker-compose.prod.yml up -d --build

# Logları takip et
docker compose -f docker-compose.prod.yml logs -f
```

### 4. Servis Durumunu Kontrol Etme

```bash
# Tüm container'ların durumunu kontrol et
docker compose -f docker-compose.prod.yml ps

# Health check'leri kontrol et
docker compose -f docker-compose.prod.yml ps --format json | jq '.[] | {name: .Name, health: .Health}'

# Belirli bir servisin loglarını görüntüle
docker compose -f docker-compose.prod.yml logs -f lifeos.api
docker compose -f docker-compose.prod.yml logs -f nginx
```

---

## ✅ İlk Çalıştırma

### 1. Servislerin Hazır Olmasını Bekleme

```bash
# Tüm servislerin sağlıklı olmasını bekle (yaklaşık 1-2 dakika)
watch -n 2 'docker compose -f docker-compose.prod.yml ps'
```

### 2. Health Check Endpoint'lerini Test Etme

```bash
# API Health Check
curl http://localhost/health
curl https://yourdomain.com/health

# Nginx durumu
curl -I https://yourdomain.com
```

### 3. Tarayıcıdan Test

- Ana sayfa: `https://yourdomain.com`
- API Health: `https://yourdomain.com/health`
- API Endpoint: `https://yourdomain.com/api/...`

### 4. İlk Admin Kullanıcısı Oluşturma

Uygulamanızın register endpoint'ini kullanarak ilk admin kullanıcısını oluşturun.

---

## 📊 Monitoring ve Backup

### 1. Log İnceleme

```bash
# Tüm loglar
docker compose -f docker-compose.prod.yml logs -f

# Belirli servis
docker compose -f docker-compose.prod.yml logs -f lifeos.api

# Son 100 satır
docker compose -f docker-compose.prod.yml logs --tail=100 lifeos.api

# Belirli tarihten itibaren
docker compose -f docker-compose.prod.yml logs --since 2024-01-01T00:00:00
```

### 2. Seq Log Viewer Erişimi

Seq, production'da sadece internal network'ten erişilebilir. Erişmek için:

```bash
# SSH tunnel oluştur (yerel makinenizden)
ssh -L 5341:localhost:5341 lifeos@your-server-ip

# Veya Nginx'e Seq için bir location ekleyebilirsiniz (güvenlik önlemleriyle)
```

### 3. Veritabanı Yedekleme

```bash
# Backup script oluştur
cat > /opt/lifeos/backup.sh << 'EOF'
#!/bin/bash
BACKUP_DIR="/opt/lifeos/backups"
DATE=$(date +%Y%m%d_%H%M%S)
mkdir -p $BACKUP_DIR

# PostgreSQL backup
docker compose -f /opt/lifeos/docker-compose.prod.yml exec -T postgresdb pg_dump -U $POSTGRES_USER $POSTGRES_DB | gzip > $BACKUP_DIR/postgres_$DATE.sql.gz

# Redis backup (RDB file zaten otomatik yedekleniyor)
# docker cp lifeos_redis_prod:/data/dump.rdb $BACKUP_DIR/redis_$DATE.rdb

# Eski backup'ları sil (7 günden eski)
find $BACKUP_DIR -name "*.gz" -mtime +7 -delete
find $BACKUP_DIR -name "*.rdb" -mtime +7 -delete

echo "Backup completed: $DATE"
EOF

chmod +x /opt/lifeos/backup.sh

# Cron job ekle (her gün saat 02:00'de)
(crontab -l 2>/dev/null; echo "0 2 * * * /opt/lifeos/backup.sh >> /var/log/lifeos-backup.log 2>&1") | crontab -
```

### 4. Disk Kullanımını İzleme

```bash
# Disk kullanımı
df -h

# Docker disk kullanımı
docker system df

# Eski image/container'ları temizle
docker system prune -a --volumes  # DİKKAT: Tüm kullanılmayan verileri siler
```

### 5. Resource Kullanımını İzleme

```bash
# Container resource kullanımı
docker stats

# Sistem kaynakları
htop
```

---

## 🔄 Güncelleme (Update) İşlemi

### 1. Yeni Versiyonu Çekme

```bash
cd /opt/lifeos

# Git ile güncelleme
git pull origin main  # veya production branch

# Veya yeni dosyaları aktarın (SCP ile)
```

### 2. Servisleri Yeniden Build ve Deploy

```bash
# Sadece değişen servisleri rebuild et
docker compose -f docker-compose.prod.yml up -d --build

# Veya tüm servisleri yeniden başlat (downtime olur)
docker compose -f docker-compose.prod.yml down
docker compose -f docker-compose.prod.yml up -d --build
```

### 3. Zero-Downtime Güncelleme (Önerilen)

```bash
# Yeni image'ları build et
docker compose -f docker-compose.prod.yml build

# Rolling update (API için)
docker compose -f docker-compose.prod.yml up -d --no-deps --build lifeos.api

# Health check'leri kontrol et
docker compose -f docker-compose.prod.yml ps
```

---

## 🛑 Sorun Giderme

### 1. Container'lar Başlamıyor

```bash
# Logları kontrol et
docker compose -f docker-compose.prod.yml logs

# Container durumunu kontrol et
docker compose -f docker-compose.prod.yml ps -a

# Health check'leri kontrol et
docker inspect lifeos_api_prod | jq '.[0].State.Health'
```

### 2. Veritabanı Bağlantı Hatası

```bash
# PostgreSQL container'ını kontrol et
docker compose -f docker-compose.prod.yml logs postgresdb

# PostgreSQL'e bağlan
docker compose -f docker-compose.prod.yml exec postgresdb psql -U $POSTGRES_USER -d $POSTGRES_DB

# Connection string'i kontrol et
docker compose -f docker-compose.prod.yml exec lifeos.api env | grep ConnectionStrings
```

### 3. SSL Sertifika Hatası

```bash
# Sertifika dosyalarını kontrol et
ls -la deploy/nginx/ssl/

# Sertifika süresini kontrol et
sudo certbot certificates

# Sertifikayı yenile
sudo certbot renew
sudo systemctl reload nginx  # veya container'ı restart et
```

### 4. Port Çakışması

```bash
# Kullanılan portları kontrol et
netstat -tulpn | grep LISTEN
ss -tulpn | grep LISTEN

# Belirli portu kullanan process'i bul
sudo lsof -i :80
sudo lsof -i :443
```

### 5. Disk Dolması

```bash
# Disk kullanımı
df -h

# Docker log dosyalarını temizle
docker compose -f docker-compose.prod.yml down
docker system prune -a
docker volume prune

# Eski log dosyalarını temizle
journalctl --vacuum-time=7d
```

### 6. Performans Sorunları

```bash
# Container resource kullanımı
docker stats

# API response time'ları kontrol et (Seq'de)
# Nginx access log'larını analiz et
docker compose -f docker-compose.prod.yml exec nginx tail -f /var/log/nginx/access.log
```

---

## 📝 Önemli Notlar

### Güvenlik Checklist

- ✅ Firewall (UFW) aktif ve doğru yapılandırılmış
- ✅ Fail2Ban aktif
- ✅ SSL sertifikası kurulu ve otomatik yenileniyor
- ✅ Tüm şifreler güçlü ve benzersiz
- ✅ `.env` dosyası güvenli (chmod 600)
- ✅ Root login SSH'da kapalı (önerilen)
- ✅ Database ve Redis portları dışarıya açık değil
- ✅ Regular backup'lar alınıyor

### Performans İpuçları

- Production'da API'yi scale edebilirsiniz: `docker compose -f docker-compose.prod.yml up -d --scale lifeos.api=3`
- Nginx'de rate limiting aktif
- PostgreSQL connection pooling yapılandırılmış
- Redis cache aktif
- Gzip compression aktif

### Maintenance

- Düzenli olarak sistem güncellemeleri yapın: `apt update && apt upgrade`
- Docker image'larını güncel tutun
- Log dosyalarını düzenli temizleyin
- Backup'ları düzenli kontrol edin
- SSL sertifikalarının otomatik yenilendiğini kontrol edin

---

## 🆘 Yardım ve Destek

Sorun yaşarsanız:

1. Logları kontrol edin: `docker compose -f docker-compose.prod.yml logs`
2. Health check'leri kontrol edin: `docker compose -f docker-compose.prod.yml ps`
3. Dokümantasyonu okuyun
4. GitHub Issues'da sorun açın

---

**Son Güncelleme:** 2024
**Versiyon:** 1.0

