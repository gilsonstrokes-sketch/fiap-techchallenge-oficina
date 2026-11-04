# Etapas de implementação — Fase 1

O desenvolvimento foi organizado em etapas para validar decisões de domínio e persistência antes da ampliação dos fluxos. A sequência abaixo registra o escopo e os critérios de aceite definidos para cada etapa.

| Etapa | Escopo | Critério de aceite |
|---|---|---|
| 0 — Contexto e decisões | Requisitos, arquitetura, linguagem de domínio, decisões pendentes e estratégia de testes | Requisitos rastreados e decisões de negócio distinguidas das exigências da FIAP |
| 1 — Estrutura da solução | Projetos Api, Application, Domain e Infrastructure, DI, OpenAPI, Docker Compose e persistência inicial | Build, API e Swagger executáveis |
| 2 — Domínio e cadastros | Objetos de valor, entidades de cadastro, catálogo, persistência e testes | Cadastros e validações cobertos por testes |
| 3 — OS e orçamento | Criação de OS, diagnóstico, snapshots, versões de orçamento, decisões e transações | Fluxo de OS e orçamento persistido e demonstrável pela API |
| 4 — Estoque e segurança | Transições, reserva/baixa, concorrência, JWT, consulta de cliente e indicador | Fluxos críticos protegidos, transacionais e testados com PostgreSQL |
| 5 — Qualidade e entrega | Cobertura, análise de dependências, documentação DDD, vídeo e PDF | Evidências técnicas e materiais de entrega consistentes com a versão publicada |

Os critérios de entrega e o estado das evidências são mantidos em [`checklist-entrega.md`](checklist-entrega.md) e [`matriz-rastreabilidade.md`](matriz-rastreabilidade.md).
