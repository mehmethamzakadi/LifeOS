#!/bin/bash
# Tüm Container Durumlarını Kontrol Et

echo "=== Container Durumları ==="
docker ps -a | grep lifeos

echo ""
echo "=== API Health Check ==="
docker exec lifeos_api_prod curl -f http://localhost:8080/health 2>&1 || echo "API health check başarısız"

echo ""
echo "=== API Container Health Status ==="
docker inspect lifeos_api_prod --format='Health: {{.State.Health.Status}}' 2>/dev/null || docker inspect lifeos_api_prod --format='Status: {{.State.Status}}'

echo ""
echo "=== Nginx Logs (Son 20 satır) ==="
docker logs --tail 20 lifeos_nginx 2>/dev/null || echo "Nginx container loglanamıyor"

echo ""
echo "=== Client Logs (Son 20 satır) ==="
docker logs --tail 20 lifeos_client_prod 2>/dev/null || echo "Client container loglanamıyor"

