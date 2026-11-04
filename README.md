# Tech Challenge FIAP SOAT — Fase 1

API REST para gestão de uma oficina mecânica, desenvolvida em .NET 10, C#, ASP.NET Core, EF Core e PostgreSQL. A solução cobre cadastros, ordens de serviço, orçamentos, estoque, autenticação e acompanhamento. A Fase 1 não inclui interface web.

## Arquitetura

O sistema é um monólito organizado em camadas:

| Projeto | Responsabilidade |
|---|---|
| `Oficina.Api` | Endpoints REST, autenticação, OpenAPI e composição da aplicação |
| `Oficina.Application` | Casos de uso, coordenação de fluxos e contratos de persistência |
| `Oficina.Domain` | Agregados, entidades, objetos de valor, invariantes e transições |
| `Oficina.Infrastructure` | EF Core, Npgsql, repositórios, transações e migrations |

O projeto `Domain` não depende de ASP.NET Core, EF Core ou banco de dados. PostgreSQL foi escolhido pelo modelo relacional, constraints e suporte a transações. A concorrência otimista utiliza `xmin`; conflitos são respondidos com HTTP 409. Mais detalhes estão em [`docs/arquitetura.md`](docs/arquitetura.md), [`docs/ddd/modelagem-inicial.md`](docs/ddd/modelagem-inicial.md) e [`docs/adr/0001-monolito-net10-postgresql.md`](docs/adr/0001-monolito-net10-postgresql.md).

## Pré-requisitos

- Docker Desktop com Docker Compose v2; ou .NET 10 SDK e PostgreSQL.
- PowerShell para os comandos deste README.

## Executar com Docker Compose

O Compose exige quatro variáveis locais. Defina-as no ambiente ou em um arquivo `.env` ignorado pelo Git. Use valores próprios; a chave de assinatura JWT deve ter pelo menos 32 bytes UTF-8.

```powershell
$env:POSTGRES_PASSWORD = '<senha local do PostgreSQL>'
$env:AUTHENTICATION_SIGNING_KEY = '<chave local com pelo menos 32 bytes>'
$env:AUTHENTICATION_ADMIN_USERNAME = '<usuário administrador>'
$env:AUTHENTICATION_ADMIN_PASSWORD = '<senha do administrador>'
docker compose up --build -d
docker compose ps
```

A API e o PostgreSQL ficam publicados em loopback: `http://localhost:8080` e `localhost:5432`. O banco da aplicação chama-se `oficina`. O perfil Compose usa `Development`, que habilita Swagger e OpenAPI. O endpoint `/health` verifica o processo da API; não verifica a conexão com o banco.

- Health: `GET http://localhost:8080/health`
- Swagger UI: `http://localhost:8080/swagger`
- OpenAPI: `http://localhost:8080/openapi/v1.json`

## Migrations

As migrations ficam em `src/Oficina.Infrastructure/Migrations/`. A API não aplica migrations ao iniciar. Instale a ferramenta EF CLI compatível com o EF Core 10 e configure a connection string local antes de atualizar o schema:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.14
$env:ConnectionStrings__Postgres = "Host=localhost;Port=5432;Database=oficina;Username=oficina;Password=$env:POSTGRES_PASSWORD"
$env:Authentication__SigningKey = '<chave local com pelo menos 32 bytes>'
dotnet ef database update --project src/Oficina.Infrastructure --startup-project src/Oficina.Api
```

## Autenticação e API

`POST /api/auth/admin` emite o JWT administrativo. No Swagger, clique **Authorize** e informe somente o token; a interface adiciona o prefixo Bearer. O token tem papel `Administrador`. `POST /api/auth/cliente` emite um token de consulta limitado a uma OS; a consulta confere o ID da rota com o claim `ordem_id`.

| Recurso | Rotas principais | Acesso |
|---|---|---|
| Autenticação | `POST /api/auth/admin`, `POST /api/auth/cliente` | Público; credenciais/documento validados |
| Clientes | `GET/POST /api/clientes`, `GET/PUT/DELETE /api/clientes/{id}`, `POST /api/clientes/{id}/reativacao` | Administrador |
| Veículos | `GET/POST /api/veiculos`, `GET/PUT/DELETE /api/veiculos/{id}`, `POST /api/veiculos/{id}/reativacao` | Administrador |
| Serviços | `/api/servicos` e `/api/servicos/{id}`; reativação em `/{id}/reativacao` | Administrador |
| Peças | `/api/pecas` e `/api/pecas/{id}`; reativação em `/{id}/reativacao` | Administrador |
| Estoque | `GET /api/estoque/pecas/{pecaId}`, `POST /api/estoque/pecas/{pecaId}/entradas` | Administrador |
| Ordens de serviço | `/api/ordens-de-servico`, `/{id}`, `/{id}/diagnostico`, `/{id}/orcamentos`, `/{id}/orcamentos/aprovacao`, `/{id}/orcamentos/rejeicao`, `/{id}/orcamentos/substituicao`, `/{id}/execucao/inicio`, `/{id}/execucao/finalizacao`, `/{id}/entrega` | Administrador |
| Consulta de cliente | `GET /api/consulta/ordens-de-servico/{id}` | JWT de consulta restrito à OS |
| Indicador | `GET /api/indicadores/tempo-medio-execucao` | Administrador |

As decisões de orçamento estão protegidas como operações administrativas. A API não oferece endpoint de aprovação/rejeição autenticado como cliente. A documentação completa dos contratos está no Swagger/OpenAPI.

As configurações são fornecidas por `ConnectionStrings__Postgres`, `Authentication__SigningKey`, `Authentication__AdminUsername` e `Authentication__AdminPassword`. Nenhum segredo real deve ser mantido no repositório.

## Build e testes

```powershell
dotnet build Oficina.slnx
dotnet test Oficina.slnx
```

Os testes unitários e HTTP são executados sem configuração adicional. Os testes PostgreSQL reais usam `OFICINA_TEST_POSTGRES`; sem essa variável, os testes marcados para PostgreSQL são ignorados. Use uma base dedicada chamada `oficina_test`, distinta do banco principal `oficina`:

```powershell
# O Compose valida as quatro variáveis mesmo ao iniciar somente o serviço db.
$env:POSTGRES_PASSWORD = '<senha local do PostgreSQL>'
$env:AUTHENTICATION_SIGNING_KEY = '<chave local com pelo menos 32 bytes>'
$env:AUTHENTICATION_ADMIN_USERNAME = '<usuário administrador>'
$env:AUTHENTICATION_ADMIN_PASSWORD = '<senha do administrador>'
docker compose up -d db
$dbExists = docker compose exec -T db psql -U oficina -d oficina -tAc "SELECT 1 FROM pg_database WHERE datname = 'oficina_test'"
if ($dbExists.Trim() -ne '1') { docker compose exec db createdb -U oficina oficina_test }
$env:OFICINA_TEST_POSTGRES = "Host=localhost;Port=5432;Database=oficina_test;Username=oficina;Password=$env:POSTGRES_PASSWORD"
dotnet test Oficina.slnx
```

A fixture aplica migrations à base configurada. Os cenários PostgreSQL removem somente os registros que criam ou revertem suas operações em transação. A cobertura disponível mede `Oficina.Domain`: 96,00% das linhas e 85,39% dos branches, conforme [`docs/relatorios/cobertura-dominio-etapa-2.md`](docs/relatorios/cobertura-dominio-etapa-2.md); não representa cobertura de todas as camadas.

## Documentação

- Requisitos, decisões e rastreabilidade: [`docs/requisitos-fiap.md`](docs/requisitos-fiap.md), [`docs/decisoes-negocio.md`](docs/decisoes-negocio.md), [`docs/matriz-rastreabilidade.md`](docs/matriz-rastreabilidade.md).
- Modelagem DDD e diagramas: [`docs/ddd/modelagem-inicial.md`](docs/ddd/modelagem-inicial.md), [`docs/ddd/event-storming.md`](docs/ddd/event-storming.md).
- Testes e segurança: [`docs/testes-postgresql.md`](docs/testes-postgresql.md), [`docs/relatorios/analise-vulnerabilidades.md`](docs/relatorios/analise-vulnerabilidades.md).
- Entrega: [`docs/checklist-entrega.md`](docs/checklist-entrega.md), [`docs/roteiro-gravacao-passo-a-passo.md`](docs/roteiro-gravacao-passo-a-passo.md).
