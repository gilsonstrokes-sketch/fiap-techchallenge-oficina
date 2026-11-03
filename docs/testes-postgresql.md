# Testes de persistência PostgreSQL

`PostgresRoundTripTests` valida gravação e recuperação com o provedor Npgsql e as migrations reais. Os testes usam a variável `OFICINA_TEST_POSTGRES` e são ignorados explicitamente quando ela não está configurada. Não há provedor InMemory substituindo a validação PostgreSQL.

Use uma base descartável dedicada; a fixture aplica as migrations e grava dados de teste, sem limpar a base ao final.

## Executar com Docker Compose

No PowerShell, com Docker Desktop ativo:

```powershell
$env:POSTGRES_PASSWORD = "senha-local"
docker compose up -d db
docker compose exec db createdb -U oficina oficina_test
$env:OFICINA_TEST_POSTGRES = "Host=localhost;Port=5432;Database=oficina_test;Username=oficina;Password=senha-local"
dotnet test tests/Oficina.Integration.Tests/Oficina.Integration.Tests.csproj --filter FullyQualifiedName~PostgresRoundTripTests
```

O teste também precisa de uma base compatível com a versão do schema; para recriá-la após execuções, remova e crie novamente a base `oficina_test` com `dropdb`/`createdb` dentro do container.

## Situação desta execução

O ambiente de desenvolvimento desta tarefa não tem o comando `docker` disponível. Portanto, o round trip PostgreSQL foi adicionado, mas não pôde ser executado aqui. O teste de construção de metadados EF, que não abre conexão nem substitui o round trip, foi executado separadamente.
