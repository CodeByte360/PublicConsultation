# ============================================
# Deployment Script - Public Consultation
# Run on Windows Server: 192.144.80.202
# ============================================

param(
    [string]$AppDir = "C:\Apps\PublicConsultation"
)

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  Deploying Public Consultation" -ForegroundColor Cyan  
Write-Host "  $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

Set-Location $AppDir

# --- 1. Pull latest code ---
Write-Host "`n[1/4] Pulling latest code..." -ForegroundColor Yellow
git pull origin main
Write-Host "  Done" -ForegroundColor Green

# --- 2. Build new image ---
Write-Host "`n[2/4] Building Docker image..." -ForegroundColor Yellow
docker compose build --no-cache blazor-app
Write-Host "  Done" -ForegroundColor Green

# --- 3. Restart services ---
Write-Host "`n[3/4] Restarting services..." -ForegroundColor Yellow
docker compose up -d --force-recreate blazor-app
Write-Host "  Done" -ForegroundColor Green

# --- 4. Health check ---
Write-Host "`n[4/4] Running health check..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

$maxRetries = 5
for ($i = 1; $i -le $maxRetries; $i++) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8080/" -UseBasicParsing -TimeoutSec 10
        if ($response.StatusCode -eq 200) {
            Write-Host "  Health check PASSED (HTTP $($response.StatusCode))" -ForegroundColor Green
            break
        }
    } catch {
        if ($i -eq $maxRetries) {
            Write-Host "  Health check FAILED after $maxRetries retries" -ForegroundColor Red
            Write-Host "  Showing logs:" -ForegroundColor Yellow
            docker compose logs blazor-app --tail=30
            exit 1
        }
        Write-Host "  Attempt $i/$maxRetries - retrying in 10s..." -ForegroundColor Yellow
        Start-Sleep -Seconds 10
    }
}

# --- Cleanup ---
Write-Host "`nCleaning up old images..." -ForegroundColor Yellow
docker image prune -f
Write-Host "  Done" -ForegroundColor Green

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  Deployment Complete!" -ForegroundColor Green
Write-Host "  https://staging-terasupport-rdp.arcapps.org" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan
