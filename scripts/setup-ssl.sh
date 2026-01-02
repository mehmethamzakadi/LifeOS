#!/bin/bash

# ============================================
# LifeOS - SSL Sertifikası Kurulum Script'i
# ============================================
# Bu script, Let's Encrypt ile SSL sertifikası kurulumunu otomatikleştirir
# ============================================

set -e

# Renkler
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Domain ve email
DOMAIN="liferegistry.app"
EMAIL="${CERTBOT_EMAIL:-info@liferegistry.app}"
PROJECT_DIR="/opt/lifeos"

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}LifeOS SSL Sertifikası Kurulumu${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# 1. Certbot kurulumunu kontrol et
echo -e "${YELLOW}[1/5] Certbot kurulumunu kontrol ediliyor...${NC}"
if ! command -v certbot &> /dev/null; then
    echo -e "${RED}Certbot bulunamadı. Kuruluyor...${NC}"
    sudo apt update
    sudo apt install -y certbot
else
    echo -e "${GREEN}✓ Certbot zaten kurulu${NC}"
fi

# 2. DNS kontrolü
echo -e "${YELLOW}[2/5] DNS kaydını kontrol ediliyor...${NC}"
DNS_IP=$(dig +short $DOMAIN | tail -n1)
EXPECTED_IP="45.143.4.244"

if [ "$DNS_IP" != "$EXPECTED_IP" ]; then
    echo -e "${RED}✗ DNS kaydı doğru değil!${NC}"
    echo -e "${YELLOW}  Beklenen IP: $EXPECTED_IP${NC}"
    echo -e "${YELLOW}  Bulunan IP: $DNS_IP${NC}"
    echo -e "${YELLOW}  Lütfen DNS ayarlarınızı kontrol edin ve yayılmasını bekleyin.${NC}"
    read -p "Devam etmek istiyor musunuz? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
else
    echo -e "${GREEN}✓ DNS kaydı doğru ($DNS_IP)${NC}"
fi

# 3. Port 80'i kullanan servisleri durdur
echo -e "${YELLOW}[3/5] Port 80'i kullanan servisler durduruluyor...${NC}"
cd "$PROJECT_DIR" || exit 1

# Docker container'ları durdur
docker-compose -f docker-compose.yml -f docker-compose.prod.yml down 2>/dev/null || true

# Port 80'i kullanan tüm Docker container'larını durdur
ALL_CONTAINERS=$(sudo docker ps -q 2>/dev/null || true)
for container in $ALL_CONTAINERS; do
    if sudo docker port "$container" 2>/dev/null | grep -q ":80->"; then
        CONTAINER_NAME=$(sudo docker ps --format "{{.Names}}" --filter "id=$container" 2>/dev/null || echo "$container")
        echo -e "${YELLOW}  Port 80 kullanan container durduruluyor: $CONTAINER_NAME${NC}"
        sudo docker stop "$container" 2>/dev/null || true
    fi
done

# Sistem Nginx'ini durdur (eğer çalışıyorsa)
if sudo systemctl is-active --quiet nginx 2>/dev/null; then
    echo -e "${YELLOW}  Sistem Nginx durduruluyor...${NC}"
    sudo systemctl stop nginx 2>/dev/null || true
fi

# Port 80'i kullanan diğer process'leri kontrol et
PORT80_PID=$(sudo lsof -ti:80 2>/dev/null || true)
if [ -n "$PORT80_PID" ]; then
    echo -e "${YELLOW}  Port 80'i kullanan process'ler bulundu (PID: $PORT80_PID)${NC}"
    echo -e "${YELLOW}  Bu process'leri durdurmak için sudo gerekiyor...${NC}"
    echo "$PORT80_PID" | xargs -r sudo kill -9 2>/dev/null || true
    sleep 2
fi

# Port 80'in boş olduğunu kontrol et
if sudo lsof -ti:80 >/dev/null 2>&1; then
    echo -e "${RED}✗ Port 80 hala kullanımda! Lütfen manuel olarak kontrol edin:${NC}"
    echo -e "${YELLOW}  sudo lsof -i:80${NC}"
    echo -e "${YELLOW}  sudo netstat -tlnp | grep :80${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Port 80 boşaltıldı${NC}"

# 4. SSL sertifikası oluştur (standalone mod - kendi web server'ını başlatır)
echo -e "${YELLOW}[4/5] SSL sertifikası oluşturuluyor...${NC}"
echo -e "${YELLOW}  Email: $EMAIL${NC}"
echo -e "${YELLOW}  Domain: $DOMAIN, www.$DOMAIN${NC}"
echo -e "${YELLOW}  Not: Certbot standalone modu kendi web server'ını başlatacak${NC}"

sudo certbot certonly \
  --standalone \
  --preferred-challenges http \
  -d "$DOMAIN" \
  -d "www.$DOMAIN" \
  --email "$EMAIL" \
  --agree-tos \
  --non-interactive \
  --expand || {
    echo -e "${RED}✗ Sertifika oluşturma başarısız!${NC}"
    echo -e "${YELLOW}  Lütfen port 80'in boş olduğundan emin olun: sudo lsof -i:80${NC}"
    exit 1
  }

echo -e "${GREEN}✓ SSL sertifikası oluşturuldu${NC}"

# 5. Sertifikaları proje klasörüne kopyala
echo -e "${YELLOW}[5/5] Sertifikalar proje klasörüne kopyalanıyor...${NC}"
mkdir -p "$PROJECT_DIR/deploy/nginx/ssl"
mkdir -p "$PROJECT_DIR/deploy/nginx/certbot"

sudo cp /etc/letsencrypt/live/$DOMAIN/fullchain.pem "$PROJECT_DIR/deploy/nginx/ssl/"
sudo cp /etc/letsencrypt/live/$DOMAIN/privkey.pem "$PROJECT_DIR/deploy/nginx/ssl/"

sudo chown -R "$USER:$USER" "$PROJECT_DIR/deploy/nginx/ssl"
chmod 600 "$PROJECT_DIR/deploy/nginx/ssl"/*.pem

echo -e "${GREEN}✓ Sertifikalar kopyalandı${NC}"

# Özet
echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}✓ SSL Sertifikası Kurulumu Tamamlandı!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "${YELLOW}Sonraki adımlar:${NC}"
echo "1. .env dosyasını güncelleyin:"
echo "   APP_URL=https://$DOMAIN"
echo "   VITE_API_URL=https://$DOMAIN"
echo ""
echo "2. Client container'ını yeniden build edin:"
echo "   cd $PROJECT_DIR"
echo "   docker-compose -f docker-compose.yml -f docker-compose.prod.yml build --no-cache lifeos.client"
echo ""
echo "3. Tüm container'ları başlatın:"
echo "   docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d"
echo ""
echo -e "${YELLOW}Otomatik sertifika yenileme için cron job ekleyin:${NC}"
echo "   sudo crontab -e"
echo "   # Şu satırı ekleyin:"
echo "   0 3 * * * certbot renew --quiet --deploy-hook \"cd $PROJECT_DIR && docker-compose -f docker-compose.yml -f docker-compose.prod.yml restart nginx\""
echo ""

