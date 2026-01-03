#!/bin/bash
# ============================================
# Restore deploy/ Folder Script
# ============================================
# Bu script, sunucuda silinen deploy/ klasörünü geri alır
# ============================================

set -e

# Renkli çıktı
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}=== Deploy Klasörünü Geri Alma Script'i ===${NC}"
echo ""

# Sunucu bilgileri (gerekirse düzenleyin)
SERVER_USER="${1:-lifeos}"
SERVER_HOST="${2:-45.143.4.244}"
SERVER_PATH="${3:-/opt/lifeos}"

echo -e "${YELLOW}Sunucu Bilgileri:${NC}"
echo "  Kullanıcı: $SERVER_USER"
echo "  Host: $SERVER_HOST"
echo "  Yol: $SERVER_PATH"
echo ""

# Mevcut dizini kontrol et
if [ ! -d "deploy/nginx" ]; then
    echo -e "${RED}HATA: deploy/nginx klasörü bulunamadı!${NC}"
    echo "Bu script'i proje kök dizininden çalıştırın."
    exit 1
fi

# Dosyaları kontrol et
if [ ! -f "deploy/nginx/default.conf" ] || [ ! -f "deploy/nginx/nginx.conf" ]; then
    echo -e "${RED}HATA: deploy/nginx/default.conf veya nginx.conf dosyası bulunamadı!${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Lokal dosyalar bulundu${NC}"
echo ""

# Sunucuya dosyaları kopyala
echo -e "${YELLOW}Sunucuya dosyalar kopyalanıyor...${NC}"

# deploy/nginx klasörünü oluştur
ssh ${SERVER_USER}@${SERVER_HOST} "mkdir -p ${SERVER_PATH}/deploy/nginx"

# Konfigürasyon dosyalarını kopyala
scp deploy/nginx/default.conf ${SERVER_USER}@${SERVER_HOST}:${SERVER_PATH}/deploy/nginx/
scp deploy/nginx/nginx.conf ${SERVER_USER}@${SERVER_HOST}:${SERVER_PATH}/deploy/nginx/

echo -e "${GREEN}✓ Konfigürasyon dosyaları kopyalandı${NC}"

# SSL ve certbot klasörlerini oluştur (dosyalar yoksa)
echo -e "${YELLOW}SSL ve certbot klasörleri kontrol ediliyor...${NC}"

ssh ${SERVER_USER}@${SERVER_HOST} << EOF
    cd ${SERVER_PATH}
    
    # SSL klasörünü oluştur (yoksa)
    if [ ! -d "deploy/nginx/ssl" ]; then
        mkdir -p deploy/nginx/ssl
        chmod 700 deploy/nginx/ssl
        echo "✓ deploy/nginx/ssl klasörü oluşturuldu"
    else
        echo "✓ deploy/nginx/ssl klasörü zaten var"
    fi
    
    # Certbot klasörünü oluştur (yoksa)
    if [ ! -d "deploy/nginx/certbot" ]; then
        mkdir -p deploy/nginx/certbot
        chmod 755 deploy/nginx/certbot
        echo "✓ deploy/nginx/certbot klasörü oluşturuldu"
    else
        echo "✓ deploy/nginx/certbot klasörü zaten var"
    fi
    
    # Dosya izinlerini kontrol et
    chmod 644 deploy/nginx/default.conf
    chmod 644 deploy/nginx/nginx.conf
    echo "✓ Dosya izinleri ayarlandı"
EOF

echo ""
echo -e "${GREEN}=== Tamamlandı! ===${NC}"
echo ""
echo -e "${YELLOW}Sonraki Adımlar:${NC}"
echo "1. SSL sertifikalarınız varsa, deploy/nginx/ssl/ klasörüne kopyalayın:"
echo "   ssh ${SERVER_USER}@${SERVER_HOST}"
echo "   sudo cp /etc/letsencrypt/live/liferegistry.app/fullchain.pem ${SERVER_PATH}/deploy/nginx/ssl/"
echo "   sudo cp /etc/letsencrypt/live/liferegistry.app/privkey.pem ${SERVER_PATH}/deploy/nginx/ssl/"
echo "   sudo chown -R \$(whoami):\$(whoami) ${SERVER_PATH}/deploy/nginx/ssl"
echo "   chmod 600 ${SERVER_PATH}/deploy/nginx/ssl/*.pem"
echo ""
echo "2. Docker container'ları yeniden başlatın:"
echo "   ssh ${SERVER_USER}@${SERVER_HOST}"
echo "   cd ${SERVER_PATH}"
echo "   docker-compose -f docker-compose.yml -f docker-compose.prod.yml restart nginx"
echo ""

