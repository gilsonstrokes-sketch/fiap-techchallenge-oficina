# Estratégia de testes

A suíte separa testes unitários de domínio, testes HTTP e testes de persistência. A cobertura não substitui assertions funcionais nem comprova cenários que não foram executados.

## Organização e execução

| Camada | Verificação |
|---|---|
| `Oficina.Domain.Tests` | Regras, invariantes, transições, objetos de valor e cálculos sem banco. O relatório versionado mede somente `Oficina.Domain`. |
| `Oficina.Integration.Tests` | Fluxos da aplicação/API, autenticação, OpenAPI e cenários PostgreSQL reais. |
| PostgreSQL | Npgsql e migrations da aplicação em uma base de teste dedicada configurada por `OFICINA_TEST_POSTGRES`. Sem essa variável, os testes que exigem PostgreSQL são ignorados. |

Comandos:

```powershell
dotnet build Oficina.slnx
dotnet test Oficina.slnx
```

Para executar os testes PostgreSQL, configure `OFICINA_TEST_POSTGRES` para uma base descartável, distinta de `oficina`. A fixture aplica migrations; os testes usam identificadores próprios e removem somente os registros que criaram, ou fazem rollback transacional quando aplicável. Consulte [`testes-postgresql.md`](testes-postgresql.md) para o preparo com Docker Compose.

## Cenários cobertos

- Validação de CPF/CNPJ, placa, dinheiro e quantidade.
- Transições de OS, composição/versionamento de orçamento, snapshots e histórico.
- Aprovação, reserva, início da execução, baixa de estoque e consistência transacional.
- Rollback após falha depois de `SaveChanges` e conflito de concorrência com dois `DbContext`.
- Autenticação e autorização HTTP, emissão de token, OpenAPI e esquema Bearer.
- Round trip de persistência usando PostgreSQL real.

## Cobertura e limites

O relatório [`cobertura-dominio-etapa-2.md`](relatorios/cobertura-dominio-etapa-2.md) registra 96,00% de cobertura de linhas e 85,39% de branches em `Oficina.Domain`. Esses percentuais não abrangem todas as camadas nem demonstram, por si só, a meta de 80% para todo o conjunto de domínios críticos.

Na revisão atual, a suíte contém 64 testes. `OFICINA_TEST_POSTGRES` não está configurada nesta sessão, portanto uma execução local sem essa variável não valida os cenários PostgreSQL. A execução anterior com banco real, reportada pelo grupo, precede o teste OpenAPI mais recente e não substitui a repetição da suíte atual.

Pontos sem evidência automatizada suficiente: tentativa de consulta de uma OS alheia com token de cliente, todas as combinações concorrentes de múltiplas peças e comportamento de envio por canal externo. O indicador também depende da definição de granularidade do requisito.
