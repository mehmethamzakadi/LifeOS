# LifeOS React Client

Modern, production-ready React istemcisi. LifeOS REST API ile tam entegre edilmiş, TypeScript ve modern React teknolojileri kullanılarak geliştirilmiştir.

## 🚀 Hızlı Başlangıç

### Docker ile Kurulum (Önerilen)

Client uygulaması Docker ile otomatik olarak build edilir ve Nginx ile serve edilir:

```bash
# Proje kök dizininden tüm servisleri başlat (Backend + Frontend)
cd ../..  # Proje kök dizinine dön
docker compose -f docker-compose.yml -f docker-compose.local.yml up --build -d

# Sadece client servisini başlatmak için
docker compose -f docker-compose.local.yml up --build -d lifeos.client

# Client loglarını izle
docker compose -f docker-compose.local.yml logs -f lifeos.client
```

**Erişim:**
- **Client UI:** http://localhost:5173
- **Backend API:** http://localhost:6060

### Manuel Kurulum (Development)

#### Gereksinimler
- Node.js 18+ 
- npm veya yarn

#### Kurulum Adımları

1. **Bağımlılıkları yükleyin:**
   ```bash
   npm install
   ```

2. **Ortam değişkenlerini yapılandırın:**
   
   Vite otomatik olarak ortam bazlı `.env` dosyalarını yükler:
   - `.env.development` - Development ortamı için (npm run dev)
   - `.env.production` - Production build için (npm run build)
   
   Bu dosyalar zaten oluşturulmuştur. Gerekirse API URL'lerini güncelleyin:
   
   **Development (.env.development):**
   ```env
   VITE_API_URL=http://localhost:6060  # Docker Compose için
   # veya
   VITE_API_URL=http://localhost:5285  # Local .NET için
   ```
   
   **Production (.env.production):**
   ```env
   VITE_API_URL=https://api.yourdomain.com
   ```

3. **Geliştirme sunucusunu başlatın:**
   ```bash
   npm run dev
   ```
   
   Uygulama varsayılan olarak `http://localhost:5173` adresinde çalışacaktır.

### Production Build

#### Docker ile (Önerilen)

Docker build sırasında otomatik olarak production build yapılır:

```bash
# Docker Compose ile build
docker compose -f docker-compose.local.yml build lifeos.client

# Veya production için
docker compose -f docker-compose.prod.yml build lifeos.client
```

**Not:** Docker build sırasında `VITE_API_URL` build argümanı olarak geçilir. `docker-compose.local.yml` veya `docker-compose.prod.yml` dosyalarında bu değeri güncelleyebilirsiniz.

#### Manuel Build

```bash
# Production build (production mode)
npm run build
# veya
npm run build:prod

# Development build (development mode - test için)
npm run build:dev
```

Build çıktıları `dist/` klasöründe oluşturulur.

**Not:** Production build için `.env.production` dosyasındaki `VITE_API_URL` değerini production API URL'inize göre güncelleyin.

## 🛠️ Teknoloji Stack

### Core
- **React 18** - Modern React hooks ve features
- **TypeScript** - Type-safe development
- **Vite** - Lightning-fast build tool

### UI/UX
- **TailwindCSS** - Utility-first CSS framework
- **shadcn/ui** - High-quality React components
- **Lucide React** - Beautiful icon library
- **Framer Motion** - Smooth animations
- **React Hot Toast** - Elegant notifications

### State Management & Data Fetching
- **Zustand** - Lightweight state management (auth store)
- **TanStack Query (React Query)** - Server state management
- **Axios** - HTTP client with interceptors

### Routing & Forms
- **React Router v7** - Client-side routing
- **React Hook Form** - Performant form handling
- **Zod** - Runtime type validation

### Data Visualization
- **TanStack Table** - Powerful table component
- **Recharts** - Responsive charts
- **date-fns** - Date manipulation

## 📁 Proje Yapısı

```
src/
├── components/          # Reusable UI components
│   ├── ui/             # shadcn/ui components
│   ├── layout/         # Layout components (Header, Sidebar, etc.)
│   └── ...
├── features/           # Feature-based modules
│   ├── auth/           # Authentication (Login, Register, etc.)
│   ├── posts/          # Blog post management
│   ├── categories/     # Category management
│   └── dashboard/      # Dashboard & analytics
├── lib/               # Utility libraries
│   ├── api/           # API client & endpoints
│   ├── hooks/         # Custom React hooks
│   └── utils/         # Helper functions
├── store/             # Zustand stores
│   └── authStore.ts   # Authentication state
├── types/             # TypeScript type definitions
├── App.tsx            # Main app component
└── main.tsx           # Entry point
```

## 🎨 Özellikler

### ✅ Kimlik Doğrulama
- JWT-based authentication
- Automatic token refresh
- Protected routes
- Persistent login state (localStorage)
- Axios interceptors for auth headers

### ✅ Blog Yönetimi
- Post CRUD operations (Create, Read, Update, Delete)
- Rich text editing support
- Image upload
- Category assignment
- Tag management
- Draft/publish states

### ✅ Kategori Yönetimi
- Server-side sorting, filtering, pagination
- TanStack Table integration
- Real-time search
- Bulk operations

### ✅ Dashboard & Analytics
- Activity logs monitoring
- User statistics
- Charts and visualizations (Recharts)
- Recent activities feed

### ✅ UI/UX
- Responsive design (mobile-first)
- Dark mode support (optional)
- Loading states & skeletons
- Error handling with toast notifications
- Accessible components (ARIA compliant)

## 🔧 Yapılandırma

### Docker Yapılandırması

Client Dockerfile'ı multi-stage build kullanır:
1. **Build Stage:** Node.js ile React uygulaması build edilir
2. **Production Stage:** Nginx ile build edilmiş dosyalar serve edilir

**Docker Build Arguments:**
- `VITE_API_URL`: API endpoint URL'i (build-time environment variable)

**Örnek Docker Build:**
```bash
docker build \
  --build-arg VITE_API_URL=http://localhost:6060 \
  -t lifeos-client:latest \
  -f Dockerfile .
```

### Environment Variables

#### Manuel Development

Vite otomatik olarak ortam bazlı environment variable dosyalarını yükler:

**Development (`.env.development`):**
```env
# Docker Compose ile çalışıyorsa
VITE_API_URL=http://localhost:6060

# Local .NET ile çalışıyorsa
# VITE_API_URL=http://localhost:5285
```

**Production (`.env.production`):**
```env
VITE_API_URL=https://api.yourdomain.com
```

**Not:** 
- `.env.development` ve `.env.production` dosyaları git'e commit edilmelidir (template olarak)
- Gerçek production URL'lerini `.env.production` dosyasında güncelleyin
- `.env` dosyası (varsa) `.gitignore`'da olduğu için commit edilmez

#### Docker Compose

Docker Compose dosyalarında `VITE_API_URL` build argümanı olarak geçilir:

```yaml
lifeos.client:
  build:
    context: ./clients/lifeos-client
    dockerfile: Dockerfile
    args:
      - VITE_API_URL=http://localhost:6060  # Development
      # veya
      - VITE_API_URL=https://api.yourdomain.com  # Production
```

### API Client

API client (`src/lib/api/client.ts`) otomatik olarak:
- Base URL configuration
- JWT token injection
- Token refresh on 401 errors
- Error handling and logging

## 🧪 Geliştirme

### Linting
```bash
npm run lint
```

### Type Checking
```bash
npm run build  # TypeScript type check dahil
```

### Preview Production Build
```bash
npm run preview
```

## 📚 Kullanım Örnekleri

### API Çağrısı (TanStack Query)

```typescript
import { useQuery } from '@tanstack/react-query';
import { api } from '@/lib/api/client';

function Posts() {
  const { data, isLoading, error } = useQuery({
    queryKey: ['posts'],
    queryFn: () => api.get('/posts'),
  });

  if (isLoading) return <div>Loading...</div>;
  if (error) return <div>Error: {error.message}</div>;

  return <div>{/* Render posts */}</div>;
}
```

### Form Validation (React Hook Form + Zod)

```typescript
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';

const schema = z.object({
  email: z.string().email(),
  password: z.string().min(8),
});

function LoginForm() {
  const { register, handleSubmit } = useForm({
    resolver: zodResolver(schema),
  });

  return <form onSubmit={handleSubmit(onSubmit)}>...</form>;
}
```

### State Management (Zustand)

```typescript
import { useAuthStore } from '@/store/authStore';

function Profile() {
  const { user, logout } = useAuthStore();

  return (
    <div>
      <p>Welcome, {user?.name}</p>
      <button onClick={logout}>Logout</button>
    </div>
  );
}
```

## 🔗 İlgili Bağlantılar

- [Ana README](../../README.md) - Genel proje bilgisi
- [API Documentation](http://localhost:5000/scalar/v1) - Scalar API docs
- [TailwindCSS Docs](https://tailwindcss.com/docs)
- [shadcn/ui Components](https://ui.shadcn.com/)
- [TanStack Query](https://tanstack.com/query/latest)

## 📝 Notlar

- API çağrıları için `withCredentials: true` kullanılıyor (cookie-based auth destekli)
- Token yenileme otomatik olarak axios interceptor tarafından yönetiliyor
- Protected route'lar için `ProtectedRoute` component'i kullanılıyor
- Form validation Zod schema'ları ile runtime type-safety sağlıyor

## 🚧 Gelecek Geliştirmeler

- [ ] i18n (Çoklu dil desteği)
- [ ] Dark mode toggle
- [ ] PWA support
- [ ] Offline mode
- [ ] Advanced search & filters
- [ ] Social sharing
- [ ] Comment system
- [ ] User profile management

## 📄 Lisans

Bu proje eğitim amaçlı geliştirilmiştir.
