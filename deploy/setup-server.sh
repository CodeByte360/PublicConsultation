#!/bin/bash
# ============================================
# Server Setup Script for Public Consultation
# Run this ONCE on server: 192.144.80.202
# ============================================

set -e

echo "=========================================="
echo "  Public Consultation - Server Setup"
echo "  Target: 192.144.80.202"
echo "=========================================="

# --- 1. Install Docker ---
echo "[1/6] Installing Docker..."
if ! command -v docker &> /dev/null; then
    curl -fsSL https://get.docker.com -o get-docker.sh
    sh get-docker.sh
    rm get-docker.sh
    sudo usermod -aG docker $USER
    sudo systemctl enable docker
    sudo systemctl start docker
    echo "  ✅ Docker installed"
else
    echo "  ✅ Docker already installed: $(docker --version)"
fi

# --- 2. Install Docker Compose ---
echo "[2/6] Installing Docker Compose..."
if ! command -v docker compose &> /dev/null; then
    sudo apt-get update
    sudo apt-get install -y docker-compose-plugin
    echo "  ✅ Docker Compose installed"
else
    echo "  ✅ Docker Compose already installed: $(docker compose version)"
fi

# --- 3. Create application directory ---
echo "[3/6] Creating application directory..."
APP_DIR="/opt/public-consultation"
sudo mkdir -p $APP_DIR
sudo mkdir -p $APP_DIR/nginx/ssl
sudo mkdir -p $APP_DIR/nginx/certbot/www
sudo chown -R $USER:$USER $APP_DIR
echo "  ✅ Directory created: $APP_DIR"

# --- 4. Install Certbot for SSL ---
echo "[4/6] Installing Certbot for SSL..."
if ! command -v certbot &> /dev/null; then
    sudo apt-get update
    sudo apt-get install -y certbot
    echo "  ✅ Certbot installed"
else
    echo "  ✅ Certbot already installed"
fi

# --- 5. Generate SSL certificate ---
echo "[5/6] Generating SSL certificate..."
DOMAIN="staging-terasupport-rdp.arcapps.org"

if [ ! -f "/etc/letsencrypt/live/$DOMAIN/fullchain.pem" ]; then
    echo "  Generating certificate for $DOMAIN..."
    sudo certbot certonly --standalone \
        -d $DOMAIN \
        --non-interactive \
        --agree-tos \
        --email admin@arcapps.org \
        --http-01-port 80

    # Copy certs to nginx ssl directory
    sudo cp /etc/letsencrypt/live/$DOMAIN/fullchain.pem $APP_DIR/nginx/ssl/fullchain.pem
    sudo cp /etc/letsencrypt/live/$DOMAIN/privkey.pem $APP_DIR/nginx/ssl/privkey.pem
    sudo chown $USER:$USER $APP_DIR/nginx/ssl/*.pem

    # Setup auto-renewal cron
    (crontab -l 2>/dev/null; echo "0 3 * * * certbot renew --quiet && cp /etc/letsencrypt/live/$DOMAIN/fullchain.pem $APP_DIR/nginx/ssl/fullchain.pem && cp /etc/letsencrypt/live/$DOMAIN/privkey.pem $APP_DIR/nginx/ssl/privkey.pem && docker restart nginx-proxy") | crontab -
    echo "  ✅ SSL certificate generated and auto-renewal configured"
else
    echo "  ✅ SSL certificate already exists"
    sudo cp /etc/letsencrypt/live/$DOMAIN/fullchain.pem $APP_DIR/nginx/ssl/fullchain.pem
    sudo cp /etc/letsencrypt/live/$DOMAIN/privkey.pem $APP_DIR/nginx/ssl/privkey.pem
    sudo chown $USER:$USER $APP_DIR/nginx/ssl/*.pem
fi

# --- 6. Setup Portainer ---
echo "[6/6] Setting up Portainer..."
if ! docker ps -a --format '{{.Names}}' | grep -q '^portainer$'; then
    echo "  Portainer will be started with docker-compose"
fi
echo "  ✅ Portainer ready"

echo ""
echo "=========================================="
echo "  ✅ Setup Complete!"
echo "=========================================="
echo ""
echo "  Next steps:"
echo "  1. Copy your project files to: $APP_DIR"
echo "  2. Copy .env.example to .env and update values"
echo "  3. Run: cd $APP_DIR && docker compose up -d"
echo ""
echo "  Access URLs:"
echo "  🌐 Application: https://staging-terasupport-rdp.arcapps.org"
echo "  🔧 Portainer:   https://192.144.80.202:9443"
echo ""
