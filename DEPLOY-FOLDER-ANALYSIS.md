# 📁 deploy/ Klasörü Analizi

Bu doküman, `deploy/` klasörünün projedeki kullanımını ve gerekliliğini açıklar.

## ✅ Kullanım Durumu

### Production Ortamında (docker-compose.prod.yml)

**KESINLIKLE GEREKLİ!** `deploy/` klasörü production ortamında kullanılıyor:

```yaml
# docker-compose.prod.yml - Satır 65-88
nginx:
  image: nginx:alpine
  container_name: lifeos_nginx
  volumes:
    - ./deploy/nginx/default.conf:/etc/nginx/conf.d/default.conf:ro
    - ./deploy/nginx/nginx.conf:/etc/nginx/nginx.conf:ro
    - ./deploy/nginx/ssl:/etc/nginx/ssl:ro
    - ./deploy/nginx/certbot:/var/www/certbot:ro
```

**Mount Edilen Dosyalar:**
1. ✅ `deploy/nginx/default.conf` → Nginx server block konfigürasyonu (API routing, SSL, rate limiting)
2. ✅ `deploy/nginx/nginx.conf` → Nginx ana konfigürasyonu (worker processes, logging, gzip)
3. ✅ `deploy/nginx/ssl/` → SSL sertifikaları klasörü (fullchain.pem, privkey.pem)
4. ✅ `deploy/nginx/certbot/` → Let's Encrypt challenge klasörü

### Local Development'ta (docker-compose.local.yml)

**KULLANILMIYOR!** Local development'ta `deploy/` klasörü kullanılmıyor çünkü:
- Nginx reverse proxy yok
- Client direkt Vite dev server üzerinden çalışıyor (port 5173)
- API direkt port 6060'da expose ediliyor
- SSL kullanılmıyor

## 📋 Dosyaların İşlevleri

### 1. deploy/nginx/default.conf

**İşlev:**
- Reverse proxy konfigürasyonu (API ve Client routing)
- SSL/HTTPS yapılandırması
- Rate limiting (API, login, register endpoints)
- Security headers (HSTS, CSP, X-Frame-Options, vb.)
- HTTP → HTTPS yönlendirme
- Let's Encrypt challenge endpoint

**İçerik Özeti:**
- Upstream tanımları (lifeos_api, lifeos_client)
- HTTP server block (port 80 - HTTPS'e yönlendirme)
- HTTPS server block (port 443 - Ana server)
- API endpoint routing (`/api/`)
- Client routing (`/`)
- Rate limiting zones
- Security headers

### 2. deploy/nginx/nginx.conf

**İşlev:**
- Nginx ana konfigürasyonu
- Worker processes ayarları
- Logging formatları
- Gzip sıkıştırma
- Performans optimizasyonları
- Proxy ayarları

**İçerik Özeti:**
- Worker processes: auto
- Worker connections: 4096
- Log formats (main, json)
- Gzip ayarları
- Timeout ayarları
- Server tokens: off (security)

### 3. deploy/nginx/ssl/

**İşlev:**
- SSL sertifikalarının saklandığı klasör
- Let's Encrypt sertifikaları buraya kopyalanır
- Dosyalar: `fullchain.pem`, `privkey.pem`

**Önemli:**
- Bu klasör `.gitignore`'da (güvenlik için)
- Sunucuda manuel olarak oluşturulmalı
- Certbot ile alınan sertifikalar buraya kopyalanır

### 4. deploy/nginx/certbot/

**İşlev:**
- Let's Encrypt HTTP-01 challenge için
- Certbot'un `.well-known/acme-challenge/` endpoint'i için
- Genellikle boş kalır (challenge sırasında kullanılır)

## 🔒 Güvenlik Notları

### .gitignore Durumu

Aşağıdaki klasörler `.gitignore`'da:
- ✅ `deploy/nginx/ssl/` - SSL private key'ler içerir
- ✅ `deploy/nginx/certbot/` - Challenge dosyaları (geçici)

Aşağıdaki dosyalar Git'te olmalı (template olarak):
- ✅ `deploy/nginx/default.conf` - Konfigürasyon şablonu
- ✅ `deploy/nginx/nginx.conf` - Konfigürasyon şablonu

## 📦 Sunucuda Olması Gereken Dosyalar

### Zorunlu Dosyalar (Git'ten gelir)

```bash
/opt/lifeos/deploy/nginx/default.conf
/opt/lifeos/deploy/nginx/nginx.conf
```

### Oluşturulması Gereken Klasörler (Sunucuda)

```bash
mkdir -p deploy/nginx/ssl
mkdir -p deploy/nginx/certbot
chmod 700 deploy/nginx/ssl
chmod 755 deploy/nginx/certbot
```

### SSL Sertifikaları (Sunucuda Oluşturulur)

```bash
# Let's Encrypt ile alınan sertifikalar buraya kopyalanır:
deploy/nginx/ssl/fullchain.pem
deploy/nginx/ssl/privkey.pem
```

## ❓ Sorular ve Cevaplar

### Q: deploy/ klasörü sunucuda olmak zorunda mı?

**A: EVET!** Production ortamında (`make prod` veya `docker-compose -f docker-compose.prod.yml`) çalıştırdığınızda Nginx container'ı bu dosyaları mount ediyor. Dosyalar yoksa container başlamaz veya yanlış konfigürasyonla çalışır.

### Q: Development'ta da gerekli mi?

**A: HAYIR!** Local development'ta (`make dev` veya `docker-compose.local.yml`) Nginx reverse proxy kullanılmıyor, bu yüzden `deploy/` klasörü gerekli değil.

### Q: Git'te olmalı mı?

**A: Konfigürasyon dosyaları EVET, SSL dosyaları HAYIR:**
- ✅ `default.conf` ve `nginx.conf` → Git'te olmalı (template olarak)
- ❌ `ssl/` klasörü → Git'te OLMAMALI (`.gitignore`'da)
- ❌ `certbot/` klasörü → Git'te OLMAMALI (`.gitignore`'da)

### Q: Silinebilir mi?

**A: HAYIR!** Production deployment için zorunlu. Sadece local development'ta çalışıyorsanız ve production'a deploy etmeyecekseniz silinebilir, ama bu durumda proje production'a deploy edilemez.

## 🚀 Deployment Checklist

Production'a deploy ederken:

- [ ] `deploy/nginx/default.conf` dosyası var mı?
- [ ] `deploy/nginx/nginx.conf` dosyası var mı?
- [ ] `deploy/nginx/ssl/` klasörü oluşturulmuş mu?
- [ ] `deploy/nginx/certbot/` klasörü oluşturulmuş mu?
- [ ] SSL sertifikaları `deploy/nginx/ssl/` klasöründe mi?
- [ ] Docker Compose volume mount'ları doğru mu?

## 📝 Özet

| Özellik | Durum | Açıklama |
|---------|-------|----------|
| **Production'da Kullanım** | ✅ Zorunlu | docker-compose.prod.yml'de mount ediliyor |
| **Development'ta Kullanım** | ❌ Gereksiz | Local development'ta Nginx yok |
| **Git'te Olmalı** | ✅ Konfigürasyon dosyaları | default.conf, nginx.conf |
| **Git'te Olmamalı** | ✅ SSL dosyaları | ssl/, certbot/ klasörleri |
| **Sunucuda Zorunlu** | ✅ Evet | Production deployment için gerekli |

---

**Son Güncelleme:** 2025-01-02

