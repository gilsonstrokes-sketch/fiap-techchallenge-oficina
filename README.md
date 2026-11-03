# FIAP SOAT — Tech Challenge Fase 1 | Pacote inicial para Codex

**Este repositório contém SOMENTE contexto, arquitetura e planejamento. Ainda não há aplicação implementada.**

## Stack decidida
.NET 10, ASP.NET Core Web API, PostgreSQL, EF Core 10, xUnit, Docker Compose. Back-end monolítico em camadas; **sem front-end nesta fase**.

## Comece aqui
1. Leia `AGENTS.md` (instruções para o Codex).
2. Leia `docs/requisitos-fiap.md` (obrigações do enunciado).
3. Leia `docs/arquitetura.md`, `docs/ddd/modelagem-inicial.md` e `docs/plano-implementacao.md`.
4. Abra esta pasta no VS Code com Codex.
5. Cole o texto de `PROMPT-INICIAL-CODEX.md` no Codex.
6. **Não gere toda a API no primeiro comando.** Revise e aprove a Etapa 0 antes de implementar.

## Estrutura futura (não criada ainda)
`src/Oficina.Api`, `src/Oficina.Application`, `src/Oficina.Domain`, `src/Oficina.Infrastructure`, `tests/Oficina.Domain.Tests`, `tests/Oficina.Integration.Tests`.

## Referência
Baseado no PDF da FIAP fornecido pelo aluno: `docs/ddd/referencias/Fase 1 - Tech Challenge.pdf`, 12 páginas. O arquivo está nessa localização (não em `docs/referencias/`). Mantenha o original disponível para consulta, sem publicá-lo inadvertidamente em repositório público (material do curso).

## Estado atual
- [x] Requisitos organizados
- [x] Arquitetura proposta
- [x] Plano incremental
- [ ] Modelagem DDD validada / Event Storming produzido
- [ ] Solução .NET criada
- [ ] API implementada
- [ ] Testes e evidências
- [ ] Entrega final
