# 🚀 LifeOS - Production Deployment Hızlı Başlangıç

Bu doküman, LifeOS uygulamasını Ubuntu 22.04 VDS sunucusunda hızlıca production'a almak için özet bilgiler içerir.

## 📋 Hızlı Özet

### 1️⃣ Sunucu Hazırlığı (Tek Seferlik)

```bash
# Sunucuya SSH ile bağlan
ssh root@your-server-ip

# Setup script'ini çalıştır
curl -O https://raw.githubusercontent.com/yourrepo/LifeOS/main/scripts/setup-server.sh
# VEYA projeyi klonladıktan sonra:
bash scripts/setup-server.sh
```

### 2️⃣ Projeyi Sunucuya Aktar

```bash
# Git ile (Önerilen)
cd /opt
git clone https://github.com/mehmethamzakadi/LifeOS.git lifeos
cd lifeos

# VEYA SCP ile
# Yerel makinenizden:
scp -r . lifeos@your-server-ip:/opt/lifeos/
```

### 3️⃣ Environment Variables Ayarla

```bash
cd /opt/lifeos

# .env dosyasını oluştur
cp .env.production.example .env

# Düzenle (TÜM değerleri değiştirin!)
nano .env

# Güvenlik için izinleri kısıtla
chmod 600 .env
```

**ÖNEMLİ:** `.env` dosyasında mutlaka değiştirmeniz gerekenler:
- ✅ `POSTGRES_PASSWORD` - Güçlü şifre
- ✅ `REDIS_PASSWORD` - Güçlü şifre  
- ✅ `TOKEN_SECURITY_KEY` - Çok güçlü key (64+ karakter)
- ✅ `SEQ_ADMIN_PASSWORD` - Güçlü şifre
- ✅ `APP_URL` - Kendi domain'iniz
- ✅ `VITE_API_URL` - API URL'iniz

Güçlü şifre oluşturma:
```bash
openssl rand -base64 48
```

### 4️⃣ SSL Sertifikası (Önerilir)

```bash
# Certbot ile Let's Encrypt sertifikası al
sudo certbot certonly --standalone -d yourdomain.com -d www.yourdomain.com

# Sertifikaları Nginx klasörüne kopyala
sudo mkdir -p /opt/lifeos/deploy/nginx/ssl
sudo cp /etc/letsencrypt/live/yourdomain.com/fullchain.pem /opt/lifeos/deploy/nginx/ssl/
sudo cp /etc/letsencrypt/live/yourdomain.com/privkey.pem /opt/lifeos/deploy/nginx/ssl/
sudo chown -R $USER:$USER /opt/lifeos/deploy/nginx/ssl
chmod 600 /opt/lifeos/deploy/nginx/ssl/*.pem
```

**Not:** SSL olmadan da çalışabilir (HTTP üzerinden).

### 5️⃣ Nginx SSL Yapılandırması (SSL kullanıyorsanız)

`deploy/nginx/default.conf` dosyasında SSL ayarlarını aktif edin (yorum satırlarından çıkarın).

### 6️⃣ Deploy Et

```bash
cd /opt/lifeos

# Deployment script'ini çalıştır
bash scripts/deploy-production.sh

# VEYA manuel olarak:
docker compose -f docker-compose.prod.yml up -d --build
```

### 7️⃣ Kontrol Et

```bash
# Container durumları
docker compose -f docker-compose.prod.yml ps

# Logları görüntüle
docker compose -f docker-compose.prod.yml logs -f

# Health check
curl https://yourdomain.com/health
```

---

## 🔒 Güvenlik Checklist

- [ ] Firewall (UFW) aktif ve yapılandırılmış
- [ ] Fail2Ban aktif
- [ ] SSL sertifikası kurulu (HTTPS)
- [ ] Tüm şifreler güçlü ve benzersiz
- [ ] `.env` dosyası güvenli (chmod 600)
- [ ] Database ve Redis portları dışarıya kapalı
- [ ] Root SSH login kapalı (önerilen)

---

## 📚 Detaylı Dokümantasyon

Tüm detaylar için: **[DEPLOYMENT.md](DEPLOYMENT.md)** dosyasına bakın.

---

## ⚡ Hızlı Komutlar

```bash
# Servisleri başlat
docker compose -f docker-compose.prod.yml up -d

# Servisleri durdur
docker compose -f docker-compose.prod.yml down

# Logları görüntüle
docker compose -f docker-compose.prod.yml logs -f

# Belirli servis logları
docker compose -f docker-compose.prod.yml logs -f lifeos.api

# Container durumları
docker compose -f docker-compose.prod.yml ps

# Servisleri yeniden başlat
docker compose -f docker-compose.prod.yml restart

# Yeni versiyonu deploy et (Önerilen: Script kullan)
bash scripts/update-production.sh

# VEYA manuel olarak:
cd /opt/lifeos
git pull origin main
docker compose -f docker-compose.prod.yml up -d --build

# Disk kullanımı
docker system df

# Eski image'ları temizle
docker system prune -a
```

---

## 🆘 Sorun mu Yaşıyorsunuz?

1. **Logları kontrol edin:**
   ```bash
   docker compose -f docker-compose.prod.yml logs
   ```

2. **Container durumlarını kontrol edin:**
   ```bash
   docker compose -f docker-compose.prod.yml ps
   ```

3. **Health check'leri kontrol edin:**
   ```bash
   docker inspect lifeos_api_prod | grep -A 10 Health
   ```

4. **Detaylı dokümantasyona bakın:** [DEPLOYMENT.md](DEPLOYMENT.md)

---

**Başarılar! 🎉**

