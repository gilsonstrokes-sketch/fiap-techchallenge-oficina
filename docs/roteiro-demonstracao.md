# Roteiro de demonstração — 9 minutos

Use este roteiro como ensaio para o vídeo FIAP (o limite oficial é 15 minutos). Grave no commit que será entregue, com banco de demonstração descartável e dados fictícios. Antes de gravar, confirme links públicos, identificação do grupo e os limites listados na matriz. Não mostre senhas, tokens completos ou connection strings.

| Tempo | Demonstração | Evidência/observação |
|---|---|---|
| 0:00–0:35 | Identificar curso, Fase 1, integrantes e escopo do back-end. | Incluir nomes completos, RMs e usernames Discord no vídeo/PDF conforme solicitado. |
| 0:35–1:20 | Mostrar camadas e dependências: API, Application, Domain, Infrastructure; justificar PostgreSQL. | Abrir README e arquitetura. Ressaltar que Domain não depende de EF/ASP.NET. |
| 1:20–2:05 | Mostrar Compose, containers saudáveis, `/health`, Swagger e OpenAPI. | Swagger está disponível no Compose `Development`; não apresentar isso como configuração de produção. `/health` atual não valida o banco. |
| 2:05–2:50 | Solicitar token administrativo e demonstrar endpoint protegido sem token (401) e autenticado. | Não narrar nem deixar token/segredo visível na gravação. Usar ambiente local. |
| 2:50–4:00 | Criar/consultar cliente e veículo e apresentar serviços, peças e saldo de estoque. | Usar dados fictícios e únicos; mostrar validação de CPF/CNPJ/placa e persistência. |
| 4:00–5:20 | Criar OS, registrar diagnóstico, gerar orçamento e mostrar versão/total/snapshots. | Mostrar relação cliente/veículo, itens e os seis status descritos no domínio. |
| 5:20–6:15 | Registrar aprovação ou rejeição e explicar transição e histórico; apresentar consulta segura do cliente. | **Precisão importante:** aprovação/rejeição hoje são administrativas. Não dizer que o cliente aprova pela API. O token do cliente só consulta uma OS vinculada. |
| 6:15–7:00 | Mostrar reserva e baixa de estoque ao iniciar execução; finalizar e entregar OS. | Demonstrar com saldo suficiente; explicar comportamento de saldo insuficiente sem simular sucesso. |
| 7:00–8:10 | Mostrar diagramas Mermaid/Event Storming, agregados, invariantes e decisões. | Os diagramas versionados são derivados do código; não substituem prova dos workshops/board público do grupo. Mostrar os links/export reais quando disponíveis. |
| 8:10–9:00 | Executar ou mostrar saída recente de build e testes PostgreSQL: rollback e conflito concorrente. | Informar o commit/data correspondentes. A evidência conhecida é 63 testes aprovados; repetir no commit final antes de gravar. |
| 9:00–9:30 | Resumir cobertura e análise de vulnerabilidades, pendências e links. | Cobertura informada mede Domain; NuGet sem advisories na verificação registrada; CVE da imagem ainda pendente. Não chamar a revisão de certificação. |

## Preparação técnica

- Subir API e PostgreSQL por Compose e confirmar migrations na base descartável antes de gravar.
- Popular apenas dados fictícios, sem executar comandos destrutivos contra `oficina`.
- Preparar um fluxo de chamadas com IDs/valores anotados fora do vídeo; evitar improvisar dados inválidos.
- Preparar uma chamada sem JWT e outra autenticada; ocultar cabeçalhos `Authorization` e valores de configuração na captura.
- Capturar a execução real dos testes no mesmo commit/ambiente cuja evidência será apresentada. Se usar gravação de terminal, evitar imprimir variáveis secretas.
- Mostrar o relatório de análise com suas limitações explícitas; obter a varredura de imagem ainda pendente antes de afirmar cobertura completa de CVEs.

## Itens que não podem ser apresentados como concluídos

- Aprovação/rejeição pelo cliente na API: atualmente não existe endpoint de decisão do cliente.
- Teste automatizado de tentativa do cliente consultar OS alheia: não comprovado.
- Scanner de imagem completo: não concluído nesta máquina.
- Workshop Event Storming, board público, export visual, PDF final, vídeo público e branch/hash finais: dependem da produção/publicação pelo grupo.
