# StarChampionship Backend API

API REST em **ASP.NET Core 8** para cadastro de jogadores, autenticacao administrativa com JWT e geracao de times balanceados.

Este repositorio representa **o backend do StarChampionship**. Ele nao e a aplicacao completa para usuario final e nao substitui o frontend. O papel dele e expor endpoints HTTP, validar entrada, acessar o banco MySQL, executar regras de negocio e devolver respostas em JSON.

## O que este repositorio e

- uma API/backend em .NET 8
- um servidor HTTP para consumo por frontend, Swagger, Postman ou testes automatizados
- a camada que persiste jogadores e calcula geracoes de times

## O que este repositorio nao e

- nao e o frontend React
- nao e um painel administrativo completo para usuario final
- nao e uma SPA
- nao e um projeto monolitico de ponta a ponta

Observacao importante:

- embora o repositorio ainda tenha `wwwroot/` e artefatos de fases anteriores, o `Program.cs` atual registra **apenas controllers** com `AddControllers()` e publica a API com `MapControllers()`
- nao ha configuracao ativa de Razor Views nem rota MVC tradicional no bootstrap atual
- na pratica, o contrato publico deste projeto e **API JSON**

## Visao geral do que a API faz hoje

A API cobre quatro frentes principais:

1. autenticacao de administrador via senha + JWT
2. CRUD de jogadores
3. endpoints basicos de informacao e health check
4. geracao de times equilibrados a partir de jogadores selecionados

## Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- Pomelo.EntityFrameworkCore.MySql
- MySQL
- Swagger / OpenAPI
- xUnit no projeto de testes
- Docker para empacotamento

## Como a API gera JSON

A API nao monta JSON manualmente em arquivos. Ela gera JSON a partir de objetos C# retornados pelos controllers e middlewares.

O fluxo atual e este:

1. O cliente chama um endpoint HTTP.
2. O controller ou middleware monta um objeto C# ou um resultado HTTP.
3. O ASP.NET Core serializa esse resultado para JSON.
4. O cliente recebe o corpo serializado na resposta.

Isso acontece principalmente de tres formas no projeto:

### 1. Controllers retornando objetos C#

Exemplos do codigo atual:

- `return Ok(players);`
- `return Ok(player);`
- `return CreatedAtAction(nameof(GetById), new { id = player.Id }, player);`
- `return Ok(new { Teams = selected.Teams, Score = selected.Score });`

Nesses casos, o ASP.NET Core usa o serializador padrao da plataforma para converter objetos e colecoes em JSON.

### 2. Middlewares e handlers escrevendo JSON explicitamente

O projeto usa `WriteAsJsonAsync(...)` em alguns pontos para garantir resposta JSON, por exemplo:

- `401 Unauthorized` no desafio JWT
- `403 Forbidden`
- erro `500` em producao

Exemplos de payload retornado:

```json
{ "error": "Unauthorized" }
```

```json
{ "error": "Forbidden" }
```

```json
{ "error": "An internal server error occurred." }
```

### 3. Swagger expondo o contrato da API em JSON

Em ambiente `Development`, o Swagger publica a especificacao OpenAPI em JSON, normalmente em:

```text
/swagger/v1/swagger.json
```

Esse JSON descreve os endpoints, modelos e respostas documentadas da API.

## Serializacao automatica e propriedades calculadas

Um ponto importante deste backend e que ele nao retorna apenas campos persistidos no banco. Algumas propriedades sao calculadas dinamicamente e entram no JSON final.

### Player

A classe `Player` tem os atributos tecnicos e uma propriedade calculada:

- `Overall`

Esse `Overall` e calculado em tempo de execucao pela media dos 8 atributos:

- `Shoot`
- `Dribble`
- `FirstTouch`
- `BallControl`
- `Defense`
- `Pass`
- `Speed`
- `Strength`

Ou seja: o cliente recebe `overall` no JSON mesmo que esse valor nao tenha sido digitado manualmente no request.

Exemplo de resposta de player:

```json
{
  "id": 1,
  "name": "Joao",
  "imageUrl": "https://exemplo.com/jogador.png",
  "shoot": 80,
  "dribble": 82,
  "firstTouch": 78,
  "ballControl": 83,
  "defense": 60,
  "pass": 76,
  "speed": 79,
  "strength": 74,
  "overall": 76.5
}
```

### Team

A classe `Team` tambem possui propriedades calculadas que aparecem no JSON:

- `TotalOverall`
- `AverageOverall`

Assim, uma resposta do gerador retorna nao so a lista de jogadores do time, mas tambem o total e a media calculados automaticamente.

Exemplo:

```json
{
  "id": 1,
  "name": "Time 1",
  "players": [],
  "totalOverall": 245.5,
  "averageOverall": 81.83
}
```

## Autenticacao JWT

A autenticacao administrativa usa senha simples configurada no backend e gera um token JWT com role `Admin`.

### Configuracoes envolvidas

Arquivo `StarChampionship/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "StarChampionshipContext": ""
  },
  "AdminConfig": {
    "Password": "senha"
  },
  "JwtSettings": {
    "SecretKey": "seu-chave-secreta-super-segura-com-pelo-menos-32-caracteres-para-HS256",
    "Issuer": "StarChampionshipApi",
    "Audience": "StarChampionshipUsers",
    "ExpirationMinutes": 60
  }
}
```

### Login principal

Endpoint confirmado no codigo:

```http
POST /api/auth/login
Content-Type: application/json
```

Request:

```json
{
  "password": "senha"
}
```

Resposta de sucesso:

```json
{
  "success": true,
  "message": "Login successful",
  "token": "jwt-aqui",
  "expiresAt": "2026-04-26T23:59:59Z"
}
```

Se a senha estiver errada, a API retorna `401` com JSON de erro.

### Endpoints protegidos

O CRUD mutavel de jogadores requer JWT com role `Admin`:

- `POST /api/players`
- `PUT /api/players/{id}`
- `DELETE /api/players/{id}`

Leituras publicas:

- `GET /api/players`
- `GET /api/players/{id}`

## Endpoints principais

Endpoints confirmados diretamente no codigo e nos testes:

| Metodo   | Endpoint            | Autenticacao | Descricao                |
| -------- | ------------------- | ------------ | ------------------------ |
| `POST`   | `/api/auth/login`   | publica      | gera token JWT de admin  |
| `GET`    | `/api/players`      | publica      | lista todos os jogadores |
| `GET`    | `/api/players/{id}` | publica      | detalhes de um jogador   |
| `POST`   | `/api/players`      | admin        | cria jogador             |
| `PUT`    | `/api/players/{id}` | admin        | atualiza jogador         |
| `DELETE` | `/api/players/{id}` | admin        | remove jogador           |

Outros controllers presentes no projeto:

- `AccountApiController`
- `HomeApiController`
- `GeneratorApiController`

Como esses controllers usam `Route("api/[controller]")` e seus nomes incluem o sufixo `Api`, use o **Swagger do ambiente em execucao** como fonte de verdade para confirmar os caminhos finais expostos por eles.

## Gerador de times

O backend inclui um servico de geracao de times (`GeneratorService`) e um controller dedicado para a API de geracao.

### O que ele faz

- recebe os IDs selecionados
- recebe a quantidade de times
- opcionalmente recebe capitaes fixos
- usa os jogadores carregados do banco
- executa multiplas tentativas de balanceamento
- mede a qualidade de cada tentativa
- devolve uma das melhores opcoes encontradas em JSON

### Como a logica funciona hoje

No fluxo atual do controller de geracao:

1. valida se ha jogadores suficientes para a quantidade de times
2. valida capitaes fixos sem repeticao
3. executa **200 geracoes candidatas**
4. calcula o `score` de cada geracao com base na dispersao dos totais dos times
5. filtra as geracoes que respeitam a `margin`
6. se nenhuma respeitar a margem, usa o melhor conjunto disponivel
7. seleciona aleatoriamente uma entre as melhores candidatas

### Exemplo de request de geracao

O corpo esperado pelo endpoint de geracao segue este shape:

```json
{
  "selectedIds": [10, 11, 12, 13],
  "numberOfTeams": 2,
  "hasFixedCaptains": true,
  "selectedCaptains": {
    "0": "10",
    "1": "11"
  },
  "margin": 5
}
```

### Exemplo de resposta do gerador

```json
{
  "teams": [
    {
      "id": 1,
      "name": "Time 1",
      "players": [
        {
          "id": 10,
          "name": "Joao",
          "overall": 82.25
        }
      ],
      "totalOverall": 164.5,
      "averageOverall": 82.25
    },
    {
      "id": 2,
      "name": "Time 2",
      "players": [
        {
          "id": 11,
          "name": "Carlos",
          "overall": 81.5
        }
      ],
      "totalOverall": 163,
      "averageOverall": 81.5
    }
  ],
  "score": 1.06
}
```

## Validacao e respostas de erro

A model `Player` usa Data Annotations como:

- `[Required]`
- `[StringLength]`
- `[Range(0, 100)]`

Quando o corpo enviado e invalido, o controller retorna `400 Bad Request`, normalmente serializando os erros do `ModelState`.

Exemplos de erros tratados no codigo atual:

- `400` para body invalido ou IDs inconsistentes
- `401` sem token ou senha invalida
- `403` token sem permissao suficiente
- `404` jogador nao encontrado
- `409` conflito de integridade ao deletar
- `500` erro interno

## Banco de dados e persistencia

A persistencia usa:

- `StarChampionshipContext`
- `DbSet<Player>`
- Entity Framework Core com MySQL

Connection string esperada:

```json
{
  "ConnectionStrings": {
    "StarChampionshipContext": "server=HOST;port=3306;database=DB;user=USER;password=PASSWORD"
  }
}
```

O projeto tambem executa `SeedingService` na inicializacao. No estado atual do codigo, esse seeding roda automaticamente, mas nao popula jogadores de exemplo por padrao.

## Estrutura do projeto

```text
.
|-- Dockerfile
|-- README.md
|-- StarChampionship/
|   |-- Controllers/
|   |-- Data/
|   |-- Models/
|   |-- Services/
|   |-- Properties/
|   |-- appsettings.json
|   |-- Program.cs
|   `-- StarChampionship.csproj
`-- StarChampionship.Tests/
```

Resumo:

- `Controllers/`: endpoints HTTP da API
- `Data/`: `DbContext` e seeding
- `Models/`: modelos de dominio e DTOs
- `Services/`: regras de negocio, JWT e geracao de times
- `StarChampionship.Tests/`: testes automatizados

## Como executar localmente

Pre-requisitos:

- .NET SDK 8
- MySQL disponivel
- connection string configurada
- segredo JWT configurado
- senha admin configurada

### 1. Configurar appsettings ou variaveis de ambiente

Campos minimos:

- `ConnectionStrings__StarChampionshipContext`
- `AdminConfig__Password`
- `JwtSettings__SecretKey`
- `JwtSettings__Issuer`
- `JwtSettings__Audience`
- `JwtSettings__ExpirationMinutes`

Exemplo no PowerShell:

```powershell
$env:ConnectionStrings__StarChampionshipContext="server=localhost;port=3306;database=StarChampionship;user=SEU_USER;password=SUA_SENHA"
$env:AdminConfig__Password="senha"
$env:JwtSettings__SecretKey="uma-chave-bem-grande-com-pelo-menos-32-caracteres"
$env:JwtSettings__Issuer="StarChampionshipApi"
$env:JwtSettings__Audience="StarChampionshipUsers"
$env:JwtSettings__ExpirationMinutes="60"
```

### 2. Restaurar pacotes

```bash
dotnet restore StarChampionship/StarChampionship.csproj
```

### 3. Aplicar migrations

```bash
dotnet ef database update --project StarChampionship/StarChampionship.csproj
```

### 4. Executar a API

```bash
dotnet run --project StarChampionship/StarChampionship.csproj
```

Observacoes:

- localmente, o `launchSettings.json` expõe URLs como `https://localhost:64913` e `http://localhost:64914`
- em hospedagem tipo Render, o projeto tambem respeita a variavel `PORT` e usa `10000` como fallback

## Swagger

Em `Development`, o Swagger fica habilitado para inspecao e testes da API.

Use o Swagger para:

- confirmar as rotas exatas do ambiente atual
- visualizar schemas JSON
- testar autenticacao JWT pelo botao `Authorize`
- validar requests e responses sem depender do frontend

## Docker

O repositrio inclui `Dockerfile` multi-stage para build e runtime.

Exemplo de build:

```bash
docker build -t starchampionship-backend .
```

Exemplo de run:

```bash
docker run --rm -p 10000:10000 \
  -e ConnectionStrings__StarChampionshipContext="server=SEU_HOST;port=3306;database=SEU_DB;user=SEU_USER;password=SUA_SENHA" \
  -e AdminConfig__Password="senha" \
  -e JwtSettings__SecretKey="uma-chave-bem-grande-com-pelo-menos-32-caracteres" \
  -e JwtSettings__Issuer="StarChampionshipApi" \
  -e JwtSettings__Audience="StarChampionshipUsers" \
  -e JwtSettings__ExpirationMinutes="60" \
  starchampionship-backend
```

## Limites e pontos de atencao

- o repositorio hoje deve ser tratado como backend/API, nao como sistema completo
- o frontend React vive separado e consome esta API
- a fonte de verdade para rotas finais deve ser o Swagger do ambiente em execucao
- algumas respostas ja sao JSON estruturado; outras dependem do `ModelState` ou de objetos anonimos montados no controller
