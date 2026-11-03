# ADR-0001 — Monólito .NET 10 com PostgreSQL

**Status:** aceita pelo usuário para arquitetura e stack do projeto em 2026-11-03. Políticas de negócio, segurança do cliente e detalhes de persistência ainda requerem validação; não são aprovados por este ADR.

## Contexto
O Tech Challenge exige back-end monolítico em camadas, API REST, banco justificado, Docker e testes. O aluno tem familiaridade com .NET 10 e escolheu PostgreSQL.

## Decisão
Usar ASP.NET Core Web API .NET 10, PostgreSQL, EF Core 10, organização Domain/Application/Infrastructure/Api, testes xUnit e Docker Compose.

O usuário aprovou esta arquitetura para o projeto. A estrutura e as dependências detalhadas estão em `docs/arquitetura.md`. A aprovação da arquitetura não autoriza implementação além da etapa incremental explicitamente solicitada.

## Justificativa
Stack familiar para o desenvolvimento; PostgreSQL é relacional, open source e simples de executar em Docker. EF Core suporta migrations e testes de integração com provedor PostgreSQL.

## Consequências
- Exige atenção a mapeamento de tipos, índices, migrations, concorrência e transações.
- PostgreSQL via Docker reduz dependências de instalações locais, mas requer Docker operacional.
- O monólito facilita a entrega do MVP; módulos lógicos podem evoluir sem criar serviços distribuídos.

## Alternativas consideradas
SQL Server: viável e familiar, mas não selecionado para este trabalho. Microsserviços: não atendem à simplicidade e ao requisito explícito de monólito.
