# 🚀 Hızlı Domain ve SSL Kurulum Rehberi

## ⚡ Hızlı Başlangıç (5 Adım)

### 1️⃣ DNS Ayarları (Domain Sağlayıcınızda)

Domain sağlayıcınızın DNS paneline giriş yapın ve şu A kaydını ekleyin:

```
Type: A
Name: @ (veya boş)
Value: 45.143.4.244
TTL: 3600
```

**Opsiyonel:** www alt domain için:
```
Type: A
Name: www
Value: 45.143.4.244
TTL: 3600
```

DNS yayılımını kontrol edin:
```bash
dig liferegistry.app +short
# Beklenen: 45.143.4.244
```

---

### 2️⃣ SSL Sertifikası Kurulumu (Otomatik)

Sunucuya SSH ile bağlanın ve script'i çalıştırın:

```bash
ssh lifeos@45.143.4.244
cd /opt/lifeos

# Script'i çalıştırın (email adresinizi girin)
CERTBOT_EMAIL=your-email@example.com bash scripts/setup-ssl.sh
```

**Not:** Script otomatik olarak:
- Certbot'u kurar (yoksa)
- DNS kaydını kontrol eder
- Geçici Nginx container'ı oluşturur
- SSL sertifikası alır
- Sertifikaları proje klasörüne kopyalar

---

### 3️⃣ Environment Variables Güncelleme

```bash
cd /opt/lifeos
nano .env
```

Şu satırları güncelleyin:

```env
APP_URL=https://liferegistry.app
VITE_API_URL=https://liferegistry.app
```

**ÖNEMLİ:** `VITE_API_URL` değerinde `/api` eklemeyin!

---

### 4️⃣ Client Container'ını Yeniden Build Etme

```bash
cd /opt/lifeos

# Client'ı yeniden build et (yeni VITE_API_URL ile)
docker-compose -f docker-compose.yml -f docker-compose.prod.yml build --no-cache lifeos.client
```

---

### 5️⃣ Container'ları Başlatma

```bash
cd /opt/lifeos

# Tüm container'ları başlat
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# Logları kontrol et
docker-compose -f docker-compose.yml -f docker-compose.prod.yml logs -f
```

---

## ✅ Test

Tarayıcınızda test edin:
- ✅ `https://liferegistry.app` - Ana sayfa
- ✅ `http://liferegistry.app` - Otomatik HTTPS'e yönlendirilmeli
- ✅ `https://liferegistry.app/api/health` - API health check

---

## 🔄 Otomatik Sertifika Yenileme

Let's Encrypt sertifikaları 90 günde bir yenilenmelidir. Otomatik yenileme için:

```bash
sudo crontab -e
```

Şu satırı ekleyin:
```
0 3 * * * certbot renew --quiet --deploy-hook "cd /opt/lifeos && docker-compose -f docker-compose.yml -f docker-compose.prod.yml restart nginx"
```

---

## 🆘 Sorun Giderme

### DNS yayılmadı
```bash
# DNS kontrolü
dig liferegistry.app +short
nslookup liferegistry.app

# Beklenen: 45.143.4.244
# Eğer farklı bir IP görüyorsanız, DNS ayarlarınızı kontrol edin
```

### Port 80/443 kapalı
```bash
# Port kontrolü
sudo netstat -tlnp | grep -E ':(80|443)'
sudo ss -tlnp | grep -E ':(80|443)'

# Firewall kontrolü
sudo ufw status
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
```

### SSL sertifikası hatası
```bash
# Sertifikaların varlığını kontrol edin
ls -la /opt/lifeos/deploy/nginx/ssl/

# Nginx container'ının sertifikalara erişebildiğini kontrol edin
docker exec lifeos_nginx ls -la /etc/nginx/ssl/

# Nginx konfigürasyonunu test edin
docker exec lifeos_nginx nginx -t
```

### CORS hatası
```bash
# .env dosyasında APP_URL'in https:// ile başladığından emin olun
grep APP_URL /opt/lifeos/.env

# docker-compose.prod.yml'de CORS ayarlarını kontrol edin
grep -A 2 "Cors__AllowedOrigins" /opt/lifeos/docker-compose.prod.yml
```

---

## 📚 Detaylı Rehber

Daha detaylı bilgi için: `DOMAIN-SSL-SETUP.md` dosyasına bakın.

---

**🎉 Başarılar!** Artık `https://liferegistry.app` üzerinden uygulamanıza erişebilirsiniz!

