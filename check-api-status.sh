#!/bin/bash
# API Container Durumunu Kontrol Et

echo "=== Container Durumları ==="
docker ps -a | grep lifeos

echo ""
echo "=== API Container Logs (Son 50 satır) ==="
docker logs --tail 50 lifeos_api_prod

echo ""
echo "=== API Container Environment Variables (CORS) ==="
docker exec lifeos_api_prod env 2>/dev/null | grep -i cors || echo "Container çalışmıyor veya erişilemiyor"

echo ""
echo "=== API Container Health Check ==="
docker inspect lifeos_api_prod --format='{{.State.Health.Status}}' 2>/dev/null || docker inspect lifeos_api_prod --format='{{.State.Status}}'

echo ""
echo "=== Database ve Redis Durumu ==="
docker ps | grep -E "postgres|redis"

