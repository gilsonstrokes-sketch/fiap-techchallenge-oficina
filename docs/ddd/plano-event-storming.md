# Plano dos Event Stormings obrigatórios

Planejamento para as três sessões exigidas pelo PDF da FIAP (p. 8). Ainda não são registros de sessões realizadas nem diagramas finais. Publicar o board em Miro ou equivalente com acesso sem solicitação manual e exportar os artefatos aprovados para `docs/ddd/`.

## Preparação comum

- Participantes: integrantes do grupo e, se possível, alguém que represente recepção, mecânica, estoque e cliente.
- Delimitar início/fim do processo e cenários de exceção; separar fatos observados de decisões propostas.
- Ordenar eventos de domínio no tempo; depois identificar comandos, atores, políticas, leituras, regras, agregados, entidades e objetos de valor.
- Marcar perguntas sem resposta e decisões aprovadas com responsável/data; não preencher lacunas do enunciado por suposição.
- Consolidar linguagem ubíqua, glossário e candidatos a bounded context após comparar os três fluxos.
- Exportar board e diagramas pedidos na disciplina; revisar vínculo com código a cada etapa de implementação.

## Sessões

| Fluxo | Escopo e cenário principal | Exceções a explorar | Saídas mínimas |
|---|---|---|---|
| Criação da OS | Identificar cliente/veículo; receber solicitação; diagnosticar; incluir serviços/peças; gerar e disponibilizar orçamento; registrar decisão | Cliente/veículo inexistente ou inativo; orçamento revisado; rejeição; reparo adicional; falta de estoque | Linha temporal de comandos/eventos, atores, políticas, agregado OS/orçamento, VOs, regras e pendências |
| Acompanhamento da OS | Consultar estado e histórico desde recebimento até entrega; conduzir ações que alteram status | Consulta não autorizada ou a OS alheia; transição inválida; execução pausada; entrega recusada | Matriz de transições, política de acesso e dados visíveis, pontos de medição de duração, decisões e pendências |
| Gestão de peças/insumos | Cadastrar/inativar item, registrar entrada/ajuste, reservar após aprovação, baixar no início e registrar devolução | Saldo insuficiente; operações concorrentes; cancelamento/rejeição/revisão; devolução parcial | Ciclo de vida do estoque, invariantes, comandos/eventos, ator autorizado, agregado e estratégia de consistência a decidir |

## Critério de conclusão documental

Cada fluxo deve conter atores, comandos, eventos, políticas/regras, agregados, entidades/VOs, dúvidas e decisões. Os três exports, diagramas da disciplina, linguagem ubíqua e glossário devem ser versionados e acessíveis pelo link público informado na entrega. A documentação final será reconciliada com o código, não mantida como exemplo independente.
