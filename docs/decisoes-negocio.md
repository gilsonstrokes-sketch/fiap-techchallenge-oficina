# Decisões de negócio — propostas iniciais

As decisões abaixo foram fornecidas pelo grupo como propostas para modelagem. Não são regras prescritas pela FIAP e não devem ser tratadas como aprovadas para implementação até validação explícita do grupo. O registro deve ser atualizado quando uma decisão for confirmada.

| ID | Tema | Proposta inicial | Questões que ainda precisam de resposta |
|---|---|---|---|
| DB01 | Rejeição de orçamento | Retornar OS a `Em diagnóstico`, preservando a versão e a decisão; permitir nova versão | Quem pode revisar? A rejeição precisa de motivo? Há limite de revisões? |
| DB02 | Estoque | Reservar após aprovação; baixar no início da execução; nunca permitir saldo negativo | O que ocorre se estoque não puder ser reservado após a aprovação? A reserva expira? Como devoluções, cancelamentos e perdas são registradas? |
| DB03 | Concorrência e transações | Concorrência e transações devem proteger aprovação/reserva e início/baixa | Qual estratégia técnica será usada? A reserva de vários itens é atômica? Como tratar repetição da mesma solicitação? |
| DB04 | Status | Toda transição decorre de uma ação de negócio | Aprovação leva imediatamente a `Em execução` ou há ação explícita posterior para iniciar? Como representar OS aprovada aguardando execução sem criar status fora do enunciado? |
| DB05 | Consulta do cliente | Exigir autenticação ou token restrito à OS | Qual alternativa será adotada? Como emitir, expirar, revogar e evitar enumeração/reuso do token? Quais campos são visíveis? |
| DB06 | Tempo médio | Medir entre início e finalização da execução | Métrica é da OS inteira ou individual por serviço? Considera espera/pausas? Qual janela e agrupamento? |
| DB07 | Preços | Valores aprovados são imutáveis | Imutabilidade vale por versão aprovada? Como representar uma revisão posterior sem alterar orçamento já aprovado? |
| DB08 | Cadastros em uso | Inativar em vez de excluir fisicamente | Como reativar? Uma inativação bloqueia itens de OS já abertos? Aplicar a todos os cadastros? |
| DB09 | Administração | Um perfil administrativo no MVP | Esse perfil tem acesso a todos os cadastros, estoque e OS? Como serão criadas/rotacionadas credenciais sem segredo fixo? |
| DB10 | Reparos adicionais | Avaliar o fluxo de revisão de orçamento para atender ao contexto do PDF | Reparos adicionais são nova versão do orçamento e exigem nova aprovação? Como separar itens já aprovados de novos itens? |
| DB11 | Estoque insuficiente | Sem proposta fechada além de não permitir saldo negativo | Rejeitar aprovação, permitir aprovação pendente de compra ou oferecer substituição? O PDF não define o comportamento. |
| DB12 | Envio do orçamento | Disponibilizar é suficiente; provedor externo não está definido | Qual canal será demonstrado? Uma API de consulta autenticada é suficiente para o MVP? |
| DB13 | Cobrança | FIAP não define impostos, descontos, condições de pagamento nem moeda além dos preços | Quais elementos entram no valor calculado e como arredondar? |

## Vocabulário de decisão

- **Proposta inicial:** hipótese orientadora de modelagem, ainda sujeita a validação.
- **Aprovada pelo grupo:** decisão confirmada e apta a orientar implementação; registrar quem/data/justificativa em ADR ou neste documento.
- **Não definido pela FIAP:** lacuna do enunciado; não atribuir a decisão à instituição.
