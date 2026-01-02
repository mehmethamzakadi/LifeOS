#!/bin/bash

# ============================================
# LifeOS - Production Diagnostic Script
# ============================================
# Sunucuda sorunları tespit etmek için kullanılır
# ============================================

set -e

# Renkler
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

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

echo "============================================"
echo "  LifeOS - Production Diagnostic Tool"
echo "============================================"
echo

# 1. Mevcut dizin kontrolü
print_info "1. Mevcut dizin kontrol ediliyor..."
if [ ! -f "docker-compose.prod.yml" ]; then
    print_error "docker-compose.prod.yml dosyası bulunamadı!"
    print_info "Lütfen proje dizinine gidin: cd /opt/lifeos"
    exit 1
fi
CURRENT_DIR=$(pwd)
print_success "Mevcut dizin: $CURRENT_DIR"

# 2. .env dosyası kontrolü
echo
print_info "2. .env dosyası kontrol ediliyor..."
if [ ! -f ".env" ]; then
    print_error ".env dosyası bulunamadı!"
    if [ -f ".env.example" ]; then
        print_info ".env.example dosyasından kopyalanıyor..."
        cp .env.example .env
        chmod 600 .env
        print_success ".env dosyası oluşturuldu (LÜTFEN DEĞERLERİ GÜNCELLEYİN!)"
    else
        print_error ".env.example dosyası da bulunamadı!"
        exit 1
    fi
else
    print_success ".env dosyası mevcut"
    
    # İzin kontrolü
    ENV_PERMS=$(stat -c %a .env 2>/dev/null || stat -f %A .env 2>/dev/null)
    print_info "  İzinler: $ENV_PERMS"
    
    if [ "$ENV_PERMS" != "600" ] && [ "$ENV_PERMS" != "644" ]; then
        print_warning ".env dosyası izinleri güvenli değil, düzeltiliyor..."
        chmod 600 .env
        print_success "İzinler düzeltildi (600)"
    fi
    
    # Dosya sahibi kontrolü
    ENV_OWNER=$(stat -c %U .env 2>/dev/null || stat -f %Su .env 2>/dev/null)
    CURRENT_USER=$(whoami)
    print_info "  Sahibi: $ENV_OWNER (Şu anki kullanıcı: $CURRENT_USER)"
    
    # Okuma izni kontrolü
    if [ ! -r ".env" ]; then
        print_error ".env dosyası okunamıyor!"
        print_info "İzinleri düzeltmek için: chmod 600 .env"
        print_info "Sahibini değiştirmek için: sudo chown $CURRENT_USER:$CURRENT_USER .env"
        exit 1
    else
        print_success ".env dosyası okunabilir"
    fi
fi

# 3. Docker kontrolü
echo
print_info "3. Docker kontrol ediliyor..."
if ! command -v docker &> /dev/null; then
    print_error "Docker kurulu değil!"
    exit 1
fi
print_success "Docker kurulu: $(docker --version)"

if ! docker info &> /dev/null; then
    print_error "Docker çalışmıyor veya erişim izniniz yok!"
    print_info "Docker grubuna eklenmek için: sudo usermod -aG docker $USER"
    print_info "Sonra oturumu kapatıp açın veya: newgrp docker"
    exit 1
fi
print_success "Docker çalışıyor"

# 4. Docker Compose kontrolü
echo
print_info "4. Docker Compose kontrol ediliyor..."
DOCKER_COMPOSE_CMD=""
if docker compose version &> /dev/null 2>&1; then
    DOCKER_COMPOSE_CMD="docker compose"
    print_success "Docker Compose mevcut (plugin): $(docker compose version | head -n1)"
elif command -v docker-compose &> /dev/null && docker-compose --version &> /dev/null 2>&1; then
    DOCKER_COMPOSE_CMD="docker-compose"
    print_success "Docker Compose mevcut (standalone): $(docker-compose --version)"
else
    print_error "Docker Compose kurulu değil!"
    print_info "Kurulum için:"
    print_info "  sudo apt install -y docker-compose-plugin  # Plugin (önerilen)"
    print_info "  VEYA"
    print_info "  sudo apt install -y docker-compose  # Standalone"
    exit 1
fi

# 5. .env dosyası içeriği kontrolü (ilk birkaç satır)
echo
print_info "5. .env dosyası içeriği kontrol ediliyor (ilk 5 satır)..."
head -n 5 .env | while IFS= read -r line; do
    # Hassas bilgileri gösterme
    if [[ $line == *"PASSWORD"* ]] || [[ $line == *"SECRET"* ]] || [[ $line == *"KEY"* ]]; then
        KEY=$(echo "$line" | cut -d'=' -f1)
        print_info "  $KEY=*** (gizli)"
    else
        print_info "  $line"
    fi
done

# 6. Docker Compose dosyalarını test et
echo
print_info "6. Docker Compose dosyaları test ediliyor..."
if $DOCKER_COMPOSE_CMD -f docker-compose.yml -f docker-compose.prod.yml config > /dev/null 2>&1; then
    print_success "Docker Compose dosyaları geçerli"
else
    print_error "Docker Compose dosyalarında hata var!"
    print_info "Detaylı hata için:"
    print_info "  $DOCKER_COMPOSE_CMD -f docker-compose.yml -f docker-compose.prod.yml config"
    $DOCKER_COMPOSE_CMD -f docker-compose.yml -f docker-compose.prod.yml config 2>&1 | head -20
    exit 1
fi

# 7. Disk alanı kontrolü
echo
print_info "7. Disk alanı kontrol ediliyor..."
DF_OUTPUT=$(df -h . | tail -1)
AVAILABLE=$(echo $DF_OUTPUT | awk '{print $4}')
print_info "  Kullanılabilir alan: $AVAILABLE"

# 8. Özet
echo
echo "============================================"
print_success "Tanılama tamamlandı!"
echo "============================================"
echo
print_info "Önerilen komutlar:"
echo "  # Build için:"
echo "  $DOCKER_COMPOSE_CMD -f docker-compose.yml -f docker-compose.prod.yml build --no-cache"
echo
echo "  # Veya Makefile ile:"
echo "  make prod-build"
echo
print_warning "ÖNEMLİ: .env dosyasındaki tüm değerleri production için güncellediğinizden emin olun!"

