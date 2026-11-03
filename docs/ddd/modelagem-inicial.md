# Modelagem DDD — proposta para validação

Este documento é uma hipótese de modelagem baseada no enunciado e nas propostas de negócio em `docs/decisoes-negocio.md`. Não substitui os três Event Stormings obrigatórios. Nenhum bounded context está validado até a sessão; módulos abaixo são organização lógica preliminar.

## Linguagem ubíqua inicial

| Termo | Significado provisório |
|---|---|
| Cliente | Pessoa física ou jurídica identificada por CPF/CNPJ |
| Veículo | Veículo vinculado ao cliente, identificado pela placa |
| Ordem de Serviço (OS) | Registro do atendimento, diagnóstico, autorização e execução de serviços para um veículo |
| Serviço | Atividade oferecida pela oficina, com preço de referência |
| Peça/Insumo | Item catalogado e sujeito a controle de estoque |
| Orçamento | Versão dos itens e valores propostos para autorização do cliente |
| Revisão do orçamento | Nova versão que preserva as anteriores e suas decisões |
| Reserva | Quantidade comprometida para uma OS aprovada, ainda não baixada do saldo físico |
| Baixa | Consumo de quantidade no início da execução, conforme proposta a validar |
| Inativação | Impedir uso futuro em novos registros sem apagar referências históricas |

## Módulos lógicos candidatos

- **Cadastro:** Cliente e Veículo.
- **Catálogo:** Serviço e Peça/Insumo.
- **Estoque:** saldo, reservas e movimentos.
- **Atendimento:** OS, diagnóstico, itens, orçamento, decisões e execução.
- **Identidade:** perfil administrativo e autorização da consulta de cliente.

São módulos no mesmo monólito e não bounded contexts confirmados. A fronteira entre Catálogo e Estoque, e a fronteira entre Atendimento e Orçamento, devem ser validadas no Event Storming.

## Agregados e invariantes candidatos

| Agregado candidato | Responsabilidade/invariantes a validar |
|---|---|
| OrdemDeServico (raiz) | Estado e histórico de transições; vínculo cliente/veículo; itens de serviço/peças; referência às versões de orçamento. Não aceitar transições arbitrárias. |
| Orcamento (raiz ou entidade da OS) | Linhas, snapshots de descrição/preço/quantidade, versão e decisão imutável. A escolha de fronteira depende de concorrência e histórico. |
| EstoquePorItem (raiz candidata) | Saldo, reservas e movimentos; rejeitar operações que gerem saldo disponível negativo. O limite de consistência entre itens precisa ser decidido. |
| Cliente | Documento fiscal único normalizado e estado ativo/inativo. |
| Veiculo | Placa normalizada e associação a cliente conforme regra a decidir. |
| Servico / ItemCatalogo | Dados de catálogo e inativação, preservando referências históricas. |

Objetos de valor candidatos: `DocumentoFiscal`, `Placa`, `Dinheiro`, `Quantidade`, `VersaoOrcamento` e identificadores. Usar `decimal` para valores monetários. Regras de normalização/validação precisam ser especificadas e testadas.

## Comandos e eventos candidatos

Comandos são intenções de negócio; eventos são fatos que ocorreram. Nomes abaixo não implicam mensageria ou event sourcing.

| Comando candidato | Eventos candidatos após sucesso |
|---|---|
| RegistrarCliente / InativarCliente | ClienteRegistrado / ClienteInativado |
| RegistrarVeiculo / VincularVeiculo | VeiculoRegistrado / VeiculoVinculadoAoCliente |
| AbrirOrdemDeServico | OrdemDeServicoRecebida |
| IniciarDiagnostico | DiagnosticoIniciado |
| AdicionarServico / AdicionarPeca | ItemDeServicoAdicionado / PecaAdicionada |
| GerarOrcamento | OrcamentoGerado |
| DisponibilizarOrcamento | OrcamentoDisponibilizado |
| AprovarOrcamento | OrcamentoAprovado; ReservaDeEstoqueEfetuada (se a proposta for aprovada) |
| RejeitarOrcamento | OrcamentoRejeitado; OrdemDeServicoRetornadaAoDiagnostico (proposta) |
| RevisarOrcamento | NovaVersaoDeOrcamentoGerada |
| IniciarExecucao | ExecucaoIniciada; EstoqueBaixado (conforme proposta) |
| FinalizarExecucao | ExecucaoFinalizada / OrdemDeServicoFinalizada |
| EntregarVeiculo | VeiculoEntregue |
| AjustarEstoque / DevolverPeca | EstoqueMovimentado / EstoqueDevolvido |

Os comandos de CRUD de serviço/peça/insumo e cadastro complementam os fluxos. Eventos definitivos devem ser atualizados após o Event Storming e refletir o código real.

## Transições de OS — rascunho para validação

| De | Para | Ação de negócio proposta | Observação |
|---|---|---|---|
| Recebida | Em diagnóstico | Iniciar diagnóstico | Definir pré-condições e responsável |
| Em diagnóstico | Aguardando aprovação | Disponibilizar orçamento | Exige orçamento calculado e itens consistentes |
| Aguardando aprovação | Em diagnóstico | Registrar rejeição | Proposta inicial DB01; preservar versão e decisão |
| Aguardando aprovação | Em execução | Aprovar orçamento e iniciar execução | Rascunho: depende de resolver se aprovação e início são uma ação ou duas; estoque precisa estar reservado/consistente |
| Em execução | Finalizada | Finalizar execução | Definir se todos os serviços/itens precisam estar concluídos |
| Finalizada | Entregue | Registrar entrega ao cliente | Definir condições/documentação da entrega |

As setas são apenas o fluxo nominal proposto. Não implementar até validar ações, autorização, exceções e a transição orçamento aprovado → execução. Rejeição, revisão, falta de estoque e cancelamento são pontos em aberto; nenhum status adicional está sendo proposto.

## Identidade e consulta

O enunciado exige consulta de andamento por cliente, mas não dita o mecanismo. Validar autenticação de cliente ou token limitado a uma OS, escopo, expiração/revogação, minimização de dados e proteção contra enumeração. O perfil administrativo único não deve ser confundido com a identidade/autorização do cliente.

## Perguntas que o Event Storming deve resolver

1. Aprovação e início de execução são um único comando? Como tratar falta de estoque após o cliente aprovar?
2. Reserva é feita para todos os itens atomicamente? Como expira, cancela e devolve?
3. Uma rejeição exige motivo? Quantas revisões são permitidas? Como itens adicionais são aprovados?
4. A OS pode incluir serviços adicionais após início? Isso cria nova versão de orçamento?
5. Qual evento marca início/fim para o indicador: execução da OS ou execução individual de cada serviço?
6. Quais atores podem diagnosticar, orçar, aprovar, iniciar, finalizar, entregar e ajustar estoque?
7. Qual política de exclusão/inativação aplica a cada cadastro e às relações já existentes?

