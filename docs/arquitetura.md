# Arquitetura do MVP

## Objetivo e decisões

Monólito organizado em camadas, .NET 10, ASP.NET Core Web API, PostgreSQL e EF Core/Npgsql. PostgreSQL foi escolhido por ser relacional e adequado às constraints, transações e consultas deste domínio; a stack é uma decisão técnica do grupo, não imposição da FIAP. A decisão inicial está em [`adr/0001-monolito-net10-postgresql.md`](adr/0001-monolito-net10-postgresql.md).

```text
Oficina.slnx
src/
  Oficina.Api/              # Controllers, JWT, OpenAPI, composition root
  Oficina.Application/      # Casos de uso, DTOs e contratos de persistência
  Oficina.Domain/           # Entidades, objetos de valor, invariantes e transições
  Oficina.Infrastructure/   # EF Core/Npgsql, migrations, repositórios e Unit of Work
tests/
  Oficina.Domain.Tests/     # Regras de domínio e limite arquitetural
  Oficina.Integration.Tests/# HTTP, fluxos e persistência PostgreSQL real
docs/
  ddd/                       # Modelagem de domínio e diagramas Mermaid
  adr/                       # Decisões técnicas
```

## Dependências

- `Oficina.Domain` não referencia Application, Infrastructure, Api, EF Core, ASP.NET ou banco.
- `Oficina.Application` depende de Domain.
- `Oficina.Infrastructure` depende de Application e Domain e implementa as portas de persistência.
- `Oficina.Api` depende de Application e Infrastructure para composição, hospedagem e HTTP.
- Projetos de teste referenciam produção, nunca são dependência da aplicação.
- Regras e transições de negócio ficam no domínio; controllers traduzem HTTP e delegam à Application.

Os limites conceituais são descritos em [`ddd/modelagem-inicial.md`](ddd/modelagem-inicial.md). Cadastro, Catálogo, Atendimento e Estoque são contextos candidatos no monólito; não há registro de validação desses limites em sessões formais de Event Storming.

## Persistência e consistência

- Migrations versionadas em `src/Oficina.Infrastructure/Migrations/`.
- Constraints garantem CPF/CNPJ e placa únicos, vínculo OS–veículo–cliente, status permitido, valores monetários com até duas casas e quantidades inteiras válidas.
- Orçamentos guardam snapshots para não serem alterados quando o Catálogo muda.
- `OrdemDeServico` e `EstoqueDaPeca` usam concorrência otimista PostgreSQL por `xmin`; conflitos viram `ConcurrencyConflictException` e HTTP 409, sem retry automático.
- Unit of Work abre transação explícita para aprovação/reserva, substituição do orçamento e início/baixa da execução.
- A API não executa migrations ao iniciar; o schema é atualizado separadamente com EF CLI.
- Testes PostgreSQL usam uma base descartável passada em `OFICINA_TEST_POSTGRES`; não usam Testcontainers nem EF InMemory.

## API e identidade

- API REST documentada com Swagger UI e OpenAPI no ambiente `Development`; o esquema Bearer aparece apenas nas operações protegidas.
- Rotas administrativas usam Bearer JWT e role `Administrador`. Chave, usuário e senha vêm da configuração ambiente.
- Login de cliente valida documento fiscal vinculado à OS e emite token com claim de uma única OS; a consulta confere o claim com o ID solicitado.
- `/health` confirma saúde do processo, não conectividade do banco.
- A emissão de token de consulta do cliente e a negação de acesso cruzado entre OS ainda não têm cobertura de integração específica comprovada.

## Testabilidade e evidências

- Testes unitários cobrem invariantes de domínio.
- Testes PostgreSQL reais cobrem round trip, aprovação/reserva/baixa, rollback após falha injetada depois de `SaveChanges` e concorrência com dois `DbContext` para a mesma raiz de estoque.
- O teste concorrente confirma conflito para duas atualizações baseadas no mesmo `xmin`; não cobre todas as corridas de reserva multi-peça entre OS distintas.
- A cobertura publicada mede `Oficina.Domain`: 96,00% linhas e 85,39% branches, conforme `docs/relatorios/cobertura-dominio-etapa-2.md`. Não é cobertura de toda a lógica da Application ou da API.
- A análise de dependências NuGet e suas limitações estão em `docs/relatorios/analise-vulnerabilidades.md`.

## Limites de escopo

O escopo não inclui front-end, microsserviços, broker, event sourcing, CQRS/MediatR ou publicação assíncrona de eventos. Os eventos nos diagramas representam fatos de negócio e não são mensagens emitidas pelo sistema. Canal de envio de orçamento, cancelamento, devolução e reparos adicionais após o início da execução não estão implementados.
