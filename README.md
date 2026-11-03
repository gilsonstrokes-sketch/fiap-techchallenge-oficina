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

As dependências seguem `Domain <- Application <- Infrastructure <- Api`; a API referencia Application e Infrastructure somente para composição. O Domain não depende de EF Core, ASP.NET ou PostgreSQL.

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

Com o PostgreSQL iniciado por `docker compose up -d db`, configure a connection string e os dados de autenticação somente na sessão local. O perfil administrativo usa credenciais configuradas por ambiente; não há credenciais padrão versionadas.

```powershell
$env:POSTGRES_PASSWORD = "escolha-uma-senha-local"
$env:ConnectionStrings__Postgres = "Host=localhost;Port=5432;Database=oficina;Username=oficina;Password=$env:POSTGRES_PASSWORD"
$env:Authentication__SigningKey = "gere-um-segredo-aleatorio-com-pelo-menos-32-bytes"
$env:Authentication__AdminUsername = "admin-local"
$env:Authentication__AdminPassword = "defina-uma-senha-local-forte"
dotnet run --project src/Oficina.Api/Oficina.Api.csproj --urls http://localhost:8080
```

Para gerar/aplicar migrations após iniciar o banco, use `dotnet ef database update --project src/Oficina.Infrastructure/Oficina.Infrastructure.csproj --startup-project src/Oficina.Api/Oficina.Api.csproj`. Configure `ConnectionStrings__Postgres` antes de executar e não versione segredos.

## API e autenticação

Swagger/OpenAPI fica disponível em `/swagger` e `/openapi/v1.json` no ambiente `Development`. `/health` verifica o processo. Rotas administrativas exigem JWT Bearer, emitido por `POST /api/auth/admin` com credenciais de ambiente. O token tem papel `Administrator` e expira em oito horas. `POST /api/auth/cliente` valida o documento fiscal vinculado à OS e emite token limitado àquela OS, com validade de 30 minutos.

Rotas implementadas:

- `api/clientes`: listar, consultar, cadastrar, alterar nome, inativar e reativar.
- `api/veiculos`: listar (opcionalmente por cliente), consultar, cadastrar, alterar e inativar/reativar.
- `api/catalogo/servicos` e `api/catalogo/pecas`: CRUD com inativação/reativação; `api/estoque/pecas/{id}` consulta saldo e `POST api/estoque/pecas/{id}/entradas` registra entrada.
- `api/ordens-de-servico`: abrir OS informando CPF/CNPJ do cliente e veículo, consultar, iniciar diagnóstico, criar/substituir orçamento, aprovar/rejeitar, iniciar/finalizar execução e entregar veículo.
- `api/indicadores/tempo-medio-execucao`: média de execução em horas.
- `api/consulta/ordens-de-servico/{id}`: consulta limitada pelo claim da OS do token de cliente.

A aprovação registra a decisão mesmo sem estoque disponível. Se todas as peças estão disponíveis, a Application cria reservas na mesma transação; caso falte estoque, o início tenta reservar novamente e permanece bloqueado sem remover a aprovação. Na execução, as reservas são verificadas no estado persistido, a confirmação tipada é criada internamente e a baixa ocorre na transação junto à transição da OS. Conflitos de concorrência retornam HTTP 409, sem retry automático.

## Build e testes

```powershell
dotnet restore Oficina.slnx
dotnet build Oficina.slnx --no-restore
dotnet test Oficina.slnx --no-build
```

Testes unitários de Domain cobrem invariantes e transições; testes de Application usam repositórios fake para validar coordenação de aprovação, reserva, baixa e vínculo cliente/veículo. Testes HTTP verificam health, OpenAPI e JWT. O round trip real depende de PostgreSQL e é ignorado se `OFICINA_TEST_POSTGRES` não estiver configurada; EF InMemory não é usado como substituto. Para executá-lo, inicie o Compose e defina `OFICINA_TEST_POSTGRES` com uma connection string de teste antes de `dotnet test`.

A cobertura medida de `Oficina.Domain` é 89,28% de linhas e 81,05% de branches; relatório Cobertura está em `artifacts/coverage/oficina-domain.cobertura.xml`. Para reproduzir, rode `dotnet test Oficina.slnx --collect:"XPlat Code Coverage"` e confira o relatório do pacote `Oficina.Domain`.

Neste ambiente sem Docker, não foram executados round trips PostgreSQL, aplicação da migration nem testes de concorrência real. Mapeamentos e transações ainda precisam ser verificados contra PostgreSQL.

## Decisões e documentação

- Requisitos oficiais transcritos: `docs/requisitos-fiap.md`.
- Arquitetura e dependências: `docs/arquitetura.md`.
- Propostas e pendências de negócio: `docs/decisoes-negocio.md`.
- Modelagem DDD e planejamento dos Event Stormings: `docs/ddd/`.
- Estratégia de testes e cobertura: `docs/estrategia-testes.md`.
- Plano incremental: `docs/plano-implementacao.md`.

O PDF original da FIAP está em `docs/ddd/referencias/`. Antes de publicar o repositório, confirme que a inclusão desse material do curso é permitida.
