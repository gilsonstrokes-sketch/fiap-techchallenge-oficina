# Instruções para o Codex — FIAP SOAT Fase 1

## Missão
Construir **incrementalmente** o MVP de back-end para uma oficina mecânica, conforme `docs/requisitos-fiap.md`. Stack decidida: **.NET 10 / C# / ASP.NET Core Web API / PostgreSQL / EF Core 10**. A documentação da FIAP é a fonte dos requisitos; decisões propostas estão identificadas como propostas.

## Prioridades e ordem de leitura
1. `docs/requisitos-fiap.md` — requisitos obrigatórios e evidências de entrega.
2. `docs/arquitetura.md` — limites, camadas e dependências.
3. `docs/ddd/modelagem-inicial.md` — hipóteses de domínio que ainda precisam ser validadas.
4. `docs/plano-implementacao.md` — sequência e critérios de aceite.
5. `docs/adr/` — decisões e suas justificativas.

## Regras inegociáveis
- **Não implementar tudo de uma vez.** Trabalhar somente na etapa solicitada, listar arquivos alterados, decisões e comandos de validação. Pedir confirmação antes da próxima etapa.
- A primeira interação deve **inspecionar o repositório e propor a Etapa 0**. Não criar endpoints de negócio sem autorização.
- Respeitar monólito organizado em camadas. O projeto `Domain` não referencia EF Core, ASP.NET, infraestrutura ou banco.
- Regras e transições da Ordem de Serviço ficam no domínio; controllers não devem conter regras de negócio.
- Não criar microserviços, mensageria distribuída, CQRS/MediatR, event sourcing ou abstrações genéricas sem justificativa e aprovação.
- Não inventar regras da FIAP: quando um comportamento não estiver especificado (ex.: orçamento rejeitado, reserva de estoque), propor alternativas e registrar decisão antes de codificar.
- Segurança: JWT nas rotas administrativas, segredos por variáveis de ambiente/user-secrets; jamais versionar tokens, senhas ou connection strings reais. APIs de consulta do cliente exigem desenho que evite exposição de dados de terceiros.
- PostgreSQL + EF Core: migrations versionadas, índices/constraints adequados, concorrência e transações nos fluxos de OS, orçamento e estoque.
- Testes unitários e integração para fluxos críticos; **cobertura >=80% dos domínios críticos**, com relatório verificável. Não confundir cobertura com correção.
- Dockerfile, compose, README, Swagger/OpenAPI e relatório de análise de vulnerabilidades são entregáveis.
- A documentação DDD, Event Storming e diagramas devem representar o código real. Atualizá-los quando decisões mudarem.
- Evitar `DateTime.Now` no domínio; preferir `TimeProvider` ou abstração de relógio injetada onde necessário. Usar `decimal` para valores monetários.
- Usar nomenclatura de domínio consistente (OrdemDeServico, Orcamento, Peca, Insumo, Cliente, Veiculo); nomes de endpoints e DTOs claros. Preferir código simples, testável e legível.
- Não marcar uma etapa como concluída sem executar os testes e comandos pertinentes; reportar o que não foi executado.

## Fluxo de trabalho de cada tarefa
1. Resumir escopo da etapa e requisitos atendidos.
2. Identificar decisões pendentes, se houver.
3. Propor alterações mínimas.
4. Implementar apenas após autorização do usuário.
5. Executar build/testes, relatar resultados e arquivos alterados.
6. Sugerir próximo passo, **sem executá-lo automaticamente**.

## Primeiro pedido recomendado
"Leia AGENTS.md e docs/. Não implemente a aplicação ainda. Revise a proposta arquitetural e prepare um plano de execução da Etapa 0, apontando dúvidas e riscos."
