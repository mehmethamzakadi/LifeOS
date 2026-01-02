# 🌐 Domain ve SSL Sertifikası Kurulum Rehberi

Bu rehber, `liferegistry.app` domain'inizi yapılandırma ve Let's Encrypt ile SSL sertifikası kurma adımlarını içerir.

## 📋 Ön Gereksinimler

- Domain adı: `liferegistry.app`
- Sunucu IP: `45.143.4.244`
- Root veya sudo erişimi
- Port 80 ve 443'ün açık olması

---

## 🔧 Adım 1: DNS Ayarları

Domain sağlayıcınızın DNS yönetim paneline giriş yapın ve aşağıdaki kayıtları ekleyin:

### A Kaydı (IPv4)

```
Type: A
Name: @ (veya boş)
Value: 45.143.4.244
TTL: 3600 (veya otomatik)
```

### WWW Alt Domain (Opsiyonel)

```
Type: A
Name: www
Value: 45.143.4.244
TTL: 3600 (veya otomatik)
```

**DNS yayılımı:** Değişikliklerin yayılması 5 dakika ile 48 saat arasında sürebilir. Kontrol için:

```bash
# DNS kaydını kontrol et
dig liferegistry.app +short
# veya
nslookup liferegistry.app

# Beklenen çıktı: 45.143.4.244
```

---

## 🔒 Adım 2: Let's Encrypt SSL Sertifikası Kurulumu

### 2.1. Certbot Kurulumu

```bash
# Sunucuya SSH ile bağlanın
ssh lifeos@45.143.4.244

# Certbot'u yükleyin
sudo apt update
sudo apt install -y certbot python3-certbot-nginx

# Certbot versiyonunu kontrol edin
certbot --version
```

### 2.2. Geçici Nginx Yapılandırması (Sadece İlk Kurulum İçin)

Certbot'un domain'i doğrulayabilmesi için port 80'in açık olması gerekir. Docker container'larınız çalışıyorsa, geçici olarak durdurun:

```bash
cd /opt/lifeos

# Mevcut container'ları durdur
docker-compose -f docker-compose.yml -f docker-compose.prod.yml down

# Geçici Nginx yapılandırması oluşturun (sadece certbot için)
sudo mkdir -p /tmp/certbot-nginx
sudo tee /tmp/certbot-nginx/default.conf > /dev/null <<EOF
server {
    listen 80;
    server_name liferegistry.app www.liferegistry.app;
    
    location /.well-known/acme-challenge/ {
        root /var/www/certbot;
    }
    
    location / {
        return 200 "Certbot verification in progress...";
        add_header Content-Type text/plain;
    }
}
EOF

# Geçici Nginx container'ı çalıştırın
sudo docker run -d \
  --name certbot-nginx \
  -p 80:80 \
  -v /tmp/certbot-nginx:/etc/nginx/conf.d:ro \
  -v /var/www/certbot:/var/www/certbot \
  nginx:alpine

# Certbot'un yazabilmesi için klasör izinlerini ayarlayın
sudo mkdir -p /var/www/certbot
sudo chmod -R 755 /var/www/certbot
```

### 2.3. SSL Sertifikası Oluşturma

```bash
# Standalone mod ile sertifika alın (port 80 kullanır)
sudo certbot certonly \
  --standalone \
  --preferred-challenges http \
  -d liferegistry.app \
  -d www.liferegistry.app \
  --email your-email@example.com \
  --agree-tos \
  --non-interactive

# Sertifikaların oluşturulduğunu kontrol edin
sudo ls -la /etc/letsencrypt/live/liferegistry.app/
```

**Beklenen çıktı:**
- `cert.pem` - Sertifika
- `chain.pem` - Ara sertifika
- `fullchain.pem` - Tam zincir (cert + chain)
- `privkey.pem` - Özel anahtar

### 2.4. Geçici Nginx Container'ını Durdurun

```bash
# Geçici container'ı durdur ve sil
sudo docker stop certbot-nginx
sudo docker rm certbot-nginx
```

---

## 🔧 Adım 3: SSL Sertifikalarını Docker Volume'a Kopyalama

```bash
cd /opt/lifeos

# SSL klasörünü oluştur
mkdir -p deploy/nginx/ssl

# Sertifikaları kopyala
sudo cp /etc/letsencrypt/live/liferegistry.app/fullchain.pem deploy/nginx/ssl/
sudo cp /etc/letsencrypt/live/liferegistry.app/privkey.pem deploy/nginx/ssl/

# İzinleri ayarla
sudo chown -R $USER:$USER deploy/nginx/ssl
chmod 600 deploy/nginx/ssl/*.pem

# Dosyaların kopyalandığını kontrol edin
ls -la deploy/nginx/ssl/
```

---

## ⚙️ Adım 4: Environment Variables Güncelleme

```bash
cd /opt/lifeos

# .env dosyasını düzenleyin
nano .env
```

`.env` dosyasında şu değişkenleri güncelleyin:

```env
# Application URL - HTTPS ile
APP_URL=https://liferegistry.app
VITE_API_URL=https://liferegistry.app
```

**ÖNEMLİ:** `VITE_API_URL` değerinde `/api` eklemeyin! Client tarafı zaten `/api` ekliyor.

---

## 📝 Adım 5: Nginx Konfigürasyonunu Güncelleme

`deploy/nginx/default.conf` dosyası zaten SSL desteği için hazırlanmış. Sadece domain adını güncelleyin:

```bash
cd /opt/lifeos
nano deploy/nginx/default.conf
```

`server_name` satırını güncelleyin:

```nginx
server_name liferegistry.app www.liferegistry.app;
```

SSL satırlarının aktif olduğundan emin olun (dosyada zaten yorum satırından çıkarılmış olmalı).

---

## 🐳 Adım 6: Docker Compose Yapılandırmasını Güncelleme

`docker-compose.prod.yml` dosyasında SSL volume'ları zaten tanımlı. Sadece kontrol edin:

```bash
cd /opt/lifeos
grep -A 5 "volumes:" docker-compose.prod.yml | grep -A 3 "nginx:"
```

Nginx service'inde şu volume'lar olmalı:
- `./deploy/nginx/default.conf:/etc/nginx/conf.d/default.conf:ro`
- `./deploy/nginx/ssl:/etc/nginx/ssl:ro` (SSL için)

---

## 🚀 Adım 7: Container'ları Yeniden Build ve Başlatma

```bash
cd /opt/lifeos

# 1. Client'ı yeniden build et (yeni VITE_API_URL ile)
docker-compose -f docker-compose.yml -f docker-compose.prod.yml build --no-cache lifeos.client

# 2. Tüm container'ları durdur
docker-compose -f docker-compose.yml -f docker-compose.prod.yml down

# 3. Container'ları başlat
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# 4. Logları kontrol edin
docker-compose -f docker-compose.yml -f docker-compose.prod.yml logs -f
```

---

## ✅ Adım 8: Test ve Doğrulama

### 8.1. HTTPS Erişim Testi

Tarayıcınızda şu adresleri test edin:
- `https://liferegistry.app` ✅
- `https://www.liferegistry.app` ✅ (eğer www kaydı eklediyseniz)
- `http://liferegistry.app` → Otomatik olarak HTTPS'e yönlendirilmeli ✅

### 8.2. SSL Sertifika Kontrolü

```bash
# SSL sertifikasını kontrol edin
openssl s_client -connect liferegistry.app:443 -servername liferegistry.app < /dev/null 2>/dev/null | openssl x509 -noout -dates

# Online SSL test (tarayıcıdan)
# https://www.ssllabs.com/ssltest/analyze.html?d=liferegistry.app
```

### 8.3. API Testi

```bash
# Health check
curl -k https://liferegistry.app/api/health

# CORS kontrolü (tarayıcı console'unda)
# https://liferegistry.app adresinde F12 > Console
# fetch('https://liferegistry.app/api/health').then(r => r.json()).then(console.log)
```

---

## 🔄 Adım 9: Otomatik Sertifika Yenileme

Let's Encrypt sertifikaları 90 günde bir yenilenmelidir. Otomatik yenileme için cron job ekleyin:

```bash
# Certbot'un otomatik yenileme komutunu test edin
sudo certbot renew --dry-run

# Cron job ekleyin (her gün kontrol eder, 30 gün kala yeniler)
sudo crontab -e

# Şu satırı ekleyin:
0 3 * * * certbot renew --quiet --deploy-hook "cd /opt/lifeos && docker-compose -f docker-compose.yml -f docker-compose.prod.yml restart nginx"
```

**Alternatif:** Docker container içinde certbot kullanmak için `certbot-nginx` container'ı ekleyebilirsiniz (daha gelişmiş).

---

## 🛠️ Sorun Giderme

### Problem: "Connection refused" veya "This site can't be reached"

**Çözüm:**
```bash
# Port 80 ve 443'ün açık olduğunu kontrol edin
sudo netstat -tlnp | grep -E ':(80|443)'
# veya
sudo ss -tlnp | grep -E ':(80|443)'

# Firewall kontrolü
sudo ufw status
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
```

### Problem: "SSL certificate problem" veya "NET::ERR_CERT_AUTHORITY_INVALID"

**Çözüm:**
```bash
# Sertifikaların doğru kopyalandığını kontrol edin
ls -la deploy/nginx/ssl/

# Nginx container'ının sertifikalara erişebildiğini kontrol edin
docker exec lifeos_nginx ls -la /etc/nginx/ssl/

# Nginx konfigürasyonunu test edin
docker exec lifeos_nginx nginx -t
```

### Problem: "Mixed Content" hatası (HTTP ve HTTPS karışık)

**Çözüm:**
- `.env` dosyasında `APP_URL` ve `VITE_API_URL` değerlerinin `https://` ile başladığından emin olun
- Client container'ını yeniden build edin
- Tarayıcı cache'ini temizleyin (Ctrl+Shift+Delete)

### Problem: CORS hatası

**Çözüm:**
```bash
# docker-compose.prod.yml'de CORS ayarlarını kontrol edin
grep -A 2 "Cors__AllowedOrigins" docker-compose.prod.yml

# APP_URL'in https://liferegistry.app olduğundan emin olun
grep APP_URL .env
```

---

## 📚 Ek Kaynaklar

- [Let's Encrypt Dokümantasyonu](https://letsencrypt.org/docs/)
- [Certbot Kullanım Kılavuzu](https://certbot.eff.org/)
- [Nginx SSL Yapılandırması](https://nginx.org/en/docs/http/configuring_https_servers.html)

---

## ✅ Kontrol Listesi

- [ ] DNS A kaydı eklendi ve yayıldı
- [ ] Certbot kuruldu
- [ ] SSL sertifikası oluşturuldu
- [ ] Sertifikalar `deploy/nginx/ssl/` klasörüne kopyalandı
- [ ] `.env` dosyasında `APP_URL` ve `VITE_API_URL` HTTPS ile güncellendi
- [ ] `deploy/nginx/default.conf` dosyasında `server_name` güncellendi
- [ ] Client container'ı yeniden build edildi
- [ ] Tüm container'lar başlatıldı
- [ ] HTTPS erişimi test edildi
- [ ] HTTP → HTTPS yönlendirmesi çalışıyor
- [ ] Otomatik sertifika yenileme cron job'ı eklendi

---

**🎉 Tebrikler!** Artık `https://liferegistry.app` üzerinden uygulamanıza güvenli bir şekilde erişebilirsiniz!

