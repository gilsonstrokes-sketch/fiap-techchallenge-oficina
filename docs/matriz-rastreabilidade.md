# Matriz de rastreabilidade — Fase 1

Fonte: `docs/requisitos-fiap.md`, conferido contra o PDF original em `docs/ddd/referencias/Fase 1 - Tech Challenge.pdf` (12 páginas; requisitos e entregáveis nas páginas 2–11). Os casos de uso, testes e evidências abaixo são planejamento, não funcionalidades já implementadas.

## Requisitos funcionais

| ID | Requisito | Caso de uso / área prevista | Testes planejados | Evidência de entrega |
|---|---|---|---|---|
| RF01 | Identificar cliente por CPF/CNPJ ao criar OS | Criar OS / Cadastro | Unitário de DocumentoFiscal e integração da criação | OpenAPI, execução demonstrada e teste |
| RF02 | CRUD de clientes | Cadastro | Unitários de regras e integração CRUD/validações | OpenAPI, persistência e testes |
| RF03 | CRUD de veículos com placa, marca, modelo e ano | Cadastro | Unitário de Placa e integração CRUD | OpenAPI, persistência e testes |
| RF04 | CRUD de serviços e inclusão dos solicitados na OS | Catálogo / Atendimento | Unitário de regras de itens e integração catálogo–OS | OpenAPI, cenário de OS e testes |
| RF05 | CRUD de peças/insumos e controle de estoque | Catálogo e Estoque | Unitário de movimentos/invariantes e integração concorrente | OpenAPI, histórico/saldo e testes de estoque |
| RF06 | Incluir peças/insumos necessários na OS | Atendimento / Estoque | Unitário de composição e integração de disponibilidade | OpenAPI e cenário demonstrado |
| RF07 | Gerar orçamento automaticamente com serviços e peças | Atendimento / Orçamento | Unitário de cálculo/snapshots e integração do fluxo | Orçamento exibido na API e testes |
| RF08 | Enviar ou disponibilizar orçamento para aprovação | Atendimento / Orçamento | Unitário de estado e integração de disponibilização | OpenAPI e demonstração; sem provedor de envio exigido |
| RF09 | Registrar aprovação ou rejeição do orçamento | Atendimento / Orçamento | Unitários de decisão/revisão e integração do fluxo | Histórico de decisões e testes |
| RF10 | Manter os seis status da OS | Atendimento / OS | Teste unitário de cada estado | Histórico/status na API e demonstração |
| RF11 | Controlar transições conforme ações/regras | Atendimento / OS | Testes unitários de transições válidas e inválidas | Regras documentadas, respostas da API e testes |
| RF12 | Cliente consulta andamento via API | Identidade / Consulta de OS | Integração de autorização, escopo e tentativas de acesso cruzado | OpenAPI e demonstração segura |
| RF13 | Listar e detalhar OS | Atendimento / Consulta administrativa | Integração de filtros, detalhe e autorização | OpenAPI e demonstração administrativa |
| RF14 | Monitorar ou calcular tempo médio dos serviços | Indicadores | Unitário do cálculo e integração da consulta/agregação | Resultado reproduzível, regra documentada e teste |

## Requisitos técnicos e entregáveis

| ID | Requisito | Planejamento / validação futura | Evidência esperada |
|---|---|---|---|
| RT01 | Back-end monolítico em camadas | Estrutura Api/Application/Domain/Infrastructure | Diagrama e dependências verificáveis no código |
| RT02 | Banco livre, com justificativa | PostgreSQL aprovado; justificar no ADR | ADR e README |
| RT03 | API REST documentada | OpenAPI/Swagger | Interface/documento publicado e exemplos |
| RT04 | JWT nas APIs administrativas | Perfil administrativo único no MVP | Testes de acesso autenticado e negado |
| RT05 | Validar CPF/CNPJ e placa | Objetos de valor no domínio | Testes de borda e erros documentados |
| RT06 | Testes unitários e integração dos fluxos principais | Estratégia em `docs/estrategia-testes.md` | Execução automatizada e resultados |
| RT07 | Cobertura mínima de 80% nos domínios críticos | Escopo e métrica definidos em `docs/estrategia-testes.md` | Relatório versionado/artefato de CI |
| RT08 | Dockerfile e Compose para ambiente completo | Previsto em etapa futura | Execução documentada; fora da Etapa 0 |
| RT09 | Execução local simples e README | Requisitos de conteúdo registrados no requisito oficial | README e execução reproduzível |
| RT10 | Transações nos fluxos principais | Orçamento/estoque/OS com concorrência definida antes de implementar | Testes de integração e desenho de persistência |
| RT11 | Análise de vulnerabilidades e resultado no repositório | Definir ferramentas e momento da análise na etapa de qualidade | Relatório/resultado reproduzível |

## Evidências transversais da entrega

| Evidência FIAP | Critério verificável |
|---|---|
| PDF único | Curso SOAT, fase, grupo, integrantes, RMs, Discord, linguagem/tecnologias e links completos |
| Repositório | Público durante avaliação, sem segredos, versão por branch/tag/hash; `main` protegida e PRs |
| Vídeo | Público no YouTube, até 15 min, demonstra solução real, arquitetura, Event Storming, DDD, fluxos, auth, API, testes e análise de vulnerabilidades |
| DDD | Link público sem concessão manual; exports versionados em `docs/ddd/`; documentação consistente com o código entregue |
| README | Objetivo, solução, arquitetura, stack, pré-requisitos, execução/testes, Swagger, justificativa do banco, autenticação, diretórios, links DDD e vídeo |

## Lacunas de rastreabilidade

- Os casos de uso acima são nomes de planejamento; detalhes ainda dependem das decisões de `docs/decisoes-negocio.md` e Event Storming.
- O requisito de cobertura não define o que significa “domínios críticos”. Este plano delimita o escopo medido na estratégia de testes e mantém a métrica como evidência, não como garantia de correção.
- A FIAP pede autorização de reparos adicionais via aplicativo como contexto do desafio. Como o escopo entregue é back-end, confirmar se isso será coberto pelo mesmo fluxo de revisão/aprovação de orçamento ou se o grupo precisa demonstrar uma operação distinta.
