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
| Orçamento | Itens e valores propostos para autorização do cliente; cada versão é imutável e o histórico é preservado |
| Revisão do orçamento | Nova versão imutável da proposta; não há limite de negócio arbitrário de versões |
| Reserva | Quantidade comprometida para uma OS aprovada, ainda não baixada do saldo físico |
| Baixa | Consumo da quantidade reservada no início da execução |
| Inativação | Impedir uso futuro em novos registros sem apagar referências históricas |

## Implementação do domínio (Etapa 2)

Implementados no projeto `Oficina.Domain`:

- `DocumentoFiscal` normaliza CPF/CNPJ; valida CPF e CNPJ numérico/alfanumérico pelos dígitos verificadores e deriva o tipo do documento validado.
- `Placa` aceita formato antigo e Mercosul; normaliza para maiúsculas e remove hífen, ponto e espaços.
- `Dinheiro` representa BRL como `decimal`, aceita zero, rejeita valores negativos e valores com mais de duas casas decimais.
- `Cliente`, `Veiculo`, `Servico` e `Peca` validam os dados essenciais de criação e atualização. `Peca` não carrega saldo de estoque.
- `OrdemDeServico` é a raiz do agregado de atendimento. Inicia em `Recebida`, registra as transições por ações explícitas, decisões do orçamento e os horários de início/finalização da execução.
- `EstoqueDaPeca` é uma raiz separada por item de catálogo: controla saldo físico e uma coleção persistível de `ReservaDeEstoque` por OS/versão, disponibilidade e baixa, sem aceitar reserva que gere saldo disponível negativo. A raiz pode emitir um recibo apenas para uma reserva ainda presente no estado carregado.
- `Orcamento` é uma versão imutável dentro da OS. Cada `ItemOrcamento` guarda snapshot de tipo, identificador de catálogo, descrição, quantidade inteira positiva e preço unitário; linha é arredondada a duas casas com `MidpointRounding.AwayFromZero` e o total da versão é calculado no domínio. Não há limite de negócio para a quantidade de versões. A rejeição registra versão, data e motivo opcional.
- `ConfirmacaoReservaEstoque` é o contrato tipado entre a orquestração da aplicação e a OS: valida identidade da OS, versão aprovada e as quantidades agregadas de cada peça sem referência ao agregado `EstoqueDaPeca`.
- O contrato de confirmação é uma validação de coerência, não uma prova criptográfica ou persistida: os recibos têm construtor interno e são emitidos pelo estoque, mas a confirmação pode ser reutilizada e não verifica se a reserva continua ativa. A Application deve compor os recibos após todas as reservas e usá-los na transação de início/baixa; uso único efetivo, atomicidade e concorrência ficam para a Etapa 3.
- `OrdemDeServico` possui construtor privado sem parâmetros e setters privados para permitir materialização futura pelo EF Core; o mapeamento permanece responsabilidade de Infrastructure, sem anotações ou referências a EF no Domain.

`StatusOrdemDeServico` contém os estados RF10: `Recebida`, `EmDiagnostico`, `AguardandoAprovacao`, `EmExecucao`, `Finalizada` e `Entregue`. O fluxo implementado é `Recebida` → `EmDiagnostico` → `AguardandoAprovacao`; rejeição retorna a `EmDiagnostico`; aprovação registra autorização sem alterar o status; início explícito, com reserva confirmada, leva a `EmExecucao`; finalização e entrega avançam para `Finalizada` e `Entregue`.

A OS mantém aprovação como estado interno enquanto o status oficial continua `AguardandoAprovacao`. `DisponibilizarOrcamento` cria uma nova versão concreta, não vazia, a partir de snapshots fornecidos pela aplicação; rejeição preserva a versão e libera a OS para diagnóstico e nova versão. A aplicação deve validar que o veículo pertence ao cliente antes de criar a OS. Também deve coordenar aprovação/reserva e início/baixa entre agregados, emitir a confirmação tipada apenas após todas as reservas terem sucesso e executar a operação em transação. Transações, concorrência otimista/relacional e testes PostgreSQL ficam para a Etapa 3; a camada Domain não acessa infraestrutura.

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
| OrdemDeServico (raiz) | Status, ações válidas, histórico, decisões, versões imutáveis do orçamento e tempos de execução. Cliente e veículo são referenciados por identificadores; Application valida a relação entre eles. |
| EstoqueDaPeca (raiz por peça) | Saldo físico e reservas por OS; disponibilidade não negativa; baixa consome somente quantidade reservada. A coordenação transacional de várias peças segue para a camada Application/Infrastructure. |
| Cliente | Identificador, nome e documento fiscal validado. Unicidade persistente e inativação ainda dependem de decisões pendentes. |
| Veiculo | Identificador, cliente, placa, marca, modelo e ano. Unicidade e transferência de proprietário seguem pendentes. |
| Servico / Peca | Entidades de catálogo com nome, descrição e preço `Dinheiro`; `Peca` não possui estoque. |
| Orcamento (entidade imutável na OS) | Versão sem limite arbitrário de negócio, linhas snapshot com quantidades inteiras positivas, total calculado por linha arredondada com `AwayFromZero`. A decisão de rejeição referencia data/versão e aceita motivo opcional. |

Objetos de valor implementados: `DocumentoFiscal`, `Placa`, `Dinheiro`, `ItemOrcamento`, `ReservaPecaConfirmada` e `ConfirmacaoReservaEstoque`; `decimal` representa dinheiro e quantidades, mas as quantidades aceitas neste MVP são inteiras e positivas (saldo físico inteiro não negativo).

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
| AprovarOrcamento | OrcamentoAprovado; aplicação reserva as peças da versão aprovada |
| RejeitarOrcamento | OrcamentoRejeitado; OrdemDeServicoRetornadaAoDiagnostico |
| RevisarOrcamento | NovaVersaoDeOrcamentoGerada (pendente: política de versões) |
| IniciarExecucao | ExecucaoIniciada; exige recibos de reserva correspondentes e a aplicação baixa as reservas |
| FinalizarExecucao | ExecucaoFinalizada / OrdemDeServicoFinalizada |
| EntregarVeiculo | VeiculoEntregue |
| AjustarEstoque / DevolverPeca | EstoqueMovimentado / EstoqueDevolvido |

Os comandos de CRUD de serviço/peça/insumo e cadastro complementam os fluxos. Eventos definitivos devem ser atualizados após o Event Storming e refletir o código real.

## Transições de OS — implementadas e pendências

| De | Para | Ação de negócio proposta | Observação |
|---|---|---|---|
| Recebida | Em diagnóstico | Iniciar diagnóstico | Implementado |
| Em diagnóstico | Aguardando aprovação | Disponibilizar orçamento | Implementado; cria versão concreta não vazia, calcula linhas e total |
| Aguardando aprovação | Em diagnóstico | Rejeitar orçamento | Implementado; registra versão/data e motivo opcional |
| Aguardando aprovação (aprovada) | Em execução | Iniciar execução | Ação separada; requer confirmação tipada que corresponde às peças/quantidades da versão aprovada |
| Em execução | Finalizada | Finalizar execução | Implementado; guarda data/hora de conclusão |
| Finalizada | Entregue | Entregar veículo | Implementado |

Não há status adicional. Cancelamento de OS está fora do escopo da Etapa 2. A camada Application valida o vínculo cliente/veículo e coordena reserva após aprovação, confirmação e baixa no início. Ao substituir orçamento aprovado antes da baixa, deve liberar as reservas da versão anterior na mesma transação que publica a nova versão; esse fluxo, atomicidade, concorrência, rollback e testes PostgreSQL são responsabilidades da Etapa 3. Quantidades fracionárias não são aceitas neste MVP. A confirmação é reutilizável e não prova reserva persistida ativa: a Application deve obter recibos do fluxo de reserva persistida e garantir uso transacional único na Etapa 3.

## Identidade e consulta

O enunciado exige consulta de andamento por cliente, mas não dita o mecanismo. Validar autenticação de cliente ou token limitado a uma OS, escopo, expiração/revogação, minimização de dados e proteção contra enumeração. O perfil administrativo único não deve ser confundido com a identidade/autorização do cliente.

## Perguntas que o Event Storming deve resolver

1. A reserva de várias peças deve ser atômica? Como liberar/devolver reserva em outros fluxos fora da substituição aprovada?
2. Como novos itens são autorizados após rejeição e após início da execução?
3. Qual evento marca início/fim para o indicador: execução da OS ou execução individual de cada serviço?
6. Quais atores podem diagnosticar, orçar, aprovar, iniciar, finalizar, entregar e ajustar estoque?
7. Qual política de exclusão/inativação aplica a cada cadastro e às relações já existentes?

