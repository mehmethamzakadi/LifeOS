# Soft Delete ve DDD Analiz Raporu

> **Tarih:** 30 Aralık 2024  
> **Versiyon:** 1.0  
> **Analiz Tipi:** Soft Delete Best Practices ve DDD Yapılanması Değerlendirmesi

---

## 📋 İçindekiler

1. [Soft Delete Yaklaşımı Analizi](#1-soft-delete-yaklaşımı-analizi)
2. [Mevcut Durum](#2-mevcut-durum)
3. [Best Practice Karşılaştırması](#3-best-practice-karşılaştırması)
4. [DDD Yapılanması Değerlendirmesi](#4-ddd-yapılanması-değerlendirmesi)
5. [Öneriler ve Sonuç](#5-öneriler-ve-sonuç)

---

## 1. Soft Delete Yaklaşımı Analizi

### 1.1. Mevcut Yaklaşım: Entity İçinde Delete() Metodu

**Mevcut Durum:**
```csharp
// Entity içinde Delete() metodu
public void Delete()
{
    if (IsDeleted)
        throw new InvalidOperationException("Category is already deleted");
    
    IsDeleted = true;
    DeletedDate = DateTime.UtcNow;
    AddDomainEvent(new CategoryDeletedEvent(Id, Name));
}
```

**Kullanım:**
```csharp
// Command Handler'da
var category = await context.Categories.FirstOrDefaultAsync(...);
category.Delete(); // Domain metodu çağrılıyor
context.Categories.Update(category);
await unitOfWork.SaveChangesAsync(cancellationToken);
```

### 1.2. Alternatif Yaklaşım: AuditableDbContext İçinde Soft Delete

**Alternatif (Önerilmeyen):**
```csharp
// AuditableDbContext.SaveChangesAsync içinde
public override async Task<int> SaveChangesAsync(...)
{
    foreach (var entry in ChangeTracker.Entries<ISoftDeletable>())
    {
        if (entry.State == EntityState.Deleted)
        {
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedDate = DateTime.UtcNow;
        }
    }
    // ...
}
```

---

## 2. Mevcut Durum

### 2.1. Entity Delete Metodları

**✅ Doğru Uygulanan Entity'ler:**
- `User.Delete()` - IsDeleted ve DeletedDate set ediyor + Domain Event
- `Category.Delete()` - IsDeleted ve DeletedDate set ediyor + Domain Event
- `Role.Delete()` - IsDeleted ve DeletedDate set ediyor + Domain Event
- `Book.Delete()` - IsDeleted ve DeletedDate set ediyor + Domain Event
- `Game.Delete()` - IsDeleted ve DeletedDate set ediyor + Domain Event
- `MovieSeries.Delete()` - IsDeleted ve DeletedDate set ediyor + Domain Event
- `PersonalNote.Delete()` - IsDeleted ve DeletedDate set ediyor + Domain Event
- `WalletTransaction.Delete()` - IsDeleted ve DeletedDate set ediyor + Domain Event

**⚠️ Eksik Domain Event:**
- `UserRole.Delete()` - Sadece IsDeleted ve DeletedDate set ediyor, Domain Event yok

### 2.2. AuditableDbContext Durumu

**Mevcut Implementasyon:**
```csharp
public override async Task<int> SaveChangesAsync(...)
{
    // Sadece Created/Updated alanlarını set ediyor
    // Soft delete işlemi YOK - Entity'lerde Delete() metodu kullanılıyor
    foreach (var entry in ChangeTracker.Entries<BaseEntity>()
        .Where(q => q.State == EntityState.Added || 
                    q.State == EntityState.Modified || 
                    q.State == EntityState.Deleted))
    {
        if (entry.State == EntityState.Added)
        {
            entry.Entity.CreatedDate = DateTime.UtcNow;
            entry.Entity.CreatedById = effectiveUserId;
        }
        if (entry.State == EntityState.Modified)
        {
            entry.Entity.UpdatedDate = DateTime.UtcNow;
            entry.Entity.UpdatedById = effectiveUserId;
        }
        // EntityState.Deleted için bir işlem YOK
    }
}
```

**Not:** `EntityState.Deleted` kontrolü var ama soft delete işlemi yapılmıyor. Bu doğru bir yaklaşım çünkü soft delete entity'nin kendi sorumluluğu.

### 2.3. Global Query Filter

**LifeOSDbContext'te:**
```csharp
// ISoftDeletable entity'lere otomatik filter uygulanıyor
if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType) &&
    entityType.ClrType != typeof(RefreshSession))
{
    var filter = Expression.Lambda(
        Expression.Equal(property, Expression.Constant(false)), 
        parameter);
    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
}
```

**✅ Doğru:** Soft delete filter otomatik olarak uygulanıyor.

---

## 3. Best Practice Karşılaştırması

### 3.1. Yaklaşım 1: Entity İçinde Delete() Metodu (Mevcut - ✅ DOĞRU)

**Avantajlar:**
1. **Domain Logic Encapsulation:** Silme işlemi entity'nin kendi sorumluluğu
2. **Business Rules:** Entity içinde silme kuralları kontrol edilebilir (örn: alt kategori kontrolü)
3. **Domain Events:** Silme işlemi domain event olarak yayınlanabilir
4. **Testability:** Entity metodları kolayca test edilebilir
5. **DDD Compliance:** Domain-Driven Design prensiplerine uygun
6. **Explicit Intent:** `entity.Delete()` çağrısı açık ve anlaşılır
7. **Validation:** Entity içinde silme öncesi validasyon yapılabilir

**Dezavantajlar:**
1. **Code Duplication:** Her entity'de Delete() metodu yazılması gerekir (ancak bu aslında avantaj - her entity'nin kendi silme mantığı olabilir)
2. **Manuel Update:** Command handler'da `context.Update(entity)` çağrılması gerekir

**Örnek Kullanım:**
```csharp
// Command Handler
var category = await context.Categories.FirstOrDefaultAsync(...);
category.Delete(); // Domain logic
context.Categories.Update(category);
await unitOfWork.SaveChangesAsync(cancellationToken);
```

### 3.2. Yaklaşım 2: AuditableDbContext İçinde Soft Delete (❌ ÖNERİLMEZ)

**Avantajlar:**
1. **Merkezi Yönetim:** Tüm soft delete işlemleri tek yerde
2. **Otomatik:** `context.Remove(entity)` çağrısı otomatik olarak soft delete'e dönüşür
3. **Kod Tekrarı Yok:** Entity'lerde Delete() metodu yazmaya gerek yok

**Dezavantajlar:**
1. **Infrastructure Concern:** Domain logic infrastructure katmanına taşınır (Clean Architecture ihlali)
2. **Business Rules Eksikliği:** Entity'ye özel silme kuralları uygulanamaz
3. **Domain Events Eksikliği:** Silme işlemi domain event olarak yayınlanamaz
4. **Testability:** DbContext'e bağımlı test yazılması gerekir
5. **DDD Violation:** Domain-Driven Design prensiplerine aykırı
6. **Implicit Behavior:** `context.Remove()` çağrısı soft delete yapıyor gibi görünür ama aslında infrastructure tarafından dönüştürülüyor
7. **Validation Zorluğu:** Entity'ye özel validasyonlar uygulanamaz

**Örnek Kullanım:**
```csharp
// Command Handler
var category = await context.Categories.FirstOrDefaultAsync(...);
context.Categories.Remove(category); // Infrastructure soft delete'e dönüştürür
await context.SaveChangesAsync(cancellationToken);
```

### 3.3. Karşılaştırma Tablosu

| Kriter | Entity Delete() | DbContext Soft Delete |
|--------|----------------|----------------------|
| **Domain Logic Encapsulation** | ✅ | ❌ |
| **Business Rules** | ✅ | ❌ |
| **Domain Events** | ✅ | ❌ |
| **Testability** | ✅ | ⚠️ |
| **DDD Compliance** | ✅ | ❌ |
| **Clean Architecture** | ✅ | ❌ |
| **Explicit Intent** | ✅ | ❌ |
| **Merkezi Yönetim** | ⚠️ | ✅ |
| **Kod Tekrarı** | ⚠️ | ✅ |
| **Validation** | ✅ | ❌ |

### 3.4. Sonuç: Entity Delete() Metodu DOĞRU Yaklaşım

**Neden Entity Delete() Metodu Doğru?**

1. **Domain-Driven Design:** Entity'ler kendi davranışlarını yönetmelidir
2. **Clean Architecture:** Domain logic domain katmanında olmalıdır
3. **Business Rules:** Her entity'nin kendine özel silme kuralları olabilir
4. **Domain Events:** Silme işlemi domain event olarak yayınlanmalıdır
5. **Testability:** Entity metodları kolayca test edilebilir
6. **Explicit Intent:** `entity.Delete()` açık ve anlaşılır

**Neden AuditableDbContext'te Soft Delete Önerilmez?**

1. **Separation of Concerns:** Infrastructure concern domain logic'e karışmamalı
2. **Business Rules:** Entity'ye özel kurallar uygulanamaz
3. **Domain Events:** Silme işlemi domain event olarak yayınlanamaz
4. **DDD Violation:** Domain-Driven Design prensiplerine aykırı

---

## 4. DDD Yapılanması Değerlendirmesi

### 4.1. Mevcut DDD Yapılanması

**✅ Doğru Uygulanan DDD Pattern'leri:**

1. **Aggregate Root:**
   - `User` → `AggregateRoot`
   - `Category` → `AggregateRoot`
   - Diğer entity'ler → `BaseEntity`

2. **Value Objects:**
   - `Email` → Value Object (validation ve encapsulation)
   - `UserName` → Value Object (validation ve encapsulation)

3. **Domain Events:**
   - Her entity için Created/Updated/Deleted event'leri
   - `IDomainEvent` interface
   - `DomainEvent` base class

4. **Domain Services:**
   - `IUserDomainService` / `UserDomainService`
   - `IAiService` (domain abstraction)

5. **Repository Pattern:**
   - `IUnitOfWork` interface (Domain katmanında)
   - Repository implementasyonları (Persistence katmanında)

6. **Entity Encapsulation:**
   - Private setters
   - Factory methods (`Create`)
   - Domain methods (`Update`, `Delete`)

### 4.2. DDD Yapılanması Gerekli mi?

**✅ EVET - Bu Proje İçin DDD Gerekli ve Doğru Uygulanmış**

**Neden DDD Gerekli?**

1. **Complex Business Logic:**
   - Kullanıcı yönetimi, rol/permission sistemi
   - Kategori hiyerarşisi
   - Audit tracking
   - Soft delete

2. **Domain Events:**
   - Cache invalidation
   - Event-driven architecture
   - Outbox pattern

3. **Aggregate Boundaries:**
   - `User` aggregate root (UserRole, UserPermission ilişkileri)
   - `Category` aggregate root (hierarchical structure)

4. **Value Objects:**
   - `Email` ve `UserName` validation ve encapsulation

5. **Domain Services:**
   - Karmaşık business logic (örn: UserDomainService)

### 4.3. DDD Yapılanması İyileştirme Önerileri

**⚠️ İyileştirilmesi Gerekenler:**

1. **AggregateRoot Kullanımı Tutarsızlığı:**
   - `Book`, `Game`, `MovieSeries` → `BaseEntity` (ama `Category` → `AggregateRoot`)
   - **Sorun:** Hangi entity'ler aggregate root olmalı?
   - **Öneri:** Her entity'nin aggregate root olup olmadığını domain uzmanlarıyla belirleyin

2. **ValueObject Kullanımı:**
   - `User` entity'sinde `Email` ve `UserName` string olarak tutuluyor
   - **Sorun:** Value Object'ler tanımlı ama kullanılmıyor
   - **Öneri:** `User` entity'sinde `Email` ve `UserName` Value Object olarak kullanılmalı

3. **DomainEventNotification:**
   - `DomainEventNotification` MediatR bağımlılığı Domain katmanında
   - **Sorun:** Domain katmanı infrastructure bağımlılığı içermemeli
   - **Öneri:** `DomainEventNotification` Application veya Infrastructure katmanına taşınmalı

4. **Common/Requests ve Common/Responses:**
   - `DataGridRequest`, `PaginatedRequest`, `PaginatedListResponse` Domain katmanında
   - **Sorun:** Bu sınıflar Application katmanında olmalı
   - **Öneri:** Application katmanına taşınmalı

5. **ApiResult:**
   - `ApiResult`, `IResult`, `DataResult` Domain katmanında
   - **Sorun:** API response modelleri Domain katmanında olmamalı
   - **Öneri:** Application veya API katmanına taşınmalı

### 4.4. DDD Yapılanması Özeti

**✅ Güçlü Yönler:**
- Clean Architecture katmanları doğru ayrılmış
- Domain katmanı saf (pure) - dış bağımlılık minimal
- Aggregate Root pattern kullanılıyor
- Value Objects tanımlı
- Domain Events implementasyonu var
- Entity encapsulation doğru uygulanmış

**⚠️ İyileştirme Alanları:**
- AggregateRoot kullanımı tutarlı hale getirilmeli
- Value Objects entity'lerde kullanılmalı
- Domain katmanından infrastructure bağımlılıkları kaldırılmalı
- Application/API modelleri Domain katmanından taşınmalı

---

## 5. Öneriler ve Sonuç

### 5.1. Soft Delete İçin Öneriler

**✅ MEVCUT YAKLAŞIM DOĞRU - Değişiklik Gereksiz**

1. **Entity Delete() Metodları Korunmalı:**
   - Domain logic entity içinde kalmalı
   - Business rules entity içinde kontrol edilmeli
   - Domain events entity içinde yayınlanmalı

2. **AuditableDbContext'e Soft Delete Eklenmemeli:**
   - Infrastructure concern domain logic'e karışmamalı
   - Clean Architecture prensiplerine aykırı

3. **İyileştirme:**
   - `UserRole.Delete()` metoduna domain event eklenebilir (opsiyonel)
   - Tüm entity'lerde Delete() metodları tutarlı hale getirilmeli

### 5.2. DDD Yapılanması İçin Öneriler

**✅ DDD YAPILANMASI GEREKLİ VE DOĞRU UYGULANMIŞ**

1. **Mevcut Yapı Korunmalı:**
   - Clean Architecture katmanları doğru
   - Domain katmanı saf (pure)
   - DDD pattern'leri doğru uygulanmış

2. **İyileştirmeler:**
   - AggregateRoot kullanımı tutarlı hale getirilmeli
   - Value Objects entity'lerde kullanılmalı
   - Domain katmanından infrastructure bağımlılıkları kaldırılmalı
   - Application/API modelleri Domain katmanından taşınmalı

### 5.3. Sonuç

**Soft Delete:**
- ✅ **Mevcut yaklaşım (Entity Delete() metodu) DOĞRU**
- ❌ **AuditableDbContext'te soft delete ÖNERİLMEZ**
- ✅ **Değişiklik gereksiz - mevcut yapı best practice'lere uygun**

**DDD Yapılanması:**
- ✅ **DDD yapılanması GEREKLİ ve DOĞRU uygulanmış**
- ⚠️ **Bazı iyileştirmeler yapılabilir (yukarıdaki öneriler)**
- ✅ **Genel yapı Clean Architecture ve DDD prensiplerine uygun**

---

## 6. Referanslar

- [Domain-Driven Design (Eric Evans)](https://www.domainlanguage.com/ddd/)
- [Clean Architecture (Robert C. Martin)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Entity Framework Core Soft Delete](https://learn.microsoft.com/en-us/ef/core/querying/filters)
- [DDD Best Practices](https://www.domainlanguage.com/ddd/patterns/)

---

**Son Güncelleme:** 30 Aralık 2024

