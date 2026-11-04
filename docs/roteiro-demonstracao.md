# Visão geral da demonstração — Fase 1

Este roteiro resume a apresentação do produto. O [roteiro operacional passo a passo](roteiro-gravacao-passo-a-passo.md) contém a sequência detalhada de navegação, corpos de requisição e respostas esperadas. A duração planejada é de aproximadamente 10 a 12 minutos, dentro do limite de 15 minutos indicado pela FIAP.

## Sequência sugerida

| Parte | Evidência apresentada |
|---|---|
| Repositório e arquitetura | Branch e versão, camadas Api/Application/Domain/Infrastructure e justificativa de PostgreSQL. |
| Execução local | Docker Compose, API, PostgreSQL, `/health`, Swagger e OpenAPI. |
| Autenticação | JWT administrativo, endpoint protegido sem token e chamada autorizada pelo Swagger. Credenciais e token ficam ocultos. |
| Cadastros | Criação e consulta de cliente, veículo, serviço e peça; entrada e consulta de estoque. |
| Ordem e orçamento | Criação da OS, diagnóstico, orçamento versionado, snapshots e decisão administrativa. |
| Execução e entrega | Reserva, baixa de estoque, finalização e entrega da OS. |
| Evidências técnicas | Testes unitários e PostgreSQL, rollback, concorrência, documentação DDD, diagramas e riscos de segurança. |

## Precisões sobre o estado atual

- A aprovação e a rejeição do orçamento são operações administrativas; não existe endpoint de decisão do cliente.
- O token de consulta do cliente restringe uma OS, mas não há evidência automatizada de teste de acesso cruzado.
- Os diagramas Mermaid representam fluxos do código. Eles não comprovam a realização de workshops de Event Storming.
- A cobertura registrada mede somente `Oficina.Domain`; a análise de CVEs da imagem e a varredura dedicada de segredos estão pendentes.
- O roteiro detalhado deve ser executado com dados fictícios e uma base de demonstração; não se deve mostrar credenciais ou tokens.
