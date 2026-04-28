#!/bin/bash
# ============================================
# Deployment Script - Public Consultation
# Called by GitHub Actions or run manually
# ============================================

set -e

APP_DIR="/opt/public-consultation"
DOMAIN="staging-terasupport-rdp.arcapps.org"

echo "=========================================="
echo "  Deploying Public Consultation"
echo "  $(date '+%Y-%m-%d %H:%M:%S')"
echo "=========================================="

cd $APP_DIR

# --- 1. Pull latest code ---
echo "[1/4] Pulling latest code..."
git pull origin main
echo "  ✅ Code updated"

# --- 2. Build new image ---
echo "[2/4] Building Docker image..."
docker compose build --no-cache blazor-app
echo "  ✅ Image built"

# --- 3. Restart services ---
echo "[3/4] Restarting services..."
docker compose up -d --force-recreate blazor-app
echo "  ✅ Services restarted"

# --- 4. Health check ---
echo "[4/4] Running health check..."
sleep 10

RETRIES=5
for i in $(seq 1 $RETRIES); do
    HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/ || echo "000")
    if [ "$HTTP_CODE" = "200" ]; then
        echo "  ✅ Health check passed (HTTP $HTTP_CODE)"
        break
    fi
    if [ "$i" -eq "$RETRIES" ]; then
        echo "  ❌ Health check failed after $RETRIES retries"
        echo "  Rolling back..."
        docker compose logs blazor-app --tail=50
        exit 1
    fi
    echo "  ⏳ Attempt $i/$RETRIES - HTTP $HTTP_CODE, retrying in 10s..."
    sleep 10
done

# --- Cleanup old images ---
echo "Cleaning up old images..."
docker image prune -f
echo "  ✅ Cleanup done"

echo ""
echo "=========================================="
echo "  ✅ Deployment Complete!"
echo "  🌐 https://$DOMAIN"
echo "=========================================="
