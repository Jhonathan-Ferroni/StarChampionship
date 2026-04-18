#!/bin/bash
# ============================================
# Setup Local Development Environment
# Linux/Mac Bash Script
# ============================================
# Uso: chmod +x setup-dev.sh && ./setup-dev.sh

set -e

echo "🚀 StarChampionship - Setup Local Development"
echo "=================================================="

# Verificar se .env.example existe
if [ ! -f ".env.example" ]; then
    echo "❌ Arquivo .env.example não encontrado!"
    exit 1
fi

# Verificar se .env já existe
if [ -f ".env" ]; then
    echo "⚠️  Arquivo .env já existe. Usando valores existentes..."
else
    echo "📝 Criando arquivo .env a partir de .env.example..."
    cp .env.example .env
    chmod 600 .env  # Proteger permissões do arquivo
    echo "✅ Arquivo .env criado. EDITE-O com suas valores reais!"
fi

# Carregar variáveis do .env
echo "📦 Carregando variáveis de ambiente..."
set -a
source .env
set +a

# Validar variáveis essenciais
echo ""
echo "🔍 Validando configuração..."

MISSING=0

for var in JWT_SECRET_KEY ADMIN_PASSWORD; do
    if [ -z "${!var}" ] || [[ "${!var}" == "\$"* ]]; then
        echo "  ❌ $var não configurado!"
        MISSING=1
    else
        echo "  ✅ $var configurado"
    fi
done

if [ $MISSING -eq 1 ]; then
    echo ""
    echo "⚠️  AVISO: Configure as variáveis no arquivo .env"
    echo "📖 Edite o arquivo .env e execute o script novamente!"
    exit 1
fi

# Restaurar dependências
echo ""
echo "📥 Restaurando dependências NuGet..."
dotnet restore

# Limpar cache de build
echo ""
echo "🧹 Limpando cache de build anterior..."
rm -rf bin obj

# Build
echo ""
echo "🔨 Compilando projeto..."
dotnet build

# Executar testes
echo ""
echo "🧪 Executando testes de segurança..."
dotnet test || echo "⚠️  Alguns testes falharam. Revise antes de continuar."

echo ""
echo "=================================================="
echo "✅ Setup Completo!"
echo "=================================================="
echo ""
echo "🚀 Para iniciar o servidor com segurança:"
echo "   dotnet run --launch-profile StarChampionshipSecure"
echo ""
echo "📝 Endpoints disponíveis:"
echo "   • Login: POST https://localhost:64913/api/auth/login"
echo "   • Players: GET  https://localhost:64913/api/players (público)"
echo ""
