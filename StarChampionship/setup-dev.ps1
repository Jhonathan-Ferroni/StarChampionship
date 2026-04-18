#!/usr/bin/env pwsh
# ============================================
# Setup Local Development Environment
# Windows PowerShell Script
# ============================================
# Uso: .\setup-dev.ps1

Write-Host "🚀 StarChampionship - Setup Local Development" -ForegroundColor Cyan
Write-Host "=" * 50 -ForegroundColor Cyan

# Verificar se .env.example existe
if (-not (Test-Path ".env.example")) {
    Write-Host "❌ Arquivo .env.example não encontrado!" -ForegroundColor Red
    exit 1
}

# Verificar se .env já existe
if (Test-Path ".env") {
    Write-Host "⚠️  Arquivo .env já existe. Usando valores existentes..." -ForegroundColor Yellow
} else {
    Write-Host "📝 Criando arquivo .env a partir de .env.example..." -ForegroundColor Green
    Copy-Item ".env.example" ".env"
    Write-Host "✅ Arquivo .env criado. EDITE-O com suas valores reais!" -ForegroundColor Green
}

# Carregar variáveis do .env
Write-Host "📦 Carregando variáveis de ambiente..." -ForegroundColor Cyan
$envContent = Get-Content ".env" | Select-String "^[^#]" | Where-Object { $_ -match "=" }

foreach ($line in $envContent) {
    $key, $value = $line -split "=", 2
    $key = $key.Trim()
    $value = $value.Trim()
    [Environment]::SetEnvironmentVariable($key, $value, [EnvironmentVariableTarget]::Process)
    Write-Host "  ✓ $key = **** (protegido)" -ForegroundColor Gray
}

# Validar variáveis essenciais
Write-Host "`n🔍 Validando configuração..." -ForegroundColor Cyan

$required = @("JWT_SECRET_KEY", "ADMIN_PASSWORD")
$missing = @()

foreach ($var in $required) {
    $value = [Environment]::GetEnvironmentVariable($var)
    if (-not $value -or $value.StartsWith('$')) {
        $missing += $var
        Write-Host "  ❌ $var não configurado!" -ForegroundColor Red
    } else {
        Write-Host "  ✅ $var configurado" -ForegroundColor Green
    }
}

if ($missing.Count -gt 0) {
    Write-Host "`n⚠️  AVISO: As seguintes variáveis precisam ser configuradas:" -ForegroundColor Yellow
    $missing | ForEach-Object { Write-Host "   - $_" }
    Write-Host "`n📖 Edite o arquivo .env e execute o script novamente!" -ForegroundColor Yellow
    exit 1
}

# Restaurar dependências
Write-Host "`n📥 Restaurando dependências NuGet..." -ForegroundColor Cyan
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Erro ao restaurar dependências!" -ForegroundColor Red
    exit 1
}

# Limpar cache de build
Write-Host "`n🧹 Limpando cache de build anterior..." -ForegroundColor Cyan
Remove-Item -Path "bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "obj" -Recurse -Force -ErrorAction SilentlyContinue

# Build
Write-Host "`n🔨 Compilando projeto..." -ForegroundColor Cyan
dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Erro na compilação!" -ForegroundColor Red
    exit 1
}

# Executar testes
Write-Host "`n🧪 Executando testes de segurança..." -ForegroundColor Cyan
dotnet test
if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️  Alguns testes falharam. Revise antes de continuar." -ForegroundColor Yellow
}

Write-Host "`n" -ForegroundColor Cyan
Write-Host "=" * 50 -ForegroundColor Cyan
Write-Host "✅ Setup Completo!" -ForegroundColor Green
Write-Host "=" * 50 -ForegroundColor Cyan
Write-Host "`n🚀 Para iniciar o servidor com segurança:" -ForegroundColor Cyan
Write-Host "   dotnet run --launch-profile StarChampionshipSecure" -ForegroundColor White
Write-Host "`n📝 Endpoints disponíveis:" -ForegroundColor Cyan
Write-Host "   • Login: POST https://localhost:64913/api/auth/login" -ForegroundColor Gray
Write-Host "   • Players: GET  https://localhost:64913/api/players (público)" -ForegroundColor Gray
Write-Host "`n" -ForegroundColor Cyan
