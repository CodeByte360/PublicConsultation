# ============================================
# Server Setup Script for Public Consultation
# Run on Windows Server: 192.144.80.202
# Run as Administrator in PowerShell
# ============================================

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  Public Consultation - Windows Server Setup" -ForegroundColor Cyan
Write-Host "  Target: 192.144.80.202" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# --- 1. Check if running as Administrator ---
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "ERROR: Please run this script as Administrator!" -ForegroundColor Red
    exit 1
}

# --- 2. Install Docker Desktop ---
Write-Host "`n[1/5] Checking Docker..." -ForegroundColor Yellow

$dockerInstalled = Get-Command docker -ErrorAction SilentlyContinue
if (-not $dockerInstalled) {
    Write-Host "  Installing Docker Desktop..." -ForegroundColor White
    
    # Download Docker Desktop installer
    $dockerUrl = "https://desktop.docker.com/win/main/amd64/Docker%20Desktop%20Installer.exe"
    $installerPath = "$env:TEMP\DockerDesktopInstaller.exe"
    
    Write-Host "  Downloading Docker Desktop..." -ForegroundColor White
    Invoke-WebRequest -Uri $dockerUrl -OutFile $installerPath -UseBasicParsing
    
    Write-Host "  Running installer (this may take a few minutes)..." -ForegroundColor White
    Start-Process -Wait -FilePath $installerPath -ArgumentList "install", "--quiet", "--accept-license"
    
    Write-Host "  Docker Desktop installed. Please RESTART the server and run this script again." -ForegroundColor Green
    Write-Host "  After restart, make sure Docker Desktop is running." -ForegroundColor Yellow
    exit 0
} else {
    $version = docker --version
    Write-Host "  Docker already installed: $version" -ForegroundColor Green
}

# --- 3. Install IIS features for reverse proxy ---
Write-Host "`n[2/5] Configuring IIS..." -ForegroundColor Yellow

# Install URL Rewrite Module
$urlRewrite = Get-WebGlobalModule -Name "RewriteModule" -ErrorAction SilentlyContinue
if (-not $urlRewrite) {
    Write-Host "  Installing URL Rewrite Module..." -ForegroundColor White
    $rewriteUrl = "https://download.microsoft.com/download/1/2/8/128E2E22-C1B9-44A4-BE2A-5859ED1D4592/rewrite_amd64_en-US.msi"
    $rewriteInstaller = "$env:TEMP\rewrite_amd64.msi"
    Invoke-WebRequest -Uri $rewriteUrl -OutFile $rewriteInstaller -UseBasicParsing
    Start-Process msiexec.exe -Wait -ArgumentList "/i `"$rewriteInstaller`" /quiet /norestart"
    Write-Host "  URL Rewrite Module installed" -ForegroundColor Green
} else {
    Write-Host "  URL Rewrite Module already installed" -ForegroundColor Green
}

# Install ARR (Application Request Routing)
$arr = Get-WebGlobalModule -Name "ApplicationRequestRouting" -ErrorAction SilentlyContinue
if (-not $arr) {
    Write-Host "  Installing Application Request Routing (ARR)..." -ForegroundColor White
    $arrUrl = "https://download.microsoft.com/download/E/9/8/E9849D6A-020E-47E4-9FD0-A023E99B54EB/requestRouter_amd64.msi"
    $arrInstaller = "$env:TEMP\requestRouter_amd64.msi"
    Invoke-WebRequest -Uri $arrUrl -OutFile $arrInstaller -UseBasicParsing
    Start-Process msiexec.exe -Wait -ArgumentList "/i `"$arrInstaller`" /quiet /norestart"
    Write-Host "  ARR installed" -ForegroundColor Green
} else {
    Write-Host "  ARR already installed" -ForegroundColor Green
}

# Enable ARR proxy
Write-Host "  Enabling ARR Proxy..." -ForegroundColor White
try {
    Set-WebConfigurationProperty -pspath 'MACHINE/WEBROOT/APPHOST' -filter "system.webServer/proxy" -name "enabled" -value "True"
    Write-Host "  ARR Proxy enabled" -ForegroundColor Green
} catch {
    Write-Host "  ARR Proxy may already be enabled" -ForegroundColor Yellow
}

# --- 4. Install WebSocket Protocol ---
Write-Host "`n[3/5] Enabling WebSocket Protocol..." -ForegroundColor Yellow

$wsFeature = Get-WindowsFeature Web-WebSockets -ErrorAction SilentlyContinue
if ($wsFeature -and -not $wsFeature.Installed) {
    Install-WindowsFeature Web-WebSockets
    Write-Host "  WebSocket Protocol enabled" -ForegroundColor Green
} else {
    Write-Host "  WebSocket Protocol already enabled" -ForegroundColor Green
}

# --- 5. Create application directory ---
Write-Host "`n[4/5] Creating application directory..." -ForegroundColor Yellow

$appDir = "C:\Apps\PublicConsultation"
if (-not (Test-Path $appDir)) {
    New-Item -ItemType Directory -Path $appDir -Force | Out-Null
    Write-Host "  Created: $appDir" -ForegroundColor Green
} else {
    Write-Host "  Directory exists: $appDir" -ForegroundColor Green
}

# --- 6. Summary ---
Write-Host "`n[5/5] Setup complete!" -ForegroundColor Yellow
Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  Setup Complete!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Next steps:" -ForegroundColor White
Write-Host "  1. Clone your repo to: $appDir" -ForegroundColor White
Write-Host "  2. Copy .env.example to .env and update values" -ForegroundColor White
Write-Host "  3. Run: docker compose up -d --build" -ForegroundColor White
Write-Host "  4. Configure IIS site (see web.config)" -ForegroundColor White
Write-Host ""
Write-Host "  Access URLs:" -ForegroundColor White
Write-Host "  App:       https://staging-terasupport-rdp.arcapps.org" -ForegroundColor Green
Write-Host "  Portainer: https://192.144.80.202:9443" -ForegroundColor Green
Write-Host ""
