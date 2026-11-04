# Testes de persistência PostgreSQL

Os testes PostgreSQL usam Npgsql e migrations reais. `PostgresRoundTripTests` valida gravação/leitura e reverte a transação do cenário. `PostgresCriticalFlowTests` valida aprovação com reserva, baixa ao iniciar execução, rollback depois de falha ocorrida após `SaveChanges` e conflito de concorrência com dois `DbContext`. Os testes usam `OFICINA_TEST_POSTGRES` e são ignorados explicitamente quando ela não está configurada. Não há provedor InMemory substituindo a validação PostgreSQL.

Use uma base descartável dedicada, nunca o banco principal `oficina`. A fixture aplica as migrations. Os fluxos críticos criam identificadores próprios e removem apenas seus registros em `finally`; o round trip usa rollback transacional.

## Executar com Docker Compose

No PowerShell, com Docker Desktop ativo:

```powershell
$env:POSTGRES_PASSWORD = "senha-local"
$env:AUTHENTICATION_SIGNING_KEY = "chave-local-com-pelo-menos-32-caracteres"
$env:AUTHENTICATION_ADMIN_USERNAME = "admin-local"
$env:AUTHENTICATION_ADMIN_PASSWORD = "senha-admin-local"
docker compose up -d db
$dbExists = docker compose exec -T db psql -U oficina -d oficina -tAc "SELECT 1 FROM pg_database WHERE datname = 'oficina_test'"
if ($dbExists.Trim() -ne "1") { docker compose exec db createdb -U oficina oficina_test }
$env:OFICINA_TEST_POSTGRES = "Host=localhost;Port=5432;Database=oficina_test;Username=oficina;Password=senha-local"
dotnet test tests/Oficina.Integration.Tests/Oficina.Integration.Tests.csproj --filter FullyQualifiedName~Postgres
```

O filtro executa os testes do PostgreSQL real, incluindo fluxo transacional e concorrência. A fixture aplica migrations à base configurada. Não é necessário apagar ou recriar a base entre execuções; os testes removem seus próprios dados. As quatro variáveis estão no exemplo porque o Compose valida substituições também quando apenas `db` é iniciado. Use valores locais, nunca versionados. Não execute `createdb` diretamente se a base já existe; o bloco acima verifica isso primeiro.
