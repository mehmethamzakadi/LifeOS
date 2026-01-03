# ============================================
# LifeOS - Makefile
# ============================================
# Kullanım: make <komut>
# Yardım: make help
# ============================================

.PHONY: help dev prod stop down clean rebuild logs ps shell-api shell-db pull-ollama migrate migrate-up migrate-down migrate-list status health prod-deploy prod-update

# Renkler
GREEN  := \033[0;32m
YELLOW := \033[0;33m
RED    := \033[0;31m
BLUE   := \033[0;34m
NC     := \033[0m # No Color

# Docker Compose dosyaları
COMPOSE_DEV  := docker-compose -f docker-compose.yml -f docker-compose.local.yml
COMPOSE_PROD := docker-compose -f docker-compose.yml -f docker-compose.prod.yml

# Container isimleri
API_CONTAINER_DEV  := lifeos_api_dev
API_CONTAINER_PROD := lifeos_api_prod
CLIENT_CONTAINER_DEV := lifeos_client_dev
CLIENT_CONTAINER_PROD := lifeos_client_prod
DB_CONTAINER_DEV   := lifeos_postgres_dev
DB_CONTAINER_PROD  := lifeos_postgres_prod

# ============================================
# Yardım Menüsü
# ============================================
help:
	@echo "$(GREEN)╔════════════════════════════════════════════════════════════╗$(NC)"
	@echo "$(GREEN)║          LifeOS - Makefile Komutları                      ║$(NC)"
	@echo "$(GREEN)╚════════════════════════════════════════════════════════════╝$(NC)"
	@echo ""
	@echo "$(YELLOW)━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━$(NC)"
	@echo "$(YELLOW)  Geliştirme Ortamı (Development)$(NC)"
	@echo "$(YELLOW)━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━$(NC)"
	@echo "  $(GREEN)make dev$(NC)          - Development ortamını başlat (build ile)"
	@echo "  $(GREEN)make dev-up$(NC)        - Development ortamını başlat (build olmadan)"
	@echo "  $(GREEN)make dev-build$(NC)     - Development servislerini rebuild et"
	@echo "  $(GREEN)make dev-rebuild$(NC)   - API container'ı rebuild et (hot reload için)"
	@echo "  $(GREEN)make dev-logs$(NC)     - Development loglarını izle"
	@echo "  $(GREEN)make dev-stop$(NC)     - Development servislerini durdur"
	@echo "  $(BLUE)🔥 Hot Reload:$(NC) Kod değişiklikleri otomatik algılanır!"
	@echo ""
	@echo "$(YELLOW)━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━$(NC)"
	@echo "$(YELLOW)  Production Ortamı$(NC)"
	@echo "$(YELLOW)━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━$(NC)"
	@echo "  $(GREEN)make prod$(NC)         - Production ortamını başlat (build ile)"
	@echo "  $(GREEN)make prod-up$(NC)     - Production ortamını başlat (build olmadan)"
	@echo "  $(GREEN)make prod-build$(NC)   - Production servislerini rebuild et"
	@echo "  $(GREEN)make prod-logs$(NC)    - Production loglarını izle"
	@echo "  $(GREEN)make prod-stop$(NC)    - Production servislerini durdur"
	@echo "  $(GREEN)make prod-restart$(NC) - Production servislerini yeniden başlat"
	@echo ""
	@echo "  $(RED)╔════════════════════════════════════════════════════════════╗$(NC)"
	@echo "  $(RED)║  PRODUCTION DEPLOYMENT (Volume'lar korunur)              ║$(NC)"
	@echo "  $(RED)╚════════════════════════════════════════════════════════════╝$(NC)"
	@echo "  $(GREEN)make prod-deploy$(NC)   - Git pull + Rebuild + Restart (Volume'lar korunur)"
	@echo "  $(GREEN)make prod-update$(NC)   - Sadece rebuild + restart (Volume'lar korunur)"
	@echo "  $(YELLOW)Not:$(NC) prod-deploy komutu git pull yapar, rebuild eder ve restart eder"
	@echo "  $(YELLOW)Not:$(NC) Volume'lar (veritabanı, redis, uploads) asla silinmez!"
	@echo ""
	@echo "$(YELLOW)━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━$(NC)"
	@echo "$(YELLOW)  Servis Yönetimi$(NC)"
	@echo "$(YELLOW)━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━$(NC)"
	@echo "  $(GREEN)make stop$(NC)         - Tüm servisleri durdur (volume'lar korunur)"
	@echo "  $(GREEN)make down$(NC)         - Tüm servisleri durdur ve volume'ları sil $(RED)(DİKKAT!)$(NC)"
	@echo "  $(GREEN)make ps$(NC)           - Çalışan servisleri listele"
	@echo "  $(GREEN)make status$(NC)       - Servis durumlarını göster"
	@echo "  $(GREEN)make health$(NC)       - Health check sonuçlarını göster"
	@echo ""
	@echo "$(YELLOW)━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━$(NC)"
	@echo "$(YELLOW)  Migration İşlemleri$(NC)"
	@echo "$(YELLOW)━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━$(NC)"
	@echo "  $(GREEN)make migrate NAME=<name>$(NC)  - Yeni migration oluştur"
	@echo "  $(GREEN)make migrate-up$(NC)           - Migration'ları uygula"
	@echo "  $(GREEN)make migrate-down$(NC)         - Son migration'ı geri al"
	@echo "  $(GREEN)make migrate-list$(NC)         - Migration listesini göster"
	@echo ""
	@echo "$(YELLOW)━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━$(NC)"
	@echo "$(YELLOW)  Log ve Debug$(NC)"
	@echo "$(YELLOW)━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━$(NC)"
	@echo "  $(GREEN)make logs$(NC)         - Tüm servislerin loglarını izle"
	@echo "  $(GREEN)make logs-api$(NC)     - API loglarını izle"
	@echo "  $(GREEN)make logs-client$(NC)  - Client loglarını izle"
	@echo "  $(GREEN)make logs-db$(NC)      - Database loglarını izle"
	@echo ""
	@echo "$(YELLOW)━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━$(NC)"
	@echo "$(YELLOW)  Container İşlemleri$(NC)"
	@echo "$(YELLOW)━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━$(NC)"
	@echo "  $(GREEN)make shell-api$(NC)    - API container'ına shell aç (dev)"
	@echo "  $(GREEN)make shell-db$(NC)     - Database container'ına shell aç (dev)"
	@echo "  $(GREEN)make shell-client$(NC) - Client container'ına shell aç (dev)"
	@echo ""
	@echo "$(YELLOW)━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━$(NC)"
	@echo "$(YELLOW)  Ollama AI$(NC)"
	@echo "$(YELLOW)━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━$(NC)"
	@echo "  $(GREEN)make pull-ollama MODEL=<model>$(NC)  - Ollama modelini yükle"
	@echo "  $(GREEN)make list-ollama$(NC)                - Yüklü Ollama modellerini listele"
	@echo ""
	@echo "$(YELLOW)━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━$(NC)"
	@echo "$(YELLOW)  Temizleme$(NC)"
	@echo "$(YELLOW)━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━$(NC)"
	@echo "  $(GREEN)make clean$(NC)       - Build cache'leri ve unused image'ları temizle"
	@echo "  $(GREEN)make clean-all$(NC)   - Tüm Docker kaynaklarını temizle $(RED)(DİKKAT!)$(NC)"
	@echo ""

# ============================================
# Development Ortamı
# ============================================
dev: dev-build dev-up
	@echo "$(GREEN)✓ Development ortamı başlatıldı!$(NC)"
	@echo "$(YELLOW)Frontend:$(NC) http://localhost:5173"
	@echo "$(YELLOW)Backend API:$(NC) http://localhost:6060"
	@echo "$(YELLOW)API Docs:$(NC) http://localhost:6060/scalar/v1"
	@echo "$(YELLOW)Seq Logs:$(NC) http://localhost:5341"
	@echo "$(BLUE)🔥 Hot Reload aktif - kod değişiklikleri otomatik algılanacak!$(NC)"

dev-up:
	@echo "$(YELLOW)Development ortamı başlatılıyor...$(NC)"
	$(COMPOSE_DEV) up -d
	@echo "$(GREEN)✓ Servisler başlatıldı$(NC)"

dev-build:
	@echo "$(YELLOW)Development servisleri build ediliyor...$(NC)"
	$(COMPOSE_DEV) build
	@echo "$(GREEN)✓ Build tamamlandı$(NC)"
	@echo "$(BLUE)Not:$(NC) Hot reload aktif - kod değişiklikleri otomatik algılanacak"

dev-rebuild:
	@echo "$(YELLOW)API container'ı rebuild ediliyor (hot reload için)...$(NC)"
	$(COMPOSE_DEV) build --no-cache lifeos.api
	$(COMPOSE_DEV) up -d lifeos.api
	@echo "$(GREEN)✓ API container yeniden başlatıldı$(NC)"

dev-logs:
	@echo "$(YELLOW)Development logları izleniyor...$(NC)"
	$(COMPOSE_DEV) logs -f

dev-stop:
	@echo "$(YELLOW)Development servisleri durduruluyor...$(NC)"
	$(COMPOSE_DEV) down
	@echo "$(GREEN)✓ Development servisleri durduruldu (volume'lar korundu)$(NC)"

dev-restart:
	@echo "$(YELLOW)Development servisleri yeniden başlatılıyor...$(NC)"
	$(COMPOSE_DEV) restart
	@echo "$(GREEN)✓ Servisler yeniden başlatıldı$(NC)"

# ============================================
# Production Ortamı
# ============================================
prod: prod-build prod-up
	@echo "$(GREEN)✓ Production ortamı başlatıldı!$(NC)"
	@echo "$(RED)ÖNEMLİ: Production ortamı için .env dosyasını kontrol edin!$(NC)"

prod-up:
	@echo "$(YELLOW)Production ortamı başlatılıyor...$(NC)"
	@if [ ! -f .env ]; then \
		echo "$(RED)✗ HATA: .env dosyası bulunamadı!$(NC)"; \
		echo "$(YELLOW)Production için .env dosyası oluşturun: cp .env.example .env$(NC)"; \
		exit 1; \
	fi
	$(COMPOSE_PROD) up -d
	@echo "$(GREEN)✓ Production servisleri başlatıldı$(NC)"
	@echo "$(YELLOW)Volume'lar korundu:$(NC) postgres_prod_data, redis_prod_data, seq_prod_data, uploads_prod_data"

prod-build:
	@echo "$(YELLOW)Production servisleri build ediliyor...$(NC)"
	@if [ ! -f .env ]; then \
		echo "$(RED)✗ HATA: .env dosyası bulunamadı!$(NC)"; \
		echo "$(YELLOW)Production için .env dosyası oluşturun: cp .env.example .env$(NC)"; \
		exit 1; \
	fi
	$(COMPOSE_PROD) build --no-cache
	@echo "$(GREEN)✓ Build tamamlandı$(NC)"

prod-logs:
	@echo "$(YELLOW)Production logları izleniyor...$(NC)"
	$(COMPOSE_PROD) logs -f

prod-stop:
	@echo "$(YELLOW)Production servisleri durduruluyor...$(NC)"
	$(COMPOSE_PROD) down
	@echo "$(GREEN)✓ Production servisleri durduruldu (volume'lar korundu)$(NC)"

prod-restart:
	@echo "$(YELLOW)Production servisleri yeniden başlatılıyor...$(NC)"
	$(COMPOSE_PROD) restart
	@echo "$(GREEN)✓ Servisler yeniden başlatıldı$(NC)"

# ============================================
# Production Deployment (Volume'lar korunur)
# ============================================
prod-deploy:
	@echo "$(RED)╔════════════════════════════════════════════════════════════╗$(NC)"
	@echo "$(RED)║     PRODUCTION DEPLOYMENT (Volume'lar korunur)            ║$(NC)"
	@echo "$(RED)╚════════════════════════════════════════════════════════════╝$(NC)"
	@echo ""
	@if [ ! -f .env ]; then \
		echo "$(RED)✗ HATA: .env dosyası bulunamadı!$(NC)"; \
		exit 1; \
	fi
	@echo "$(YELLOW)1. Git pull yapılıyor...$(NC)"
	@git pull origin || (echo "$(RED)✗ Git pull başarısız!$(NC)" && exit 1)
	@echo "$(GREEN)✓ Git pull tamamlandı$(NC)"
	@echo ""
	@echo "$(YELLOW)2. Production servisleri rebuild ediliyor...$(NC)"
	@$(COMPOSE_PROD) build --no-cache lifeos.api lifeos.client
	@echo "$(GREEN)✓ Build tamamlandı$(NC)"
	@echo ""
	@echo "$(YELLOW)3. Container'lar yeniden başlatılıyor (volume'lar korunuyor)...$(NC)"
	@$(COMPOSE_PROD) up -d --no-deps lifeos.api lifeos.client
	@echo "$(GREEN)✓ Container'lar yeniden başlatıldı$(NC)"
	@echo ""
	@echo "$(YELLOW)4. Migration kontrolü yapılıyor...$(NC)"
	@echo "$(BLUE)Migration uygulamak için: make migrate-up-prod$(NC)"
	@echo ""
	@echo "$(GREEN)╔════════════════════════════════════════════════════════════╗$(NC)"
	@echo "$(GREEN)║  ✓ Production deployment tamamlandı!                      ║$(NC)"
	@echo "$(GREEN)║  ✓ Volume'lar korundu (veritabanı, redis, uploads)        ║$(NC)"
	@echo "$(GREEN)╚════════════════════════════════════════════════════════════╝$(NC)"

prod-update:
	@echo "$(RED)╔════════════════════════════════════════════════════════════╗$(NC)"
	@echo "$(RED)║     PRODUCTION UPDATE (Volume'lar korunur)                 ║$(NC)"
	@echo "$(RED)╚════════════════════════════════════════════════════════════╝$(NC)"
	@echo ""
	@if [ ! -f .env ]; then \
		echo "$(RED)✗ HATA: .env dosyası bulunamadı!$(NC)"; \
		exit 1; \
	fi
	@echo "$(YELLOW)1. Production servisleri rebuild ediliyor...$(NC)"
	@$(COMPOSE_PROD) build --no-cache lifeos.api lifeos.client
	@echo "$(GREEN)✓ Build tamamlandı$(NC)"
	@echo ""
	@echo "$(YELLOW)2. Container'lar yeniden başlatılıyor (volume'lar korunuyor)...$(NC)"
	@$(COMPOSE_PROD) up -d --no-deps lifeos.api lifeos.client
	@echo "$(GREEN)✓ Container'lar yeniden başlatıldı$(NC)"
	@echo ""
	@echo "$(GREEN)╔════════════════════════════════════════════════════════════╗$(NC)"
	@echo "$(GREEN)║  ✓ Production update tamamlandı!                          ║$(NC)"
	@echo "$(GREEN)║  ✓ Volume'lar korundu (veritabanı, redis, uploads)        ║$(NC)"
	@echo "$(GREEN)╚════════════════════════════════════════════════════════════╝$(NC)"

# ============================================
# Servis Yönetimi
# ============================================
stop:
	@echo "$(YELLOW)Servisler durduruluyor (volume'lar korunacak)...$(NC)"
	@if docker ps | grep -q lifeos; then \
		$(COMPOSE_DEV) down 2>/dev/null || true; \
		$(COMPOSE_PROD) down 2>/dev/null || true; \
		echo "$(GREEN)✓ Servisler durduruldu (volume'lar korundu)$(NC)"; \
	else \
		echo "$(YELLOW)Çalışan servis bulunamadı$(NC)"; \
	fi

down:
	@echo "$(RED)╔════════════════════════════════════════════════════════════╗$(NC)"
	@echo "$(RED)║  DİKKAT: Tüm servisler ve volume'lar silinecek!          ║$(NC)"
	@echo "$(RED)╚════════════════════════════════════════════════════════════╝$(NC)"
	@read -p "Devam etmek istediğinize emin misiniz? (y/N): " confirm && [ "$$confirm" = "y" ] || exit 1
	@echo "$(YELLOW)Servisler ve volume'lar siliniyor...$(NC)"
	$(COMPOSE_DEV) down -v 2>/dev/null || true
	$(COMPOSE_PROD) down -v 2>/dev/null || true
	@echo "$(GREEN)✓ Tüm servisler ve volume'lar silindi$(NC)"

restart: stop dev-up
	@echo "$(GREEN)✓ Servisler yeniden başlatıldı$(NC)"

ps:
	@echo "$(YELLOW)Çalışan servisler:$(NC)"
	@docker ps --filter "name=lifeos" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

status:
	@echo "$(YELLOW)Development Servis Durumları:$(NC)"
	@$(COMPOSE_DEV) ps 2>/dev/null || echo "Development servisleri çalışmıyor"
	@echo ""
	@echo "$(YELLOW)Production Servis Durumları:$(NC)"
	@$(COMPOSE_PROD) ps 2>/dev/null || echo "Production servisleri çalışmıyor"

health:
	@echo "$(YELLOW)Health Check Sonuçları:$(NC)"
	@docker ps --filter "name=lifeos" --format "{{.Names}}" | while read container; do \
		health=$$(docker inspect --format='{{.State.Health.Status}}' $$container 2>/dev/null || echo "no-healthcheck"); \
		status=$$(docker inspect --format='{{.State.Status}}' $$container); \
		printf "%-30s Status: %-10s Health: %s\n" $$container $$status $$health; \
	done

# ============================================
# Migration İşlemleri
# ============================================
migrate:
	@if [ -z "$(NAME)" ]; then \
		echo "$(RED)✗ HATA: Migration adı belirtilmedi!$(NC)"; \
		echo "$(YELLOW)Kullanım: make migrate NAME=MigrationName$(NC)"; \
		exit 1; \
	fi
	@echo "$(YELLOW)Migration oluşturuluyor: $(NAME)$(NC)"
	@docker exec -it $(API_CONTAINER_DEV) dotnet ef migrations add $(NAME) \
		--project /src/src/LifeOS.Persistence/LifeOS.Persistence.csproj \
		--startup-project /src/src/LifeOS.API/LifeOS.API.csproj \
		--output-dir Migrations/PostgreSql \
		--context LifeOSDbContext || \
		(echo "$(RED)Migration oluşturulamadı. Container çalışıyor mu?$(NC)" && exit 1)
	@echo "$(GREEN)✓ Migration oluşturuldu: $(NAME)$(NC)"

migrate-up:
	@echo "$(YELLOW)Migration'lar uygulanıyor (dev)...$(NC)"
	@docker exec -it $(API_CONTAINER_DEV) dotnet ef database update \
		--project /src/src/LifeOS.Persistence/LifeOS.Persistence.csproj \
		--startup-project /src/src/LifeOS.API/LifeOS.API.csproj \
		--context LifeOSDbContext || \
		(echo "$(RED)Migration'lar uygulanamadı. Container çalışıyor mu?$(NC)" && exit 1)
	@echo "$(GREEN)✓ Migration'lar uygulandı$(NC)"

migrate-up-prod:
	@echo "$(YELLOW)Migration'lar uygulanıyor (prod)...$(NC)"
	@docker exec -it $(API_CONTAINER_PROD) dotnet ef database update \
		--project /app/LifeOS.Persistence.csproj \
		--startup-project /app/LifeOS.API.csproj \
		--context LifeOSDbContext || \
		(echo "$(RED)Migration'lar uygulanamadı. Container çalışıyor mu?$(NC)" && exit 1)
	@echo "$(GREEN)✓ Migration'lar uygulandı$(NC)"

migrate-down:
	@echo "$(RED)DİKKAT: Son migration geri alınacak!$(NC)"
	@read -p "Devam etmek istediğinize emin misiniz? (y/N): " confirm && [ "$$confirm" = "y" ] || exit 1
	@echo "$(YELLOW)Son migration geri alınıyor...$(NC)"
	@docker exec -it $(API_CONTAINER_DEV) dotnet ef migrations remove \
		--project /src/src/LifeOS.Persistence/LifeOS.Persistence.csproj \
		--startup-project /src/src/LifeOS.API/LifeOS.API.csproj \
		--context LifeOSDbContext || \
		(echo "$(RED)Migration geri alınamadı. Container çalışıyor mu?$(NC)" && exit 1)
	@echo "$(GREEN)✓ Migration geri alındı$(NC)"

migrate-list:
	@echo "$(YELLOW)Migration Listesi:$(NC)"
	@docker exec -it $(API_CONTAINER_DEV) dotnet ef migrations list \
		--project /src/src/LifeOS.Persistence/LifeOS.Persistence.csproj \
		--startup-project /src/src/LifeOS.API/LifeOS.API.csproj \
		--context LifeOSDbContext || \
		(echo "$(RED)Migration listesi alınamadı. Container çalışıyor mu?$(NC)" && exit 1)

# ============================================
# Log İşlemleri
# ============================================
logs:
	@echo "$(YELLOW)Tüm servis logları izleniyor...$(NC)"
	$(COMPOSE_DEV) logs -f

logs-api:
	@echo "$(YELLOW)API logları izleniyor...$(NC)"
	$(COMPOSE_DEV) logs -f lifeos.api

logs-client:
	@echo "$(YELLOW)Client logları izleniyor...$(NC)"
	$(COMPOSE_DEV) logs -f lifeos.client

logs-db:
	@echo "$(YELLOW)Database logları izleniyor...$(NC)"
	$(COMPOSE_DEV) logs -f postgresdb

logs-prod:
	@echo "$(YELLOW)Production logları izleniyor...$(NC)"
	$(COMPOSE_PROD) logs -f

logs-prod-api:
	@echo "$(YELLOW)Production API logları izleniyor...$(NC)"
	$(COMPOSE_PROD) logs -f lifeos.api

# ============================================
# Container Shell İşlemleri
# ============================================
shell-api:
	@echo "$(YELLOW)API container'ına bağlanılıyor (dev)...$(NC)"
	@docker exec -it $(API_CONTAINER_DEV) /bin/sh || \
		docker exec -it $(API_CONTAINER_DEV) /bin/bash || \
		echo "$(RED)Container çalışmıyor veya shell bulunamadı$(NC)"

shell-api-prod:
	@echo "$(YELLOW)API container'ına bağlanılıyor (prod)...$(NC)"
	@docker exec -it $(API_CONTAINER_PROD) /bin/sh || \
		docker exec -it $(API_CONTAINER_PROD) /bin/bash || \
		echo "$(RED)Container çalışmıyor veya shell bulunamadı$(NC)"

shell-db:
	@echo "$(YELLOW)Database container'ına bağlanılıyor (dev)...$(NC)"
	@docker exec -it $(DB_CONTAINER_DEV) psql -U postgres -d LifeOSDb || \
		echo "$(RED)Database container çalışmıyor$(NC)"

shell-db-prod:
	@echo "$(YELLOW)Database container'ına bağlanılıyor (prod)...$(NC)"
	@docker exec -it $(DB_CONTAINER_PROD) psql -U postgres -d $$(grep POSTGRES_DB .env | cut -d '=' -f2) || \
		echo "$(RED)Database container çalışmıyor$(NC)"

shell-client:
	@echo "$(YELLOW)Client container'ına bağlanılıyor (dev)...$(NC)"
	@docker exec -it $(CLIENT_CONTAINER_DEV) /bin/sh || \
		echo "$(RED)Client container çalışmıyor$(NC)"

shell-client-prod:
	@echo "$(YELLOW)Client container'ına bağlanılıyor (prod)...$(NC)"
	@docker exec -it $(CLIENT_CONTAINER_PROD) /bin/sh || \
		echo "$(RED)Client container çalışmıyor$(NC)"

# ============================================
# Ollama AI İşlemleri
# ============================================
pull-ollama:
	@MODEL=$${MODEL:-qwen2.5:1.5b}; \
	echo "$(YELLOW)Ollama modeli yükleniyor: $$MODEL$(NC)"; \
	docker exec -it lifeos_ollama_dev ollama pull $$MODEL || \
		echo "$(RED)Model yüklenemedi. Ollama container çalışıyor mu?$(NC)"

list-ollama:
	@echo "$(YELLOW)Yüklü Ollama Modelleri:$(NC)"
	@docker exec -it lifeos_ollama_dev ollama list || \
		echo "$(RED)Ollama container çalışmıyor$(NC)"

# ============================================
# Temizleme İşlemleri
# ============================================
clean:
	@echo "$(YELLOW)Build cache'leri temizleniyor...$(NC)"
	@docker system prune -f
	@docker builder prune -f
	@echo "$(GREEN)✓ Temizleme tamamlandı$(NC)"

clean-all:
	@echo "$(RED)╔════════════════════════════════════════════════════════════╗$(NC)"
	@echo "$(RED)║  DİKKAT: Tüm Docker kaynakları silinecek!                 ║$(NC)"
	@echo "$(RED)╚════════════════════════════════════════════════════════════╝$(NC)"
	@read -p "Devam etmek istediğinize emin misiniz? (y/N): " confirm && [ "$$confirm" = "y" ] || exit 1
	@echo "$(YELLOW)Tüm Docker kaynakları temizleniyor...$(NC)"
	@docker system prune -a --volumes -f
	@echo "$(GREEN)✓ Tüm Docker kaynakları temizlendi$(NC)"

# ============================================
# Özel İşlemler
# ============================================
seed:
	@echo "$(YELLOW)Database seed işlemi çalıştırılıyor...$(NC)"
	@echo "$(YELLOW)Not: Seed işlemi API başlatıldığında otomatik çalışır$(NC)"
	@docker exec -it $(API_CONTAINER_DEV) dotnet run --project /src/src/LifeOS.API/LifeOS.API.csproj || \
		echo "$(RED)Seed işlemi çalıştırılamadı$(NC)"

test:
	@echo "$(YELLOW)Testler çalıştırılıyor...$(NC)"
	@docker exec -it $(API_CONTAINER_DEV) dotnet test || \
		echo "$(RED)Testler çalıştırılamadı. Container çalışıyor mu?$(NC)"

rebuild: clean dev-build dev-up
	@echo "$(GREEN)✓ Servisler rebuild edildi ve başlatıldı$(NC)"

# ============================================
# Hızlı Komutlar
# ============================================
quick-start: dev
	@echo "$(GREEN)✓ Hızlı başlatma tamamlandı!$(NC)"

quick-stop: stop
	@echo "$(GREEN)✓ Servisler durduruldu$(NC)"

# Varsayılan komut
.DEFAULT_GOAL := help
