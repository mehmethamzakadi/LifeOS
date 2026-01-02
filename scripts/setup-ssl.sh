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
EMAIL="${CERTBOT_EMAIL:-your-email@example.com}"
PROJECT_DIR="/opt/lifeos"

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}LifeOS SSL Sertifikası Kurulumu${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# 1. Certbot kurulumunu kontrol et
echo -e "${YELLOW}[1/7] Certbot kurulumunu kontrol ediliyor...${NC}"
if ! command -v certbot &> /dev/null; then
    echo -e "${RED}Certbot bulunamadı. Kuruluyor...${NC}"
    sudo apt update
    sudo apt install -y certbot
else
    echo -e "${GREEN}✓ Certbot zaten kurulu${NC}"
fi

# 2. DNS kontrolü
echo -e "${YELLOW}[2/7] DNS kaydını kontrol ediliyor...${NC}"
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

# 3. Mevcut container'ları durdur
echo -e "${YELLOW}[3/7] Mevcut container'lar durduruluyor...${NC}"
cd "$PROJECT_DIR" || exit 1
docker-compose -f docker-compose.yml -f docker-compose.prod.yml down 2>/dev/null || true
echo -e "${GREEN}✓ Container'lar durduruldu${NC}"

# 4. Geçici Nginx container'ı oluştur
echo -e "${YELLOW}[4/7] Geçici Nginx container'ı oluşturuluyor...${NC}"
sudo mkdir -p /tmp/certbot-nginx
sudo mkdir -p /var/www/certbot
sudo chmod -R 755 /var/www/certbot

sudo tee /tmp/certbot-nginx/default.conf > /dev/null <<EOF
server {
    listen 80;
    server_name $DOMAIN www.$DOMAIN;
    
    location /.well-known/acme-challenge/ {
        root /var/www/certbot;
    }
    
    location / {
        return 200 "Certbot verification in progress...";
        add_header Content-Type text/plain;
    }
}
EOF

# Eski geçici container'ı temizle
sudo docker stop certbot-nginx 2>/dev/null || true
sudo docker rm certbot-nginx 2>/dev/null || true

# Yeni geçici container'ı başlat
sudo docker run -d \
  --name certbot-nginx \
  -p 80:80 \
  -v /tmp/certbot-nginx:/etc/nginx/conf.d:ro \
  -v /var/www/certbot:/var/www/certbot \
  nginx:alpine

sleep 2
echo -e "${GREEN}✓ Geçici Nginx container'ı başlatıldı${NC}"

# 5. SSL sertifikası oluştur
echo -e "${YELLOW}[5/7] SSL sertifikası oluşturuluyor...${NC}"
echo -e "${YELLOW}  Email: $EMAIL${NC}"
echo -e "${YELLOW}  Domain: $DOMAIN, www.$DOMAIN${NC}"

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
    sudo docker stop certbot-nginx
    sudo docker rm certbot-nginx
    exit 1
  }

echo -e "${GREEN}✓ SSL sertifikası oluşturuldu${NC}"

# 6. Geçici container'ı durdur
echo -e "${YELLOW}[6/7] Geçici container temizleniyor...${NC}"
sudo docker stop certbot-nginx
sudo docker rm certbot-nginx
echo -e "${GREEN}✓ Geçici container temizlendi${NC}"

# 7. Sertifikaları proje klasörüne kopyala
echo -e "${YELLOW}[7/7] Sertifikalar proje klasörüne kopyalanıyor...${NC}"
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

