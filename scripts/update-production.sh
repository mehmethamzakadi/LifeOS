#!/bin/bash

# ============================================
# LifeOS - Production Update Script
# ============================================
# Bu script GitHub'dan güncellemeleri çeker
# ve production servislerini günceller
# ============================================

set -e

# Renkler
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Parametreler
BRANCH="${1:-main}"
FORCE_BUILD="${2:-false}"
SERVICE="${3:-all}"

echo "============================================"
echo "  LifeOS - Production Update Script"
echo "============================================"
echo

# Proje dizinine git
if [ ! -d "/opt/lifeos" ]; then
    print_error "Proje dizini bulunamadı: /opt/lifeos"
    print_info "Lütfen proje dizinini kontrol edin"
    exit 1
fi

cd /opt/lifeos

# Git durumunu kontrol et
if [ ! -d ".git" ]; then
    print_error "Bu dizin bir Git repository değil!"
    exit 1
fi

# Mevcut branch'i göster
CURRENT_BRANCH=$(git branch --show-current)
print_info "Mevcut branch: $CURRENT_BRANCH"

# Git durumunu kontrol et
print_info "Git durumu kontrol ediliyor..."
if [ -n "$(git status --porcelain)" ]; then
    print_warning "Local değişiklikler tespit edildi:"
    git status --short
    
    read -p "Local değişiklikleri saklamak istiyor musunuz? (y/n) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        print_info "Değişiklikler stash'e alınıyor..."
        git stash push -m "Auto-stash before update $(date +%Y%m%d_%H%M%S)"
        STASHED=true
    else
        read -p "Local değişiklikleri silmek istiyor musunuz? (y/n) " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            print_warning "Local değişiklikler siliniyor..."
            git reset --hard HEAD
            git clean -fd
        else
            print_error "Güncelleme iptal edildi"
            exit 1
        fi
    fi
fi

# GitHub'dan güncellemeleri çek
print_info "GitHub'dan güncellemeler çekiliyor (branch: $BRANCH)..."
if git fetch origin $BRANCH; then
    LOCAL=$(git rev-parse HEAD)
    REMOTE=$(git rev-parse origin/$BRANCH)
    
    if [ "$LOCAL" = "$REMOTE" ]; then
        print_warning "Zaten en güncel versiyondasınız!"
        read -p "Yine de rebuild yapmak istiyor musunuz? (y/n) " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            if [ "$STASHED" = true ]; then
                git stash pop
            fi
            exit 0
        fi
    else
        print_info "Yeni güncellemeler bulundu"
        git log HEAD..origin/$BRANCH --oneline
        
        # Pull yap
        if git pull origin $BRANCH; then
            print_success "Güncellemeler çekildi"
        else
            print_error "Git pull başarısız oldu!"
            if [ "$STASHED" = true ]; then
                git stash pop
            fi
            exit 1
        fi
    fi
else
    print_error "Git fetch başarısız oldu!"
    if [ "$STASHED" = true ]; then
        git stash pop
    fi
    exit 1
fi

# Stash'i geri al (eğer varsa)
if [ "$STASHED" = true ]; then
    print_info "Stash geri alınıyor..."
    if git stash pop; then
        print_success "Stash geri alındı"
    else
        print_warning "Stash geri alınırken conflict oluştu, manuel çözüm gerekebilir"
    fi
fi

# .env dosyasının korunduğunu kontrol et
if [ ! -f ".env" ]; then
    print_warning ".env dosyası bulunamadı!"
    print_info "Lütfen .env dosyasını oluşturun: cp .env.production.example .env"
fi

# Docker Compose kontrolü ve komut belirleme
print_info "Docker Compose kontrol ediliyor..."
if docker compose version &> /dev/null 2>&1; then
    DOCKER_COMPOSE_CMD="docker compose"
    print_success "Docker Compose mevcut (plugin)"
elif command -v docker-compose &> /dev/null && docker-compose --version &> /dev/null 2>&1; then
    DOCKER_COMPOSE_CMD="docker-compose"
    print_success "Docker Compose mevcut (standalone)"
else
    print_error "Docker Compose bulunamadı!"
    print_info "Lütfen Docker Compose'u kurun:"
    print_info "  sudo apt install -y docker-compose-plugin  # Plugin (önerilen)"
    print_info "  VEYA"
    print_info "  sudo apt install -y docker-compose  # Standalone"
    exit 1
fi

# Servisleri güncelle
echo
print_info "Docker servisleri güncelleniyor..."

if [ "$SERVICE" = "all" ]; then
    # Tüm servisleri güncelle
    if [ "$FORCE_BUILD" = "true" ] || [ "$FORCE_BUILD" = "force" ]; then
        print_info "Force rebuild yapılıyor..."
        $DOCKER_COMPOSE_CMD -f docker-compose.prod.yml build --no-cache
        $DOCKER_COMPOSE_CMD -f docker-compose.prod.yml up -d --build
    else
        print_info "Normal rebuild yapılıyor..."
        $DOCKER_COMPOSE_CMD -f docker-compose.prod.yml build
        $DOCKER_COMPOSE_CMD -f docker-compose.prod.yml up -d --build
    fi
else
    # Sadece belirli servisi güncelle
    print_info "Sadece $SERVICE servisi güncelleniyor..."
    if [ "$FORCE_BUILD" = "true" ] || [ "$FORCE_BUILD" = "force" ]; then
        $DOCKER_COMPOSE_CMD -f docker-compose.prod.yml build --no-cache $SERVICE
    else
        $DOCKER_COMPOSE_CMD -f docker-compose.prod.yml build $SERVICE
    fi
    $DOCKER_COMPOSE_CMD -f docker-compose.prod.yml up -d --no-deps $SERVICE
fi

print_success "Servisler güncellendi"

# Durum kontrolü
echo
print_info "Container durumları:"
$DOCKER_COMPOSE_CMD -f docker-compose.prod.yml ps

echo
print_info "Health check'ler kontrol ediliyor..."
sleep 5

# Health check
UNHEALTHY=$($DOCKER_COMPOSE_CMD -f docker-compose.prod.yml ps --format json 2>/dev/null | grep -i '"Health":"unhealthy"' || true)
if [ -n "$UNHEALTHY" ]; then
    print_warning "Bazı container'lar unhealthy durumda!"
    $DOCKER_COMPOSE_CMD -f docker-compose.prod.yml ps | grep -i unhealthy
else
    print_success "Tüm container'lar sağlıklı görünüyor"
fi

echo
print_success "Güncelleme tamamlandı!"
echo
print_info "Logları görüntülemek için:"
echo "  $DOCKER_COMPOSE_CMD -f docker-compose.prod.yml logs -f"
echo
print_info "Belirli bir servisin loglarını görmek için:"
echo "  $DOCKER_COMPOSE_CMD -f docker-compose.prod.yml logs -f $SERVICE"

