# Ollama Model Kurulum Rehberi

## 📋 Genel Bilgi

Ollama modelleri Docker volume'unda (`lifeos_ollama_local`) saklanır. Bu volume silinirse veya container yeniden oluşturulursa modeller de silinir.

## 🔍 Model Durumunu Kontrol Etme

```bash
# Ollama container'ında yüklü modelleri listele
docker exec lifeos_ollama_dev ollama list

# Volume'un varlığını kontrol et
docker volume ls | grep ollama

# Volume içeriğini kontrol et (opsiyonel)
docker volume inspect lifeos_ollama_local
```

## 📥 Model Yükleme

### Yöntem 1: Manuel Yükleme (Önerilen)

```bash
# qwen2.5:1.5b modelini yükle (yaklaşık 4-5 GB, 5-10 dakika sürebilir)
docker exec lifeos_ollama_dev ollama pull qwen2.5:1.5b

# Daha küçük alternatif model (yaklaşık 2 GB, daha hızlı)
docker exec lifeos_ollama_dev ollama pull qwen2.5:3b
```

### Yöntem 2: Daha Küçük Model Kullanma

Eğer `qwen2.5:1.5b` çok büyükse, daha küçük bir model kullanabilirsiniz:

1. `appsettings.Development.json` veya `docker-compose.local.yml` dosyasında `ModelId`'yi değiştirin:
   ```json
   "OllamaOptions": {
     "ModelId": "qwen2.5:3b"  // 7b yerine 3b
   }
   ```

2. Modeli yükleyin:
   ```bash
   docker exec lifeos_ollama_dev ollama pull qwen2.5:3b
   ```

## ⚠️ Önemli Notlar

### Volume Yönetimi

1. **Container Durdurma**: Container durdurulduğunda (`docker stop`) volume korunur, modeller kaybolmaz.

2. **Container Silme**: Container silindiğinde (`docker rm`) volume korunur, modeller kaybolmaz.

3. **Volume Silme**: Volume silindiğinde (`docker volume rm lifeos_ollama_local`) **TÜM MODELLER SİLİNİR**.

4. **`docker-compose down -v`**: `-v` flag'i ile çalıştırılırsa **TÜM VOLUMELER SİLİNİR**, modeller kaybolur.

### Volume'u Korumak İçin

```bash
# Container'ları durdur (volume korunur)
docker-compose -f docker-compose.local.yml down

# Volume'ları da silmek isterseniz (DİKKAT: Modeller silinir!)
docker-compose -f docker-compose.local.yml down -v
```

## 🔄 Model Yeniden Yükleme

Eğer model silinmişse veya yeni bir model denemek isterseniz:

```bash
# Mevcut modeli sil (opsiyonel)
docker exec lifeos_ollama_dev ollama rm qwen2.5:1.5b

# Yeni modeli yükle
docker exec lifeos_ollama_dev ollama pull qwen2.5:1.5b
```

## 📊 Model Boyutları ve Öneriler

| Model | Boyut | Önerilen Kullanım |
|-------|-------|-------------------|
| `qwen2.5:1.5b` | ~4-5 GB | Production, yüksek kalite |
| `qwen2.5:3b` | ~2 GB | Development, hızlı test |
| `qwen2.5:1.5b` | ~1 GB | Hızlı test, düşük kaynak |

## 🐛 Sorun Giderme

### Model Bulunamadı Hatası

**Hata:**
```
"error": "model 'qwen2.5:1.5b' not found"
```

**Çözüm:**
```bash
# 1. Container'ın çalıştığını kontrol et
docker ps | grep ollama

# 2. Modeli yükle
docker exec lifeos_ollama_dev ollama pull qwen2.5:1.5b

# 3. Yüklü modelleri kontrol et
docker exec lifeos_ollama_dev ollama list
```

### Volume Boş Görünüyor

Eğer volume var ama modeller görünmüyorsa:

```bash
# Volume'u kontrol et
docker volume inspect lifeos_ollama_local

# Container'ı yeniden başlat
docker-compose -f docker-compose.local.yml restart ollama

# Modeli tekrar yükle
docker exec lifeos_ollama_dev ollama pull qwen2.5:1.5b
```

## 💡 Best Practices

1. **Volume Yedekleme**: Önemli modeller için volume'u yedekleyin:
   ```bash
   docker run --rm -v lifeos_ollama_local:/data -v $(pwd):/backup alpine tar czf /backup/ollama-backup.tar.gz -C /data .
   ```

2. **Model Seçimi**: Development için küçük model, production için büyük model kullanın.

3. **Disk Alanı**: Modeller büyük olduğu için disk alanını kontrol edin:
   ```bash
   docker system df
   ```

## 📚 Ek Kaynaklar

- [Ollama Model Listesi](https://ollama.ai/library)
- [Ollama Docker Documentation](https://github.com/ollama/ollama/blob/main/docs/docker.md)
