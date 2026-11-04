# Escopo dos Event Stormings — Fase 1

O enunciado da FIAP prevê três fluxos de Event Storming. Esta página delimita os cenários e os artefatos correspondentes. Os diagramas Mermaid em [`event-storming.md`](event-storming.md) descrevem o comportamento observado no código e não constituem registros de workshop.

## Fluxos previstos

| Fluxo | Cenário principal | Exceções e questões de domínio | Artefatos esperados |
|---|---|---|---|
| Criação da OS | Identificação de cliente/veículo, recebimento, diagnóstico, itens de serviço/peça, orçamento e decisão. | Cadastro inexistente/inativo, revisão, rejeição, reparo adicional e indisponibilidade de estoque. | Linha temporal de comandos e eventos, atores, políticas, agregado OS/orçamento, objetos de valor e decisões. |
| Acompanhamento da OS | Consulta de estado e histórico do recebimento à entrega e ações que alteram o status. | Consulta sem autorização ou a OS de terceiro, transição inválida, pausa e entrega recusada. | Matriz de transições, dados visíveis, política de acesso, métricas e pontos em aberto. |
| Gestão de peças/insumos | Cadastro, entrada, reserva após aprovação, baixa no início e eventual devolução. | Saldo insuficiente, concorrência, cancelamento, rejeição, revisão e devolução parcial. | Ciclo de vida do estoque, invariantes, comandos/eventos, ator autorizado e consistência. |

## Registro e evidências

Um registro final das sessões deve identificar participantes, data, decisões, questões em aberto e os boards/exports correspondentes. Esses registros e links de boards públicos não estão presentes nesta versão do repositório. A validação colaborativa do vocabulário, dos limites de contexto e das decisões permanece a cargo do grupo.
