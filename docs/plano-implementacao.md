# Plano incremental — não executar etapas futuras sem autorização

## Etapa 0 — Contexto e decisões (PR 0)
**Objetivo:** validar requisitos, estrutura e riscos; ainda **sem endpoints de negócio**.
- Conferir PDF original (12 páginas; armazenado em `docs/ddd/referencias/`, não em `docs/referencias/`) com `docs/requisitos-fiap.md`.
- Registrar matriz requisito → caso de uso → teste → evidência em `docs/matriz-rastreabilidade.md`.
- Registrar propostas e dúvidas de negócio em `docs/decisoes-negocio.md`; não declarar propostas iniciais como regras FIAP ou decisões aprovadas.
- Preparar comandos/eventos/agregados e rascunho de transições em `docs/ddd/modelagem-inicial.md`; planejar as três sessões obrigatórias em `docs/ddd/plano-event-storming.md`.
- Confirmar stack e dependências de projetos; revisar ADR de arquitetura/banco.
- Delimitar cobertura dos domínios críticos e camadas de teste em `docs/estrategia-testes.md`.
**Aceite:** fonte oficial conferida; documentação de rastreabilidade, modelagem, sessões, dependências e testes preparada; pendências de negócio visíveis; sem código de aplicação ou regra inventada.

## Etapa 1 — Bootstrap (PR 1)
- Criar `.slnx` e projetos Api/Application/Domain/Infrastructure/testes.
- Configurar DI, OpenAPI, healthcheck, PostgreSQL Compose, Dockerfile, EF Core e migrations iniciais quando modelo validado.
- Pipeline de build/teste e README inicial.
**Aceite:** `dotnet build`, testes de exemplo e `docker compose up --build` funcionando; Swagger acessível; sem lógica de OS.

## Etapa 2 — Domínio e cadastros (PR 2)
- Validar VOs CPF/CNPJ e placa; modelar cliente, veículo, serviço, peça/insumo.
- CRUDs, persistência, índices, validações e testes unitários/integração.
**Aceite:** endpoints e testes para cada CRUD; erros coerentes; dados persistidos.

## Etapa 3 — Ordem de Serviço e orçamento (PR 3)
- Criar OS, vincular cliente/veículo, adicionar serviços e peças, calcular orçamento com snapshots.
- Disponibilizar orçamento e registrar aprovação/rejeição, conforme regras aprovadas.
- Transações e testes de fluxos completos.
**Aceite:** cenário ponta a ponta testado e demonstrável por OpenAPI.

## Etapa 4 — Status, estoque, segurança e indicadores (PR 4)
- Transições automáticas autorizadas e histórico; consulta segura do cliente.
- Política aprovada de estoque, concorrência e transações.
- JWT administrativo e tempo médio de execução.
**Aceite:** transições inválidas bloqueadas; estoque consistente sob concorrência; APIs protegidas.

## Etapa 5 — Qualidade e entrega (PR 5)
- Cobertura >=80% nos domínios críticos com relatório; integração; revisão de segurança e análise de vulnerabilidades.
- Completar Event Storming/diagramas/linguagem ubíqua, ADRs e README.
- Verificar GitHub público, `main` protegida, PRs, branch/hash, vídeo YouTube público <=15 min e PDF final de entrega.
**Aceite:** checklist de `docs/checklist-entrega.md` completo, links acessíveis e evidências consistentes com commit final.

## Definição de pronto por PR
Build limpo; testes relevantes executados; sem segredos; documentação sincronizada; requisitos/critério de aceite verificados; review; PR para `main` protegida.
