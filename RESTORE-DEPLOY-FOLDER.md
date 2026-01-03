# 🔄 Deploy Klasörünü Geri Alma Rehberi

Bu doküman, sunucuda yanlışlıkla silinen `deploy/` klasörünün nasıl geri alınacağını açıklar.

## 🚀 Hızlı Çözüm (Otomatik Script)

### Yöntem 1: Otomatik Script (Önerilen)

Proje kök dizininde:

```bash
# Script'i çalıştır
bash scripts/restore-deploy-folder.sh

# Veya özel sunucu bilgileri ile:
bash scripts/restore-deploy-folder.sh lifeos 45.143.4.244 /opt/lifeos
```

Script otomatik olarak:
- ✅ `deploy/nginx/default.conf` dosyasını kopyalar
- ✅ `deploy/nginx/nginx.conf` dosyasını kopyalar
- ✅ `deploy/nginx/ssl/` klasörünü oluşturur (yoksa)
- ✅ `deploy/nginx/certbot/` klasörünü oluşturur (yoksa)
- ✅ Dosya izinlerini ayarlar

---

## 📋 Manuel Yöntemler

### Yöntem 2: SCP ile Kopyalama

Yerel makinenizden:

```bash
# Sunucuya bağlan
ssh lifeos@45.143.4.244

# deploy klasörünü oluştur
mkdir -p /opt/lifeos/deploy/nginx

# Çık (exit)
exit

# Yerel makinenizden dosyaları kopyala
cd "/Users/mehmethamzakadi/Desktop/GitHub Projects/LifeOS"

scp deploy/nginx/default.conf lifeos@45.143.4.244:/opt/lifeos/deploy/nginx/
scp deploy/nginx/nginx.conf lifeos@45.143.4.244:/opt/lifeos/deploy/nginx/
```

### Yöntem 3: Git ile Geri Alma

Eğer proje Git repository'si ise:

```bash
# Sunucuda
ssh lifeos@45.143.4.244
cd /opt/lifeos

# Git'ten dosyaları geri al
git checkout HEAD -- deploy/nginx/default.conf
git checkout HEAD -- deploy/nginx/nginx.conf

# Veya tüm deploy klasörünü
git checkout HEAD -- deploy/
```

### Yöntem 4: GitHub'dan Çekme

Eğer değişiklikler GitHub'a push edilmişse:

```bash
# Sunucuda
ssh lifeos@45.143.4.244
cd /opt/lifeos

# En son değişiklikleri çek
git pull origin main

# Veya sadece deploy klasörünü
git checkout origin/main -- deploy/
```

---

## 🔒 SSL Sertifikalarını Geri Alma

⚠️ **ÖNEMLİ:** SSL sertifikaları Git'te değil, sunucuda `/etc/letsencrypt/` klasöründe saklanır.

Eğer SSL sertifikaları da silindiyse:

### Let's Encrypt Sertifikaları Varsa

```bash
ssh lifeos@45.143.4.244
cd /opt/lifeos

# SSL klasörünü oluştur
mkdir -p deploy/nginx/ssl
chmod 700 deploy/nginx/ssl

# Sertifikaları kopyala
sudo cp /etc/letsencrypt/live/liferegistry.app/fullchain.pem deploy/nginx/ssl/
sudo cp /etc/letsencrypt/live/liferegistry.app/privkey.pem deploy/nginx/ssl/

# İzinleri ayarla
sudo chown -R $(whoami):$(whoami) deploy/nginx/ssl
chmod 600 deploy/nginx/ssl/*.pem
```

### SSL Sertifikaları Yoksa (Yeniden Oluşturma)

```bash
# Certbot ile yeni sertifika al
sudo certbot certonly --standalone -d liferegistry.app -d www.liferegistry.app

# Sertifikaları deploy/nginx/ssl/ klasörüne kopyala
mkdir -p /opt/lifeos/deploy/nginx/ssl
sudo cp /etc/letsencrypt/live/liferegistry.app/fullchain.pem /opt/lifeos/deploy/nginx/ssl/
sudo cp /etc/letsencrypt/live/liferegistry.app/privkey.pem /opt/lifeos/deploy/nginx/ssl/
sudo chown -R $(whoami):$(whoami) /opt/lifeos/deploy/nginx/ssl
chmod 600 /opt/lifeos/deploy/nginx/ssl/*.pem
```

---

## ✅ Kontrol ve Test

### 1. Dosyaları Kontrol Et

```bash
ssh lifeos@45.143.4.244
cd /opt/lifeos

# Dosyaların varlığını kontrol et
ls -la deploy/nginx/

# Çıktı şöyle olmalı:
# default.conf
# nginx.conf
# ssl/ (klasör)
# certbot/ (klasör)
```

### 2. Nginx Konfigürasyonunu Test Et

```bash
# Nginx container'ı içinde test et
docker-compose -f docker-compose.yml -f docker-compose.prod.yml exec nginx nginx -t

# Çıktı şöyle olmalı:
# nginx: the configuration file /etc/nginx/nginx.conf syntax is ok
# nginx: configuration file /etc/nginx/nginx.conf test is successful
```

### 3. Container'ları Yeniden Başlat

```bash
cd /opt/lifeos

# Sadece nginx container'ını yeniden başlat
docker-compose -f docker-compose.yml -f docker-compose.prod.yml restart nginx

# Veya tüm servisleri yeniden başlat
docker-compose -f docker-compose.yml -f docker-compose.prod.yml restart
```

### 4. Container Loglarını Kontrol Et

```bash
# Nginx loglarını kontrol et
docker-compose -f docker-compose.yml -f docker-compose.prod.yml logs nginx

# Hata varsa göreceksiniz
```

---

## 🆘 Sorun Giderme

### Hata: "No such file or directory"

**Sorun:** `deploy/nginx/` klasörü yok.

**Çözüm:**
```bash
ssh lifeos@45.143.4.244
cd /opt/lifeos
mkdir -p deploy/nginx
```

### Hata: "Permission denied"

**Sorun:** Dosya izinleri yanlış.

**Çözüm:**
```bash
ssh lifeos@45.143.4.244
cd /opt/lifeos
chmod 644 deploy/nginx/*.conf
chmod 755 deploy/nginx/ssl
chmod 755 deploy/nginx/certbot
```

### Hata: Nginx container başlamıyor

**Sorun:** Konfigürasyon dosyasında hata var.

**Çözüm:**
```bash
# Nginx konfigürasyonunu test et
docker-compose -f docker-compose.yml -f docker-compose.prod.yml exec nginx nginx -t

# Hata mesajını oku ve düzelt
# Genellikle syntax hatası veya dosya yolu hatası olur
```

### Hata: SSL sertifikası bulunamadı

**Sorun:** SSL dosyaları yok veya yanlış yolda.

**Çözüm:**
```bash
# SSL klasörünü kontrol et
ls -la deploy/nginx/ssl/

# Eğer boşsa, sertifikaları kopyala (yukarıdaki SSL bölümüne bak)
```

---

## 📝 Özet Adımlar

1. ✅ Konfigürasyon dosyalarını geri al (default.conf, nginx.conf)
2. ✅ SSL klasörlerini oluştur (ssl/, certbot/)
3. ✅ SSL sertifikalarını kopyala (eğer varsa)
4. ✅ Dosya izinlerini ayarla
5. ✅ Nginx konfigürasyonunu test et
6. ✅ Container'ları yeniden başlat
7. ✅ Logları kontrol et

---

**Son Güncelleme:** 2025-01-02

