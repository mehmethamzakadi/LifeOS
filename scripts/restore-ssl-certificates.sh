#!/bin/bash
# ============================================
# Restore SSL Certificates Script
# ============================================
# Bu script, Let's Encrypt sertifikalarını deploy/nginx/ssl/ klasörüne kopyalar
# ============================================

set -e

# Renkli çıktı
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}=== SSL Sertifikalarını Geri Alma Script'i ===${NC}"
echo ""

# Sunucu bilgileri
SERVER_USER="${1:-lifeos}"
SERVER_HOST="${2:-45.143.4.244}"
SERVER_PATH="${3:-/opt/lifeos}"
DOMAIN="${4:-liferegistry.app}"

echo -e "${YELLOW}Sunucu Bilgileri:${NC}"
echo "  Kullanıcı: $SERVER_USER"
echo "  Host: $SERVER_HOST"
echo "  Yol: $SERVER_PATH"
echo "  Domain: $DOMAIN"
echo ""

# Sunucuda çalıştır
ssh ${SERVER_USER}@${SERVER_HOST} << EOF
    set -e
    
    # SSL klasörünü oluştur
    mkdir -p ${SERVER_PATH}/deploy/nginx/ssl
    chmod 700 ${SERVER_PATH}/deploy/nginx/ssl
    
    # Let's Encrypt sertifikalarını kontrol et
    if [ -f "/etc/letsencrypt/live/${DOMAIN}/fullchain.pem" ]; then
        echo "✓ Let's Encrypt sertifikaları bulundu"
        
        # Sertifikaları kopyala (sudo gerekebilir)
        if sudo test -f "/etc/letsencrypt/live/${DOMAIN}/fullchain.pem"; then
            sudo cp /etc/letsencrypt/live/${DOMAIN}/fullchain.pem ${SERVER_PATH}/deploy/nginx/ssl/
            sudo cp /etc/letsencrypt/live/${DOMAIN}/privkey.pem ${SERVER_PATH}/deploy/nginx/ssl/
            sudo chown \$(whoami):\$(whoami) ${SERVER_PATH}/deploy/nginx/ssl/*.pem
            chmod 600 ${SERVER_PATH}/deploy/nginx/ssl/*.pem
            
            echo "✓ SSL sertifikaları kopyalandı"
            echo ""
            echo "Kopyalanan dosyalar:"
            ls -lh ${SERVER_PATH}/deploy/nginx/ssl/
        else
            echo -e "${RED}HATA: Sertifikalara erişim izni yok (sudo gerekli)${NC}"
            echo ""
            echo "Manuel olarak çalıştırın:"
            echo "  ssh ${SERVER_USER}@${SERVER_HOST}"
            echo "  sudo cp /etc/letsencrypt/live/${DOMAIN}/fullchain.pem ${SERVER_PATH}/deploy/nginx/ssl/"
            echo "  sudo cp /etc/letsencrypt/live/${DOMAIN}/privkey.pem ${SERVER_PATH}/deploy/nginx/ssl/"
            echo "  sudo chown \$(whoami):\$(whoami) ${SERVER_PATH}/deploy/nginx/ssl/*.pem"
            echo "  chmod 600 ${SERVER_PATH}/deploy/nginx/ssl/*.pem"
            exit 1
        fi
    else
        echo -e "${YELLOW}UYARI: Let's Encrypt sertifikaları bulunamadı${NC}"
        echo "Sertifikalar mevcut değilse, yeniden oluşturmanız gerekebilir:"
        echo "  sudo certbot certonly --standalone -d ${DOMAIN} -d www.${DOMAIN}"
        exit 1
    fi
EOF

echo ""
echo -e "${GREEN}=== Tamamlandı! ===${NC}"
echo ""
echo -e "${YELLOW}Sonraki Adımlar:${NC}"
echo "1. Nginx container'ını yeniden başlatın:"
echo "   ssh ${SERVER_USER}@${SERVER_HOST}"
echo "   cd ${SERVER_PATH}"
echo "   docker-compose -f docker-compose.yml -f docker-compose.prod.yml restart nginx"
echo ""

