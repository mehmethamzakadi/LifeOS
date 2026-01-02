#!/bin/bash

# ============================================
# Docker Compose Komut Kontrolü
# ============================================

echo "Docker Compose komut kontrolü..."
echo

# Plugin versiyonu kontrol et
if docker compose version &> /dev/null 2>&1; then
    echo "✓ docker compose (plugin) çalışıyor"
    docker compose version
    echo
    echo "Komut: docker compose -f docker-compose.prod.yml up -d --build"
    exit 0
fi

# Standalone versiyonu kontrol et
if command -v docker-compose &> /dev/null && docker-compose --version &> /dev/null 2>&1; then
    echo "✓ docker-compose (standalone) çalışıyor"
    docker-compose --version
    echo
    echo "Komut: docker-compose -f docker-compose.prod.yml up -d --build"
    exit 0
fi

echo "✗ Docker Compose bulunamadı!"
echo "Lütfen Docker Compose'u kurun:"
echo "  sudo apt install -y docker-compose-plugin  # Plugin (önerilen)"
echo "  VEYA"
echo "  sudo apt install -y docker-compose  # Standalone"
exit 1

