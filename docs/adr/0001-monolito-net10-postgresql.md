# ADR-0001 — Monólito .NET 10 com PostgreSQL

**Status:** adotada para a arquitetura e stack do projeto em 3 de novembro de 2026. Políticas de negócio, segurança do cliente e detalhes de persistência são tratados em decisões específicas.

## Contexto
O Tech Challenge exige back-end monolítico em camadas, API REST, banco justificado, Docker e testes. A stack selecionada pelo grupo é .NET 10 e PostgreSQL.

## Decisão
Usar ASP.NET Core Web API .NET 10, PostgreSQL, EF Core 10, organização Domain/Application/Infrastructure/Api, testes xUnit e Docker Compose.

As dependências entre camadas estão descritas em `docs/arquitetura.md`.

## Justificativa
.NET 10 atende à stack definida pelo grupo. PostgreSQL fornece persistência relacional, constraints, transações e execução local em Docker. EF Core oferece migrations e integração por Npgsql.

## Consequências
- Exige atenção a mapeamento de tipos, índices, migrations, concorrência e transações.
- PostgreSQL via Docker reduz dependências de instalações locais, mas requer Docker operacional.
- O monólito facilita a entrega do MVP; módulos lógicos podem evoluir sem criar serviços distribuídos.

## Alternativas consideradas
SQL Server: viável e familiar, mas não selecionado para este trabalho. Microsserviços: não atendem à simplicidade e ao requisito explícito de monólito.
