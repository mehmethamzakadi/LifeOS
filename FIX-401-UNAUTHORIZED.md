# 401 Unauthorized Hatası - Çözüm

## Sorun
Login başarılı ama sonraki isteklerde 401 (Unauthorized) hatası alınıyor.

## Olası Nedenler

### 1. Production'da HTTPS Redirection Aktif
Production'da HTTP istekleri HTTPS'e yönlendiriliyor, bu cookie'lerin çalışmamasına neden olabilir.

**Çözüm:** HTTPS redirection'ı devre dışı bırakın (SSL sertifikası yoksa):

```bash
# API loglarını kontrol edin
docker logs --tail 50 lifeos_api_prod | grep -i "401\|unauthorized\|token"
```

### 2. Token Cookie Ayarları
Token cookie'lerinin domain ve secure ayarları yanlış olabilir.

### 3. Browser Console'da Kontrol
Browser console'da (F12) Network sekmesinde:
- Request headers'da `Authorization: Bearer ...` var mı?
- Response headers'da cookie set ediliyor mu?

## Hızlı Test

Sunucuda API loglarını kontrol edin:

```bash
cd /opt/lifeos

# API loglarını izleyin (login sonrası 401 hatası alan istekleri görmek için)
docker logs -f lifeos_api_prod | grep -i "401\|unauthorized\|token\|authorization"
```

Browser'da (F12 -> Network):
1. Login isteğini kontrol edin - Response'da `Set-Cookie` header'ı var mı?
2. Sonraki bir isteği kontrol edin - Request'te `Authorization` header'ı var mı?
3. Console'da hata mesajları var mı?

## Geçici Çözüm: HTTPS Redirection'ı Devre Dışı Bırakma

Eğer SSL sertifikası yoksa, production'da HTTPS redirection'ı devre dışı bırakmak gerekebilir. Ama bu güvenlik açığı yaratır, sadece test için kullanın!

**ÖNEMLİ:** Bu sadece test için! Production'da mutlaka SSL sertifikası kullanın!

