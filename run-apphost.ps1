#Requires -Version 5.1

<#
.SYNOPSIS
    Runs the EzraToDo AppHost with proper Aspire configuration and sets up the local environment.

.DESCRIPTION
    This script handles all local setup (including npm install for the UI) and starts 
    the Aspire orchestration. It automatically opens the UI, API Swagger, and 
    Aspire Dashboard in the default browser.
    
.EXAMPLE
    .\run-apphost.ps1
#>

param(
    [int]$DashboardPort = 18888,
    [switch]$NoDashboard
)

$ErrorActionPreference = "Stop"

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║    EzraToDo - Local Setup & Run Orchestration           ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# 1. Validate environment
$appHostPath = "EzraToDo.AppHost"
$uiPath = "ezratodo-ui"

if (-not (Test-Path $appHostPath)) {
    Write-Host "❌ Error: Cannot find $appHostPath directory. Run from project root." -ForegroundColor Red
    exit 1
}

# 2. Cleanup Zombie Processes (Best Practice for Local Dev)
Write-Host "🧹 Cleaning up existing processes..." -ForegroundColor Gray
Stop-Process -Name "EzraToDo.AppHost" -Force -ErrorAction SilentlyContinue
Stop-Process -Name "EzraToDo.Api" -Force -ErrorAction SilentlyContinue

# 3. Setup UI (npm install)
Write-Host "🚀 Setting up UI dependencies (npm install)..." -ForegroundColor Yellow
if (Test-Path "$uiPath\node_modules") {
    Write-Host "  ✅ node_modules found, skipping npm install (run 'npm install' manually to update)" -ForegroundColor Gray
} else {
    Push-Location $uiPath
    try {
        & npm install
        Write-Host "  ✅ UI dependencies installed successfully." -ForegroundColor Green
    } catch {
        Write-Host "  ❌ Failed to install UI dependencies. Ensure Node.js and npm are installed." -ForegroundColor Red
        exit 1
    }
    Pop-Location
}

# 4. Define Browser Launcher
$launchBrowsers = {
    param($uiUrl, $apiUrl, $dashboardUrl)
    Write-Host "⏳ Waiting for services to start before opening browser..." -ForegroundColor Gray
    Start-Sleep -Seconds 10
    
    Write-Host "🌐 Opening UI: $uiUrl" -ForegroundColor Cyan
    Start-Process $uiUrl
    
    Write-Host "🌐 Opening API Swagger: $apiUrl" -ForegroundColor Cyan
    Start-Process $apiUrl

    if ($dashboardUrl) {
        Write-Host "🌐 Opening Aspire Dashboard: $dashboardUrl" -ForegroundColor Cyan
        Start-Process $dashboardUrl
    }
}

# 5. Start AppHost and Launch Browser
Write-Host "`n🚀 Starting Aspire AppHost..." -ForegroundColor Green
$uiUrl = "http://localhost:3000"
$apiUrl = "https://localhost:5001/swagger"
$dashboardUrl = if ($NoDashboard) { $null } else { "http://localhost:$DashboardPort" }

# Launch browsers in a background job so they don't block the AppHost
Start-Job -ScriptBlock $launchBrowsers -ArgumentList $uiUrl, $apiUrl, $dashboardUrl | Out-Null

# Set environment for Aspire
$env:DOTNET_ENVIRONMENT = "Development"
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPIRE_DASHBOARD_PORT = $DashboardPort

try {
    Push-Location $appHostPath
    if ($NoDashboard) {
        & dotnet run -- --no-dashboard
    } else {
        & dotnet run
    }
} finally {
    Pop-Location
}
