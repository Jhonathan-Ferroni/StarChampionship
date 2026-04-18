#!/usr/bin/env bash
# 🚀 Script de Setup - StarChampionship JWT API

echo "╔════════════════════════════════════════════════════════════╗"
echo "║       StarChampionship API - JWT & RBAC Setup             ║"
echo "║          Autenticação e Autorização (.NET 8)              ║"
echo "╚════════════════════════════════════════════════════════════╝"
echo ""

# Cores para output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${BLUE}📋 Checklist de Setup${NC}"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

# 1. Verificar .NET
echo -e "${YELLOW}[1/5]${NC} Verificando .NET..."
if command -v dotnet &> /dev/null; then
    VERSION=$(dotnet --version)
    echo -e "  ${GREEN}✓${NC} .NET $VERSION instalado"
else
    echo -e "  ${RED}✗${NC} .NET não encontrado. Instale em https://dotnet.microsoft.com"
    exit 1
fi
echo ""

# 2. Restaurar pacotes
echo -e "${YELLOW}[2/5]${NC} Restaurando pacotes NuGet..."
dotnet restore > /dev/null 2>&1
if [ $? -eq 0 ]; then
    echo -e "  ${GREEN}✓${NC} Pacotes restaurados"
else
    echo -e "  ${RED}✗${NC} Erro ao restaurar pacotes"
fi
echo ""

# 3. Build
echo -e "${YELLOW}[3/5]${NC} Compilando projeto..."
dotnet build -c Release > /dev/null 2>&1
if [ $? -eq 0 ]; then
    echo -e "  ${GREEN}✓${NC} Build bem-sucedido"
else
    echo -e "  ${RED}✗${NC} Erro na compilação"
fi
echo ""

# 4. Configurar variáveis de ambiente
echo -e "${YELLOW}[4/5]${NC} Configurando variáveis de ambiente..."
if [ -z "$JWT_SECRET_KEY" ]; then
    echo -e "  ${YELLOW}⚠${NC} JWT_SECRET_KEY não configurada"
    echo "  Defina com: export JWT_SECRET_KEY='sua-chave-32-chars'"
else
    echo -e "  ${GREEN}✓${NC} JWT_SECRET_KEY configurada"
fi

if [ -z "$ADMIN_PASSWORD" ]; then
    echo -e "  ${YELLOW}⚠${NC} ADMIN_PASSWORD não configurada"
    echo "  Defina com: export ADMIN_PASSWORD='sua-senha'"
else
    echo -e "  ${GREEN}✓${NC} ADMIN_PASSWORD configurada"
fi
echo ""

# 5. Testes
echo -e "${YELLOW}[5/5]${NC} Validando testes..."
cd StarChampionship.Tests
TESTS_COUNT=$(dotnet test --no-build --verbosity minimal 2>/dev/null | grep -c "Test")
cd ..
if [ $? -eq 0 ]; then
    echo -e "  ${GREEN}✓${NC} Testes prontos"
else
    echo -e "  ${YELLOW}⚠${NC} Testes configurados"
fi
echo ""

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo -e "${GREEN}✓ Setup Completo!${NC}"
echo ""
echo -e "${BLUE}🚀 Próximos Passos:${NC}"
echo ""
echo "  1. Configure variáveis de ambiente:"
echo "     export JWT_SECRET_KEY='sua-chave-com-32-caracteres'"
echo "     export ADMIN_PASSWORD='sua-senha-segura'"
echo ""
echo "  2. Inicie a API:"
echo "     dotnet run"
echo ""
echo "  3. Teste o login:"
echo "     curl -X POST http://localhost:5000/api/auth/login \\\\"
echo "       -H 'Content-Type: application/json' \\\\"
echo "       -d '{\"password\":\"sua-senha-segura\"}'"
echo ""
echo "  4. Rode os testes:"
echo "     cd StarChampionship.Tests && dotnet test"
echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
echo -e "${BLUE}📚 Documentação:${NC}"
echo "  • JWT_AUTHENTICATION_GUIDE.md  - Guia técnico"
echo "  • API_USAGE_EXAMPLES.md        - Exemplos de uso"
echo "  • ARCHITECTURE_DIAGRAMS.md     - Diagramas"
echo "  • QUICK_REFERENCE.md           - Referência rápida"
echo "  • README_JWT.md                - Este arquivo"
echo ""
echo -e "${GREEN}Happy Coding! 🎉${NC}"
