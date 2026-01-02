#!/bin/bash
# Nginx Konfigürasyonunu Kontrol Et

echo "=== Nginx Container Logs ==="
docker logs --tail 50 lifeos_nginx

echo ""
echo "=== Nginx Konfigürasyon Testi ==="
docker exec lifeos_nginx nginx -t 2>&1

echo ""
echo "=== Nginx Konfigürasyon Dosyası ==="
docker exec lifeos_nginx cat /etc/nginx/conf.d/default.conf 2>&1 | head -100

echo ""
echo "=== Host'ta Nginx Konfigürasyon Dosyası ==="
ls -la deploy/nginx/default.conf 2>&1
cat deploy/nginx/default.conf 2>&1 | head -50

echo ""
echo "=== Container İçindeki Volume Mount'ları ==="
docker inspect lifeos_nginx --format='{{range .Mounts}}{{.Source}} -> {{.Destination}} ({{.Type}}){{println}}{{end}}'

echo ""
echo "=== Client Container Durumu ==="
docker ps | grep client
docker exec lifeos_client_prod curl -f http://localhost/ 2>&1 | head -20

echo ""
echo "=== API Container Durumu ==="
docker ps | grep api
docker exec lifeos_api_prod wget -O- http://localhost:8080/health 2>&1 || echo "wget yok, başka test gerekli"

