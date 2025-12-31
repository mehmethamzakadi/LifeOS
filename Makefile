# ============================================
# LifeOS - Makefile
# ============================================
# Kullanım: make <komut>
# Yardım: make help
# ============================================

.PHONY: help dev prod stop down clean rebuild logs ps shell-api shell-db pull-ollama migrate migrate-up migrate-down migrate-list status health

# Renkler (opsiyonel - terminal desteği varsa)
GREEN  := \033[0;32m
YELLOW := \033[0;33m
RED    := \033[0;31m
NC     := \033[0m # No Color

# Docker Compose dosyaları
COMPOSE_DEV  := docker-compose -f docker-compose.yml -f docker-compose.local.yml
COMPOSE_PROD := docker-compose -f docker-compose.yml -f docker-compose.prod.yml

# Container isimleri
API_CONTAINER_DEV  := lifeos_api_dev
API_CONTAINER_PROD := lifeos_api_prod
DB_CONTAINER_DEV   := lifeos_postgres_dev
DB_CONTAINER_PROD  := lifeos_postgres_prod

# ============================================
# Yardım Menüsü
# ============================================
help:
	@echo "$(GREEN)LifeOS - Makefile Komutları$(NC)"
	@echo ""
	@echo "$(YELLOW)Geliştirme Ortamı:$(NC)"
	@echo "  make dev          - Development ortamını başlat (build ile)"
	@echo "  make dev-up        - Development ortamını başlat (build olmadan)"
	@echo "  make dev-build    - Development servislerini rebuild et"
	@echo "  make dev-logs     - Development loglarını izle"
	@echo ""
	@echo "$(YELLOW)Production Ortamı:$(NC)"
	@echo "  make prod         - Production ortamını başlat (build ile)"
	@echo "  make prod-up      - Production ortamını başlat (build olmadan)"
	@echo "  make prod-build   - Production servislerini rebuild et"
	@echo "  make prod-logs    - Production loglarını izle"
	@echo ""
	@echo "$(YELLOW)Servis Yönetimi:$(NC)"
	@echo "  make stop         - Tüm servisleri durdur (volume'lar korunur)"
	@echo "  make down         - Tüm servisleri durdur ve volume'ları sil $(RED)(DİKKAT!)$(NC)"
	@echo "  make restart      - Tüm servisleri yeniden başlat"
	@echo "  make ps           - Çalışan servisleri listele"
	@echo "  make status       - Servis durumlarını göster"
	@echo "  make health       - Health check sonuçlarını göster"
	@echo ""
	@echo "$(YELLOW)Migration İşlemleri:$(NC)"
	@echo "  make migrate NAME=<migration-name>  - Yeni migration oluştur"
	@echo "  make migrate-up                     - Migration'ları uygula"
	@echo "  make migrate-down                   - Son migration'ı geri al"
	@echo "  make migrate-list                   - Migration listesini göster"
	@echo ""
	@echo "$(YELLOW)Log ve Debug:$(NC)"
	@echo "  make logs         - Tüm servislerin loglarını izle"
	@echo "  make logs-api     - API loglarını izle"
	@echo "  make logs-client  - Client loglarını izle"
	@echo "  make logs-db      - Database loglarını izle"
	@echo ""
	@echo "$(YELLOW)Container İşlemleri:$(NC)"
	@echo "  make shell-api    - API container'ına shell aç (dev)"
	@echo "  make shell-db     - Database container'ına shell aç (dev)"
	@echo "  make shell-client - Client container'ına shell aç (dev)"
	@echo ""
	@echo "$(YELLOW)Ollama AI:$(NC)"
	@echo "  make pull-ollama MODEL=<model-name>  - Ollama modelini yükle (varsayılan: qwen2.5:1.5b)"
	@echo "  make list-ollama                     - Yüklü Ollama modellerini listele"
	@echo ""
	@echo "$(YELLOW)Temizleme:$(NC)"
	@echo "  make clean        - Build cache'leri ve unused image'ları temizle"
	@echo "  make clean-all    - Tüm Docker kaynaklarını temizle $(RED)(DİKKAT!)$(NC)"
	@echo ""
	@echo "$(YELLOW)Özel İşlemler:$(NC)"
	@echo "  make seed         - Database seed işlemini çalıştır (dev)"
	@echo "  make test         - Testleri çalıştır"
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

dev-up:
	@echo "$(YELLOW)Development ortamı başlatılıyor...$(NC)"
	$(COMPOSE_DEV) up -d
	@echo "$(GREEN)✓ Servisler başlatıldı$(NC)"

dev-build:
	@echo "$(YELLOW)Development servisleri build ediliyor...$(NC)"
	$(COMPOSE_DEV) build --no-cache
	@echo "$(GREEN)✓ Build tamamlandı$(NC)"

dev-logs:
	$(COMPOSE_DEV) logs -f

dev-restart:
	@echo "$(YELLOW)Development servisleri yeniden başlatılıyor...$(NC)"
	$(COMPOSE_DEV) restart
	@echo "$(GREEN)✓ Servisler yeniden başlatıldı$(NC)"

# ============================================
# Production Ortamı
# ============================================
prod: prod-build prod-up
	@echo "$(GREEN)✓ Production ortamı başlatıldı!$(NC)"
	@echo "$(YELLOW)ÖNEMLİ: Production ortamı için .env dosyasını kontrol edin!$(NC)"

prod-up:
	@echo "$(YELLOW)Production ortamı başlatılıyor...$(NC)"
	@if [ ! -f .env ]; then \
		echo "$(RED)✗ HATA: .env dosyası bulunamadı!$(NC)"; \
		echo "$(YELLOW)Production için .env dosyası oluşturun: cp .env.example .env$(NC)"; \
		exit 1; \
	fi
	$(COMPOSE_PROD) up -d
	@echo "$(GREEN)✓ Production servisleri başlatıldı$(NC)"

prod-build:
	@echo "$(YELLOW)Production servisleri build ediliyor...$(NC)"
	$(COMPOSE_PROD) build --no-cache
	@echo "$(GREEN)✓ Build tamamlandı$(NC)"

prod-logs:
	$(COMPOSE_PROD) logs -f

prod-restart:
	@echo "$(YELLOW)Production servisleri yeniden başlatılıyor...$(NC)"
	$(COMPOSE_PROD) restart
	@echo "$(GREEN)✓ Servisler yeniden başlatıldı$(NC)"

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
	@echo "$(RED)DİKKAT: Tüm servisler ve volume'lar silinecek!$(NC)"
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
	@echo "$(YELLOW)Not: Migration dosyaları container içinde oluşturulur. Host'a kopyalamak için volume mount gerekir.$(NC)"
	@docker exec -it $(API_CONTAINER_DEV) dotnet ef migrations add $(NAME) \
		--project /app/LifeOS.Persistence.csproj \
		--output-dir Migrations/PostgreSql \
		--context LifeOSDbContext || \
		(echo "$(RED)Migration oluşturulamadı. Container çalışıyor mu?$(NC)" && exit 1)
	@echo "$(GREEN)✓ Migration oluşturuldu: $(NAME)$(NC)"
	@echo "$(YELLOW)Not: Migration dosyalarını görmek için: docker exec -it $(API_CONTAINER_DEV) ls -la /app/Migrations/PostgreSql$(NC)"

migrate-up:
	@echo "$(YELLOW)Migration'lar uygulanıyor...$(NC)"
	@docker exec -it $(API_CONTAINER_DEV) dotnet ef database update \
		--project /app/LifeOS.Persistence.csproj \
		--context LifeOSDbContext || \
		(echo "$(RED)Migration'lar uygulanamadı. Container çalışıyor mu?$(NC)" && exit 1)
	@echo "$(GREEN)✓ Migration'lar uygulandı$(NC)"

migrate-down:
	@echo "$(RED)DİKKAT: Son migration geri alınacak!$(NC)"
	@read -p "Devam etmek istediğinize emin misiniz? (y/N): " confirm && [ "$$confirm" = "y" ] || exit 1
	@echo "$(YELLOW)Son migration geri alınıyor...$(NC)"
	@docker exec -it $(API_CONTAINER_DEV) dotnet ef migrations remove \
		--project /app/LifeOS.Persistence.csproj \
		--context LifeOSDbContext || \
		(echo "$(RED)Migration geri alınamadı. Container çalışıyor mu?$(NC)" && exit 1)
	@echo "$(GREEN)✓ Migration geri alındı$(NC)"

migrate-list:
	@echo "$(YELLOW)Migration Listesi:$(NC)"
	@docker exec -it $(API_CONTAINER_DEV) dotnet ef migrations list \
		--project /app/LifeOS.Persistence.csproj \
		--context LifeOSDbContext || \
		(echo "$(RED)Migration listesi alınamadı. Container çalışıyor mu?$(NC)" && exit 1)

# ============================================
# Log İşlemleri
# ============================================
logs:
	$(COMPOSE_DEV) logs -f

logs-api:
	$(COMPOSE_DEV) logs -f lifeos.api

logs-client:
	$(COMPOSE_DEV) logs -f lifeos.client

logs-db:
	$(COMPOSE_DEV) logs -f postgresdb

logs-all:
	@echo "$(YELLOW)Tüm servis logları:$(NC)"
	@docker logs -f $(API_CONTAINER_DEV) 2>/dev/null || echo "API container çalışmıyor"

# ============================================
# Container Shell İşlemleri
# ============================================
shell-api:
	@echo "$(YELLOW)API container'ına bağlanılıyor...$(NC)"
	@docker exec -it $(API_CONTAINER_DEV) /bin/sh || \
		docker exec -it $(API_CONTAINER_DEV) /bin/bash || \
		echo "$(RED)Container çalışmıyor veya shell bulunamadı$(NC)"

shell-db:
	@echo "$(YELLOW)Database container'ına bağlanılıyor...$(NC)"
	@docker exec -it $(DB_CONTAINER_DEV) psql -U postgres -d LifeOSDb || \
		echo "$(RED)Database container çalışmıyor$(NC)"

shell-client:
	@echo "$(YELLOW)Client container'ına bağlanılıyor...$(NC)"
	@docker exec -it lifeos_client_dev /bin/sh || \
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
	@echo "$(RED)DİKKAT: Tüm Docker kaynakları silinecek!$(NC)"
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
	@docker exec -it $(API_CONTAINER_DEV) dotnet run --project /app/LifeOS.API.csproj || \
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

