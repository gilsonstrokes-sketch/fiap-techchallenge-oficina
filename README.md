# FIAP SOAT — Tech Challenge Fase 1

MVP de back-end monolítico para uma oficina mecânica, construído incrementalmente com .NET 10, ASP.NET Core, PostgreSQL e EF Core 10.

## Estrutura

```text
src/
  Oficina.Api/            # ASP.NET Core, OpenAPI/Swagger e health check
  Oficina.Application/    # Casos de uso e coordenação da aplicação
  Oficina.Domain/         # Regras e tipos de domínio, sem dependências externas
  Oficina.Infrastructure/ # EF Core, Npgsql e acesso a dados
tests/
  Oficina.Domain.Tests/       # Testes de unidade e limites arquiteturais
  Oficina.Integration.Tests/  # Testes HTTP da API
```

As dependências seguem `Domain <- Application <- Infrastructure <- Api`; a API referencia Application e Infrastructure somente para composição. Nesta etapa não há regras de negócio, autenticação, migrations ou endpoints de OS.

## Pré-requisitos

- .NET SDK 10.0
- Docker com Docker Compose v2, para execução completa

## Executar com Docker Compose

No PowerShell, defina uma senha local para o PostgreSQL e suba os serviços:

```powershell
$env:POSTGRES_PASSWORD = "escolha-uma-senha-local"
docker compose up --build
```

O Compose inicia PostgreSQL e API. A senha vem do ambiente e não fica armazenada no repositório. Para encerrar, use `Ctrl+C`; para remover os containers mantendo os dados, execute `docker compose down`. Para apagar também o volume local de dados, use `docker compose down --volumes`.

- API: <http://localhost:8080>
- Health check da API: <http://localhost:8080/health>
- Swagger UI (ambiente Development): <http://localhost:8080/swagger>
- Documento OpenAPI: <http://localhost:8080/openapi/v1.json>
- PostgreSQL local: `localhost:5432`, banco/usuário `oficina`

O endpoint `/health` informa a saúde do processo da API; nesta etapa ele não verifica conectividade com o banco.

## Executar a API pelo SDK

Com o PostgreSQL iniciado por `docker compose up -d db`, configure a connection string apenas na sessão local e inicie a API:

```powershell
$env:POSTGRES_PASSWORD = "escolha-uma-senha-local"
$env:ConnectionStrings__Postgres = "Host=localhost;Port=5432;Database=oficina;Username=oficina;Password=$env:POSTGRES_PASSWORD"
dotnet run --project src/Oficina.Api/Oficina.Api.csproj --urls http://localhost:8080
```

## Build e testes

```powershell
dotnet restore Oficina.slnx
dotnet build Oficina.slnx --no-restore
dotnet test Oficina.slnx --no-build
```

Os testes iniciais verificam que Domain não referencia camadas externas e que `/health` e o documento OpenAPI respondem. Eles não substituem os testes de domínio e integração dos fluxos críticos, que serão acrescentados nas próximas etapas.

## Decisões e documentação

- Requisitos oficiais transcritos: `docs/requisitos-fiap.md`.
- Arquitetura e dependências: `docs/arquitetura.md`.
- Propostas e pendências de negócio: `docs/decisoes-negocio.md`.
- Modelagem DDD e planejamento dos Event Stormings: `docs/ddd/`.
- Estratégia de testes e cobertura: `docs/estrategia-testes.md`.
- Plano incremental: `docs/plano-implementacao.md`.

O PDF original da FIAP está em `docs/ddd/referencias/`. Antes de publicar o repositório, confirme que a inclusão desse material do curso é permitida.
