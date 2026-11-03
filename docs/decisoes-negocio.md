# Decisões de negócio — propostas iniciais

As decisões abaixo foram fornecidas pelo grupo como propostas para modelagem. Não são regras prescritas pela FIAP e não devem ser tratadas como aprovadas para implementação até validação explícita do grupo. O registro deve ser atualizado quando uma decisão for confirmada.

| ID | Tema | Proposta inicial | Questões que ainda precisam de resposta |
|---|---|---|---|
| DB01 | Rejeição de orçamento | Retornar OS a `Em diagnóstico`, preservando a versão e a decisão; permitir nova versão | Quem pode revisar? A rejeição precisa de motivo? |
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

## Decisões confirmadas pelo grupo — 2026-11-03

As confirmações abaixo atualizam parcialmente as propostas da tabela. Os pontos explicitamente pendentes continuam sem regra implementada.

| ID | Decisão confirmada | Limite/ponto pendente |
|---|---|---|
| DB01 | Rejeitar orçamento retorna a OS para `Em diagnóstico`. | Ainda não foi decidido se o motivo é obrigatório; versões são preservadas sem limite de negócio arbitrário conforme DB19. |
| DB02 | Reservar estoque após aprovação; baixar no início da execução; não permitir saldo disponível negativo. Falta de estoque bloqueia o início e preserva a aprovação. Estoque é agregado separado de `Peca`. | Atomicidade de reserva/baixa entre múltiplas peças, liberação/devolução e cancelamento seguem pendentes. |
| DB03 | PostgreSQL usa concorrência otimista; conflitos são informados explicitamente e não há retry automático. Reservas de múltiplas peças são verificadas no estado persistido e gravadas atomicamente. Application coordena os agregados; Infrastructure implementa persistência/transações. | A resposta HTTP do conflito será definida quando os endpoints forem implementados. |
| DB04 | Aprovação autoriza a execução, mas não inicia a execução. O início é uma ação de negócio separada. | O estado aprovado permanece representado junto ao status `AguardandoAprovacao` até o início, sem novo status. |
| DB13 | Valores usam BRL, tipo decimal, não negativos, com zero permitido; cada linha é arredondada para duas casas. O modo é `AwayFromZero`, conforme DB17. | Descontos e impostos não foram definidos. |
| RF05/RT05 | Aceitar placas no formato antigo e Mercosul, normalizadas em maiúsculas sem hífen, ponto ou espaços. | Unicidade da placa e transferência de veículo entre clientes não foram definidas. |
| DB14 | Orçamento concreto com ao menos um serviço ou peça; linhas preservam tipo, item de catálogo, descrição, quantidade e preço unitário; total soma linhas arredondadas; cada nova proposta gera versão imutável. | Detalhamentos de arredondamento e quantidade estão nas decisões DB17 e DB18. |
| DB15 | Rejeição registra data e versão do orçamento; motivo é opcional. Reserva confirmada é um contrato tipado validado contra OS, versão, peças e quantidades do orçamento; a Application verifica as reservas no estado persistido e não confia exclusivamente em confirmação do chamador. | O contrato tipado não prova sozinho que uma reserva ainda está ativa; persistência e concorrência são verificadas na operação transacional. |
| DB16 | Documento fiscal do cliente é imutável neste MVP. | Correções cadastrais deverão seguir processo administrativo a definir, se necessário. |
| DB17 | Valores de cada linha são arredondados para duas casas com `MidpointRounding.AwayFromZero`. | Impostos e descontos continuam fora do escopo definido. |
| DB18 | Quantidades de serviços e peças são inteiras e positivas; saldo físico do estoque é inteiro e não negativo. | Unidade fracionária não é suportada neste MVP. |
| DB19 | Orçamentos são versionados e imutáveis, preservando histórico, sem limite de negócio arbitrário de versões. | A numeração técnica usa inteiro positivo; limites físicos da plataforma não são política de negócio. |
| DB20 | Cancelamento de OS está fora do escopo da Etapa 2; permanecem somente os seis estados oficiais. | Não foi definida transição de cancelamento nem estado correspondente. |
| DB21 | Ao substituir orçamento aprovado antes da baixa, as reservas da versão anterior devem ser liberadas, e a nova versão exige nova aprovação do cliente. A substituição e a liberação são coordenadas transacionalmente pela Application/Infrastructure. | Reparos adicionais após o início da execução ficam fora desta etapa. |

As demais linhas da tabela permanecem propostas ou pendentes, não aprovadas.

### Limites do contrato de confirmação de reserva

`ReservaPecaConfirmada` não tem construtor público e é emitida pelo agregado `EstoqueDaPeca` após uma reserva local bem-sucedida. `ConfirmacaoReservaEstoque.Criar` é público para permitir que a Application componha os recibos; valida OS, versão, duplicidade de peças e a OS valida correspondência exata das quantidades do orçamento. O contrato é imutável e não aceita um booleano como prova.

O contrato não é assinatura criptográfica, não consulta persistência e não prova que a reserva ainda está ativa nem que os recibos são recentes; uma confirmação válida pode ser reutilizada. Embora o construtor do recibo seja interno, qualquer código com acesso ao modelo pode instanciar um agregado de estoque em memória e obter recibos dele, portanto o recibo também não comprova por si só que houve reserva no estoque persistido autoritativo. A Application deve construí-la somente a partir dos resultados das reservas persistidas e utilizá-la na mesma transação que inicia a OS e baixa as reservas. Validade persistida, uso único efetivo, atomicidade, concorrência e idempotência são responsabilidades da Etapa 3.

## Vocabulário de decisão

- **Proposta inicial:** hipótese orientadora de modelagem, ainda sujeita a validação.
- **Aprovada pelo grupo:** decisão confirmada e apta a orientar implementação; registrar quem/data/justificativa em ADR ou neste documento.
- **Não definido pela FIAP:** lacuna do enunciado; não atribuir a decisão à instituição.

## Comportamento de estoque implementado na Etapa 3

A aprovação da OS é registrada mesmo quando não há saldo suficiente. Se todos os itens de peça tiverem saldo disponível, as reservas são criadas atomicamente após a aprovação. Se faltar saldo, o início da execução permanece bloqueado e a aprovação continua registrada; uma tentativa posterior pode reservar e baixar as quantidades se o saldo já estiver disponível. A API não aceita confirmações de reserva: a Application consulta o estado persistido, compõe o contrato internamente e executa reserva/baixa junto às transições em transação. Conflitos otimistas são explícitos, sem retry automático. Esta implementação segue a decisão confirmada de que estoque insuficiente bloqueia o início e preserva a aprovação.
