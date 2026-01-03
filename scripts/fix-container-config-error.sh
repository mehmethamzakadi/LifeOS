#!/bin/bash
# ============================================
# Fix ContainerConfig KeyError Script
# ============================================
# Bu script, docker-compose ContainerConfig hatasını düzeltir
# ============================================

set -e

# Renkli çıktı
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}=== ContainerConfig Hatasını Düzeltme Script'i ===${NC}"
echo ""

# Mevcut container'ları kontrol et
echo -e "${YELLOW}Mevcut container'lar kontrol ediliyor...${NC}"
CONTAINERS=$(docker ps -a --filter "name=lifeos" --format "{{.Names}}" 2>/dev/null || echo "")

if [ -z "$CONTAINERS" ]; then
    echo -e "${YELLOW}LifeOS container'ları bulunamadı.${NC}"
else
    echo -e "${YELLOW}Bulunan container'lar:${NC}"
    echo "$CONTAINERS"
    echo ""
    
    # Container'ları durdur
    echo -e "${YELLOW}Container'lar durduruluyor...${NC}"
    docker stop $CONTAINERS 2>/dev/null || true
    echo -e "${GREEN}✓ Container'lar durduruldu${NC}"
    
    # Container'ları sil
    echo -e "${YELLOW}Container'lar siliniyor...${NC}"
    docker rm $CONTAINERS 2>/dev/null || true
    echo -e "${GREEN}✓ Container'lar silindi${NC}"
fi

echo ""
echo -e "${GREEN}=== Tamamlandı! ===${NC}"
echo ""
echo -e "${YELLOW}Sonraki Adımlar:${NC}"
echo "Şimdi make prod komutunu tekrar çalıştırabilirsiniz:"
echo "  make prod"
echo ""

