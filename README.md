# Tech Challenge FIAP SOAT — Fase 1

API REST para gestão de uma oficina mecânica, implementada em .NET 10, C# e PostgreSQL. O escopo inclui cadastros, ordens de serviço, orçamentos, decisões, acompanhamento, estoque, autenticação e indicador de execução. Não há frontend nesta fase.

> Antes de publicar ou gravar uma demonstração, confirme os itens marcados como parciais ou pendentes em [`docs/matriz-rastreabilidade.md`](docs/matriz-rastreabilidade.md) e [`docs/checklist-entrega.md`](docs/checklist-entrega.md).

## Arquitetura

Monólito organizado em camadas: `Api` trata HTTP e autenticação; `Application` orquestra casos de uso; `Domain` concentra entidades, objetos de valor e invariantes sem dependência de infraestrutura; `Infrastructure` implementa persistência EF Core/Npgsql e migrations. Os limites e as decisões são descritos em [`docs/arquitetura.md`](docs/arquitetura.md), [`docs/ddd/modelagem-inicial.md`](docs/ddd/modelagem-inicial.md) e [`docs/adr/`](docs/adr/).

PostgreSQL foi escolhido pela persistência relacional, constraints e suporte transacional, com concorrência otimista por `xmin` no estoque. A aplicação não executa migrations automaticamente ao iniciar: aplique-as explicitamente antes de usar uma base nova.

## Pré-requisitos

- Docker Desktop com Compose, ou .NET 10 SDK e PostgreSQL compatível.
- PowerShell nos exemplos abaixo (adapte a sintaxe para seu shell).

## Executar com Docker Compose

Defina segredos locais no ambiente ou em um `.env` não versionado. `.env` é ignorado pelo Git. Use valores locais fortes; a chave JWT precisa ter pelo menos 32 bytes UTF-8.

```powershell
$env:POSTGRES_PASSWORD = "<senha-local>"
$env:AUTHENTICATION_SIGNING_KEY = "<chave-local-com-pelo-menos-32-caracteres>"
$env:AUTHENTICATION_ADMIN_USERNAME = "<usuario-admin>"
$env:AUTHENTICATION_ADMIN_PASSWORD = "<senha-admin>"
docker compose up --build -d
docker compose ps
```

O Compose publica API e PostgreSQL somente em `127.0.0.1` (`8080` e `5432`). O banco de execução tem nome `oficina`. Não use `docker compose down -v` em ambiente com dados que devam ser preservados.

Verifique `http://localhost:8080/health`, Swagger em `http://localhost:8080/swagger` e o documento OpenAPI em `http://localhost:8080/openapi/v1.json`. Swagger/OpenAPI é exposto no ambiente `Development` configurado pelo Compose. O health check atual mede a disponibilidade do processo, não é uma prova de prontidão do PostgreSQL.

## Migrations

As migrations versionadas ficam em `src/Oficina.Infrastructure/Migrations/`. Para criar/aplicar migrations via EF CLI, use a solution e informe `Infrastructure` como projeto de migrations e `Api` como startup, com a connection string configurada localmente. Exemplo para aplicar migrations em uma base já iniciada pelo Compose:

```powershell
dotnet ef database update --project src/Oficina.Infrastructure --startup-project src/Oficina.Api
```

Confirme a connection string antes do comando; não a grave no repositório. Em Compose, a API aguarda o health check do banco, mas não altera o schema sozinha.

## Autenticação e endpoints

Solicite token administrativo em `POST /api/auth/admin`. As rotas administrativas exigem JWT com papel `Administrador`; sem token retornam `401`. O endpoint `POST /api/auth/cliente` emite um token de consulta vinculado a uma OS e documento fiscal. `GET /api/consulta/ordens-de-servico/{id}` valida o ID solicitado contra o claim `ordem_id` do token.

| Área | Endpoints principais | Acesso |
|---|---|---|
| Autenticação | `POST /api/auth/admin`, `POST /api/auth/cliente` | Público; credenciais/documento validados |
| Clientes | `/api/clientes` CRUD | Administrador |
| Veículos | `/api/veiculos` CRUD | Administrador |
| Serviços e peças | `/api/servicos`, `/api/pecas` CRUD | Administrador |
| Estoque | `GET /api/estoque/pecas/{pecaId}`, `POST /api/estoque/pecas/{pecaId}/entradas` | Administrador |
| Ordens de serviço | `/api/ordens-de-servico`, detalhes, diagnóstico, orçamento, aprovação/rejeição/substituição, início/finalização/entrega | Administrador |
| Consulta de cliente | `GET /api/consulta/ordens-de-servico/{id}` | JWT de consulta restrito à OS do claim |
| Indicador | `GET /api/indicadores/tempo-medio-execucao` | Administrador |

O código atual registra aprovação e rejeição nas rotas administrativas. Não há endpoint de decisão autenticado como cliente; essa diferença em relação ao ator esperado precisa ser validada pelo grupo e está marcada como lacuna, não deve ser ocultada na demonstração. Veja as rotas e schemas exatos no OpenAPI.

Configuração por variáveis de ambiente: `ConnectionStrings__Postgres`, `Authentication__SigningKey`, `Authentication__AdminUsername` e `Authentication__AdminPassword`. Não coloque tokens, senhas reais ou connection strings com credenciais em arquivos versionados, exemplos ou commits.

## Testes

```powershell
dotnet build Oficina.slnx
dotnet test Oficina.slnx
```

A suíte tem testes unitários e de integração HTTP. Os testes que validam PostgreSQL real usam a variável `OFICINA_TEST_POSTGRES`, aplicam migrations e são ignorados quando ela não está configurada. Use uma base dedicada descartável, como `oficina_test`; nunca a base principal `oficina`:

```powershell
$env:POSTGRES_PASSWORD = "<senha-local>"
$env:AUTHENTICATION_SIGNING_KEY = "<chave-local-com-pelo-menos-32-caracteres>"
$env:AUTHENTICATION_ADMIN_USERNAME = "<usuario-admin>"
$env:AUTHENTICATION_ADMIN_PASSWORD = "<senha-admin>"
docker compose up -d db
docker compose exec db createdb -U oficina oficina_test
$env:OFICINA_TEST_POSTGRES = "Host=localhost;Port=5432;Database=oficina_test;Username=oficina;Password=$env:POSTGRES_PASSWORD"
dotnet test tests/Oficina.Integration.Tests/Oficina.Integration.Tests.csproj
```

Se `oficina_test` já existir, não execute `createdb` novamente. Consulte [`docs/testes-postgresql.md`](docs/testes-postgresql.md) para o escopo dos cenários. No último resultado reportado pelo grupo, a suíte completa teve 63 aprovados; repita os comandos no commit final para produzir evidência associada a esse commit.

O relatório versionado [`docs/relatorios/cobertura-dominio-etapa-2.md`](docs/relatorios/cobertura-dominio-etapa-2.md) informa 96,00% de linhas e 85,39% de branches para o projeto `Domain` no escopo descrito. Isso não mede a cobertura completa de Application/Infrastructure nem comprova por si só todos os fluxos críticos.

## Documentação e entrega

- Requisitos e rastreabilidade: [`docs/requisitos-fiap.md`](docs/requisitos-fiap.md), [`docs/matriz-rastreabilidade.md`](docs/matriz-rastreabilidade.md).
- DDD e fluxos Mermaid: [`docs/ddd/modelagem-inicial.md`](docs/ddd/modelagem-inicial.md), [`docs/ddd/event-storming.md`](docs/ddd/event-storming.md).
- Segurança: [`docs/relatorios/analise-vulnerabilidades.md`](docs/relatorios/analise-vulnerabilidades.md).
- Demonstração e checklist: [`docs/roteiro-demonstracao.md`](docs/roteiro-demonstracao.md), [`docs/checklist-entrega.md`](docs/checklist-entrega.md).

O repositório contém documentação técnica; PDF final, links públicos dos boards/exports do grupo e vídeo público precisam ser preenchidos e conferidos pelo grupo conforme o checklist antes da entrega.
