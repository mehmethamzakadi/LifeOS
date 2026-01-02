#!/bin/bash

# ============================================
# LifeOS - Production Deployment Script
# ============================================
# Bu script Ubuntu 22.04 VDS sunucusunda
# LifeOS uygulamasını production ortamında deploy eder
# ============================================

set -e  # Hata durumunda dur

# Renkler
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Fonksiyonlar
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

check_root() {
    if [ "$EUID" -eq 0 ]; then
        print_warning "Root olarak çalışıyorsunuz. Non-root user kullanmanız önerilir."
        read -p "Devam etmek istiyor musunuz? (y/n) " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            exit 1
        fi
    fi
}

check_docker() {
    print_info "Docker kurulumunu kontrol ediliyor..."
    if ! command -v docker &> /dev/null; then
        print_error "Docker kurulu değil! Lütfen önce Docker'ı kurun."
        exit 1
    fi
    
    if ! docker info &> /dev/null; then
        print_error "Docker çalışmıyor veya erişim izniniz yok!"
        print_info "Docker grubuna eklenmek için: sudo usermod -aG docker $USER"
        exit 1
    fi
    
    print_success "Docker kurulu ve çalışıyor"
}

check_docker_compose() {
    print_info "Docker Compose kurulumunu kontrol ediliyor..."
    
    # Docker Compose komutunu belirle
    if docker compose version &> /dev/null 2>&1; then
        DOCKER_COMPOSE_CMD="docker compose"
        print_success "Docker Compose mevcut (plugin)"
        docker compose version
    elif command -v docker-compose &> /dev/null && docker-compose --version &> /dev/null 2>&1; then
        DOCKER_COMPOSE_CMD="docker-compose"
        print_success "Docker Compose mevcut (standalone)"
        docker-compose --version
    else
        print_error "Docker Compose kurulu değil!"
        print_info "Lütfen Docker Compose'u kurun:"
        print_info "  sudo apt install -y docker-compose-plugin  # Plugin (önerilen)"
        print_info "  VEYA"
        print_info "  sudo apt install -y docker-compose  # Standalone"
        exit 1
    fi
    
    # Global değişken olarak export et (script içinde kullanmak için)
    export DOCKER_COMPOSE_CMD
}

check_env_file() {
    print_info ".env dosyası kontrol ediliyor..."
    if [ ! -f ".env" ]; then
        print_error ".env dosyası bulunamadı!"
        print_info "Lütfen .env dosyasını oluşturun: cp .env.example .env"
        print_info "Ve tüm değerleri doldurun!"
        exit 1
    fi
    print_success ".env dosyası mevcut"
    
    # .env dosyası güvenlik kontrolü
    if [ "$(stat -c %a .env)" != "600" ]; then
        print_warning ".env dosyası izinleri güvenli değil (600 olmalı)"
        chmod 600 .env
        print_success ".env dosyası izinleri düzeltildi"
    fi
}

check_ssl_certificates() {
    print_info "SSL sertifikaları kontrol ediliyor..."
    
    if [ -f "deploy/nginx/ssl/fullchain.pem" ] && [ -f "deploy/nginx/ssl/privkey.pem" ]; then
        print_success "SSL sertifikaları mevcut"
        return 0
    else
        print_warning "SSL sertifikaları bulunamadı!"
        print_info "SSL sertifikaları yoksa HTTP üzerinden devam edebilirsiniz"
        read -p "SSL olmadan devam etmek istiyor musunuz? (y/n) " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            print_info "SSL sertifikası kurulumu için DEPLOYMENT.md dosyasına bakın"
            exit 1
        fi
        return 1
    fi
}

stop_existing_containers() {
    print_info "Mevcut container'lar durduruluyor..."
    $DOCKER_COMPOSE_CMD -f docker-compose.prod.yml down 2>/dev/null || true
    print_success "Mevcut container'lar durduruldu"
}

build_images() {
    print_info "Docker image'ları build ediliyor..."
    $DOCKER_COMPOSE_CMD -f docker-compose.prod.yml build --no-cache
    print_success "Docker image'ları build edildi"
}

start_services() {
    print_info "Servisler başlatılıyor..."
    $DOCKER_COMPOSE_CMD -f docker-compose.prod.yml up -d
    print_success "Servisler başlatıldı"
}

wait_for_health() {
    print_info "Servislerin sağlıklı olması bekleniyor (maksimum 2 dakika)..."
    
    max_attempts=60
    attempt=0
    
    while [ $attempt -lt $max_attempts ]; do
        if $DOCKER_COMPOSE_CMD -f docker-compose.prod.yml ps 2>/dev/null | grep -q "healthy"; then
            print_success "Servisler sağlıklı durumda"
            return 0
        fi
        attempt=$((attempt + 1))
        echo -n "."
        sleep 2
    done
    
    echo
    print_warning "Bazı servisler henüz sağlıklı değil, logları kontrol edin"
    return 1
}

show_status() {
    print_info "Container durumları:"
    $DOCKER_COMPOSE_CMD -f docker-compose.prod.yml ps
    
    echo
    print_info "Health check sonuçları:"
    $DOCKER_COMPOSE_CMD -f docker-compose.prod.yml ps --format json 2>/dev/null | \
        grep -o '"Health":"[^"]*"' || echo "Health check bilgisi alınamadı"
}

show_logs_command() {
    echo
    print_info "Logları görüntülemek için:"
    echo "  $DOCKER_COMPOSE_CMD -f docker-compose.prod.yml logs -f"
    echo
    print_info "Belirli bir servisin loglarını görmek için:"
    echo "  $DOCKER_COMPOSE_CMD -f docker-compose.prod.yml logs -f lifeos.api"
    echo "  $DOCKER_COMPOSE_CMD -f docker-compose.prod.yml logs -f nginx"
}

# Ana işlem
main() {
    echo "============================================"
    echo "  LifeOS - Production Deployment Script"
    echo "============================================"
    echo
    
    check_root
    check_docker
    check_docker_compose
    check_env_file
    check_ssl_certificates
    SSL_EXISTS=$?
    
    echo
    print_info "Deployment başlatılıyor..."
    echo
    
    stop_existing_containers
    build_images
    start_services
    
    echo
    wait_for_health
    
    echo
    show_status
    
    echo
    print_success "Deployment tamamlandı!"
    
    if [ $SSL_EXISTS -eq 0 ]; then
        echo
        print_info "Uygulamanıza erişebilirsiniz:"
        echo "  HTTPS: https://yourdomain.com"
        echo "  API Health: https://yourdomain.com/health"
    else
        echo
        print_warning "SSL sertifikası olmadan çalışıyorsunuz!"
        print_info "HTTP üzerinden erişebilirsiniz: http://your-server-ip"
        print_info "SSL sertifikası kurulumu için DEPLOYMENT.md dosyasına bakın"
    fi
    
    show_logs_command
}

# Script'i çalıştır
main

