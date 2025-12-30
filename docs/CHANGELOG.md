# LifeOS - Değişiklik Geçmişi (Changelog)

> **Son Güncelleme:** 2 Aralık 2025  
> **Versiyon:** 1.6

---

## 📋 Genel Bakış

Bu dosya LifeOS projesindeki önemli değişiklikleri, iyileştirmeleri ve yeni özellikleri takip eder.

## 🎯 Tamamlanan İyileştirmeler

### v1.6 (2 Aralık 2025)

#### ✅ RedisCacheService Refactoring
- **Durum:** Redis'te `WRONGTYPE` hatası kalıcı olarak çözüldü
- **Yapılan İşlem:** `RedisCacheService` tamamen `IConnectionMultiplexer` kullanacak şekilde refactor edildi
  - `IDistributedCache` bağımlılığı kaldırıldı
  - Tutarlı String veri tipi garantisi
  - Performans iyileştirmesi (`KeyExistsAsync` kullanımı)

#### ✅ ActivityLogConsumer Race Condition Düzeltmesi
- **Durum:** Merkezi Idempotency Service eklendi
- **Yapılan İşlem:**
  - `IIdempotencyService` interface oluşturuldu
  - `IdempotencyFilter<TMessage>` MassTransit filter eklendi
  - ActivityLogConsumer basitleştirildi (~100 satır kod azaldı)
  - SOLID ve Clean Code prensipleri uygulandı

### v1.5 (30 Kasım 2025)

#### ✅ OpenTelemetry ve Jaeger Entegrasyonu
- **Durum:** Dağıtık sistem takibi için OpenTelemetry altyapısı kuruldu
- **Yapılan İşlem:**
  - Jaeger servisi docker-compose.local.yml'e eklendi
  - OTLP exporter entegrasyonu
  - HTTP Request, EF Core, MassTransit tracing
  - Docker ve Local ortam desteği

#### ✅ Serilog ve Seq İyileştirmeleri
- **Durum:** Log yönetimi optimize edildi
- **Yapılan İşlem:**
  - Docker ve Local ortam ayrımı
  - Ortam bazlı log seviyesi optimizasyonu
  - Environment variable desteği

### v1.4 (30 Kasım 2025)

#### ✅ Yapay Zeka Destekli İçerik Üretme
- **Durum:** Ollama (Qwen 2.5:7b) entegrasyonu eklendi
- **Yapılan İşlem:**
  - `IAiService` interface'i Domain katmanına eklendi
  - `AiService` implementasyonu Infrastructure katmanına eklendi
  - Best practices uygulandı (IHttpClientFactory, Polly retry policy)
  - Frontend'e "Yapay Zeka ile Üret ✨" butonu eklendi
  - Docker Compose'a Ollama servisi eklendi

#### ✅ Docker Compose ve PermissionSeeder İyileştirmeleri
- OllamaOptions environment variables eklendi
- Redis connection string düzeltildi
- PermissionSeeder duplicate key sorunu çözüldü

### v1.3 (28 Kasım 2025)

#### ✅ Domain Katmanı Temizliği (Clean Architecture)
- **Durum:** Domain katmanı dış bağımlılıklardan arındırıldı
- **Yapılan İşlem:**
  - `Microsoft.EntityFrameworkCore` ve `System.Linq.Dynamic.Core` bağımlılıkları kaldırıldı
  - `IIncludableQueryable` yerine `IQueryable` yapısına geçildi
  - Extension metodlar Persistence katmanına taşındı

#### ✅ N+1 Performans Sorunu Çözümü
- `UserRepository.GetRolesAsync` optimize edildi
- Projection kullanımı ile tek sorguda veri çekilmesi sağlandı

#### ✅ Extension Method Refactoring
- `IQueryablePaginateExtensions` -> `LifeOS.Persistence.Extensions`
- `IQueryableDynamicFilterExtensions` -> `LifeOS.Persistence.Extensions`

---

## 📊 İlerleme Takibi

### Tamamlanan Görevler

| ID       | Görev                              | Tarih      | Durum                                          |
| -------- | ---------------------------------- | ---------- | ---------------------------------------------- |
| SEC-002  | Domain katmanı temizliği           | 28.11.2025 | ✅ Tamamlandı (EF Core kaldırıldı)             |
| PERF-003 | N+1 Sorunu                         | 28.11.2025 | ✅ Tamamlandı (UserRepository optimize edildi) |
| ARCH-003 | Extension Metod Taşıma             | 28.11.2025 | ✅ Tamamlandı (Persistence'a taşındı)          |
| FEAT-001 | Ollama AI Entegrasyonu             | 30.11.2025 | ✅ Tamamlandı (Best practices ile)             |
| ARCH-004 | Models Klasör Yapısı               | 30.11.2025 | ✅ Tamamlandı (Separation of Concerns)         |
| FEAT-002 | OpenTelemetry/Jaeger Entegrasyonu  | 30.11.2025 | ✅ Tamamlandı (Trace görselleştirme)           |
| FEAT-003 | Serilog/Seq İyileştirmeleri        | 30.11.2025 | ✅ Tamamlandı (Docker/Local ortam desteği)     |
| FIX-001  | ActivityLogConsumer Race Condition | 02.12.2025 | ✅ Tamamlandı (Merkezi idempotency service)    |
| ARCH-005 | Merkezi Idempotency Service        | 02.12.2025 | ✅ Tamamlandı (SOLID, Clean Code)              |
| FIX-002  | RedisCacheService WRONGTYPE Hatası | 02.12.2025 | ✅ Tamamlandı (IConnectionMultiplexer refactoring) |
| ARCH-006 | RedisCacheService Refactoring      | 02.12.2025 | ✅ Tamamlandı (Tutarlı veri tipi, performans) |

---

## 🔜 Kalan İşler ve Sonraki Adımlar

### Öncelik: 🟠 Yüksek (Test Coverage)

- [ ] **TEST-001:** Domain Entity testleri yazılmalı (User, Post aggregate roots).
- [ ] **TEST-002:** Application Command/Query handler testleri yazılmalı.

### Öncelik: 🟡 Orta (Frontend & Refactoring)

- [ ] **FE-001:** Frontend hata yönetimi (Error Boundary).
- [ ] **ARCH-002:** Interface Segregation (IReadRepository / IWriteRepository ayrımı - Opsiyonel ama önerilir).

---

## 📝 Notlar

Detaylı teknik analiz ve mevcut durum değerlendirmesi için [DETAILED_PROJECT_ANALYSIS.md](./DETAILED_PROJECT_ANALYSIS.md) dosyasına bakın.
