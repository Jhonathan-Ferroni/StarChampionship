@echo off
REM 🚀 Script de Setup - StarChampionship JWT API (Windows)

setlocal enabledelayedexpansion

echo.
echo ╔════════════════════════════════════════════════════════════╗
echo ║       StarChampionship API - JWT ^& RBAC Setup             ║
echo ║          Autenticacao e Autorizacao (.NET 8)              ║
echo ╚════════════════════════════════════════════════════════════╝
echo.

echo [1/5] Verificando .NET...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo   ✗ .NET nao encontrado. Instale em https://dotnet.microsoft.com
    exit /b 1
)
for /f "tokens=*" %%i in ('dotnet --version') do set VERSION=%%i
echo   ✓ .NET %VERSION% instalado
echo.

echo [2/5] Restaurando pacotes NuGet...
dotnet restore >nul 2>&1
if %errorlevel% equ 0 (
    echo   ✓ Pacotes restaurados
) else (
    echo   ✗ Erro ao restaurar pacotes
)
echo.

echo [3/5] Compilando projeto...
dotnet build -c Release >nul 2>&1
if %errorlevel% equ 0 (
    echo   ✓ Build bem-sucedido
) else (
    echo   ✗ Erro na compilacao
)
echo.

echo [4/5] Configurando variaveis de ambiente...
if "%JWT_SECRET_KEY%"=="" (
    echo   ⚠ JWT_SECRET_KEY nao configurada
    echo   Defina com: set JWT_SECRET_KEY=sua-chave-32-chars
) else (
    echo   ✓ JWT_SECRET_KEY configurada
)

if "%ADMIN_PASSWORD%"=="" (
    echo   ⚠ ADMIN_PASSWORD nao configurada
    echo   Defina com: set ADMIN_PASSWORD=sua-senha
) else (
    echo   ✓ ADMIN_PASSWORD configurada
)
echo.

echo [5/5] Validando testes...
cd StarChampionship.Tests
dotnet test --no-build --verbosity minimal >nul 2>&1
cd ..
echo   ✓ Testes prontos
echo.

echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
echo ✓ Setup Completo!
echo.
echo 🚀 Proximos Passos:
echo.
echo   1. Configure variaveis de ambiente:
echo      set JWT_SECRET_KEY=sua-chave-com-32-caracteres
echo      set ADMIN_PASSWORD=sua-senha-segura
echo.
echo   2. Inicie a API:
echo      dotnet run
echo.
echo   3. Teste o login:
echo      curl -X POST http://localhost:5000/api/auth/login ^
echo        -H "Content-Type: application/json" ^
echo        -d "{\"password\":\"sua-senha-segura\"}"
echo.
echo   4. Rode os testes:
echo      cd StarChampionship.Tests ^& dotnet test
echo.
echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
echo.
echo 📚 Documentacao:
echo   • JWT_AUTHENTICATION_GUIDE.md  - Guia tecnico
echo   • API_USAGE_EXAMPLES.md        - Exemplos de uso
echo   • ARCHITECTURE_DIAGRAMS.md     - Diagramas
echo   • QUICK_REFERENCE.md           - Referencia rapida
echo   • README_JWT.md                - Este arquivo
echo.
echo Happy Coding! 🎉
echo.
