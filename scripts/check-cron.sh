#!/bin/bash

# ============================================
# LifeOS - Cron Job Kontrol Script'i
# ============================================

echo "=== Cron Job Kontrolü ==="
echo ""

# Renkler
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# 1. Mevcut kullanıcının cron job'ları
echo -e "${YELLOW}[1] Mevcut kullanıcı ($USER) cron job'ları:${NC}"
if crontab -l 2>/dev/null; then
    echo -e "${GREEN}✓ Kullanıcı cron job'ları bulundu${NC}"
else
    echo -e "${YELLOW}  Kullanıcı için cron job yok${NC}"
fi
echo ""

# 2. Root cron job'ları (SSL sertifikası genellikle burada)
echo -e "${YELLOW}[2] Root cron job'ları (sudo crontab -l):${NC}"
if sudo crontab -l 2>/dev/null; then
    echo -e "${GREEN}✓ Root cron job'ları bulundu${NC}"
else
    echo -e "${YELLOW}  Root için cron job yok${NC}"
fi
echo ""

# 3. Sistem cron job'ları
echo -e "${YELLOW}[3] Sistem cron job'ları (/etc/crontab):${NC}"
if [ -f /etc/crontab ]; then
    cat /etc/crontab | grep -v "^#" | grep -v "^$" || echo -e "${YELLOW}  Sistem cron job'u yok (sadece yorum satırları)${NC}"
else
    echo -e "${YELLOW}  /etc/crontab dosyası bulunamadı${NC}"
fi
echo ""

# 4. Certbot ile ilgili cron job'ları arama
echo -e "${YELLOW}[4] Certbot ile ilgili cron job'ları:${NC}"
FOUND=0

# Root crontab'ta ara
if sudo crontab -l 2>/dev/null | grep -q "certbot\|letsencrypt"; then
    echo -e "${GREEN}✓ Root crontab'ta certbot job'u bulundu:${NC}"
    sudo crontab -l 2>/dev/null | grep -E "certbot|letsencrypt" || true
    FOUND=1
fi

# Kullanıcı crontab'ta ara
if crontab -l 2>/dev/null | grep -q "certbot\|letsencrypt"; then
    echo -e "${GREEN}✓ Kullanıcı crontab'ta certbot job'u bulundu:${NC}"
    crontab -l 2>/dev/null | grep -E "certbot|letsencrypt" || true
    FOUND=1
fi

# Sistem crontab'ta ara
if [ -f /etc/crontab ] && grep -q "certbot\|letsencrypt" /etc/crontab 2>/dev/null; then
    echo -e "${GREEN}✓ Sistem crontab'ta certbot job'u bulundu:${NC}"
    grep -E "certbot|letsencrypt" /etc/crontab || true
    FOUND=1
fi

# /etc/cron.d/ klasöründe ara
if [ -d /etc/cron.d ]; then
    for file in /etc/cron.d/*; do
        if [ -f "$file" ] && grep -q "certbot\|letsencrypt" "$file" 2>/dev/null; then
            echo -e "${GREEN}✓ /etc/cron.d/$(basename $file) dosyasında certbot job'u bulundu:${NC}"
            grep -E "certbot|letsencrypt" "$file" || true
            FOUND=1
        fi
    done
fi

if [ $FOUND -eq 0 ]; then
    echo -e "${RED}✗ Certbot ile ilgili cron job bulunamadı${NC}"
    echo -e "${YELLOW}  SSL sertifikası otomatik yenileme için cron job eklenmemiş${NC}"
    echo ""
    echo -e "${YELLOW}Cron job eklemek için:${NC}"
    echo "  sudo crontab -e"
    echo "  # Şu satırı ekleyin:"
    echo "  0 3 * * * certbot renew --quiet --deploy-hook \"cd /opt/lifeos && docker-compose -f docker-compose.yml -f docker-compose.prod.yml restart nginx\""
fi

echo ""

