# Modelagem DDD observada no código

**Estado:** documentação derivada da implementação atual, revisada em 2026-11-03. Os módulos e limites abaixo são candidatos de bounded context; o repositório não contém ata nem evidência de que as sessões obrigatórias de Event Storming tenham sido realizadas pelo grupo. Validar os limites e a linguagem com os participantes antes de apresentá-los como decisões de domínio aprovadas.

Os diagramas de fluxo estão em [`event-storming.md`](event-storming.md). Eles representam comportamentos encontrados no código e não eventos de integração publicados: o sistema não usa broker, event sourcing nem uma infraestrutura de eventos.

## Contextos candidatos e relações

| Contexto candidato | Responsabilidade no código | Relação observada |
|---|---|---|
| **Cadastro** | `Cliente` e `Veiculo`, documentos, placas e estado ativo | Atendimento referencia os identificadores. `OficinaApplicationService` valida cliente ativo, veículo ativo e vínculo do veículo antes de abrir OS. |
| **Catálogo** | `Servico` e `Peca`, descrição, preço de referência e inativação | Atendimento lê catálogo ao gerar orçamento e copia descrição, tipo, identificador, preço e quantidade para snapshots imutáveis. |
| **Atendimento** | `OrdemDeServico`, diagnóstico, histórico, versões de orçamento, decisões e execução | Raiz da OS; usa identificadores do Cadastro e snapshots lidos do Catálogo. |
| **Estoque** | `EstoqueDaPeca`, saldo físico e reservas por OS/versão | É agregado separado de `Peca`. A Application coordena a reserva e baixa junto às transições de Atendimento em transação PostgreSQL. |
| **Identidade e acesso** *(módulo técnico; bounded context não confirmado)* | Login administrativo JWT e emissão de token de consulta limitado a uma OS | Implementação em `Oficina.Api`; consulta de cliente recebe apenas dados reduzidos da OS. Não há identidade persistida nem provedor externo. |

Estes contextos são separações conceituais dentro de um único processo e banco. Não são projetos, serviços implantáveis nem limites já validados em workshop. A Application é a camada de orquestração; Infrastructure implementa repositórios, `xmin` e Unit of Work; Api compõe, autentica e expõe HTTP.

## Agregados, entidades e invariantes

| Agregado / raiz | Entidades e estado subordinado | Invariantes observadas |
|---|---|---|
| `Cliente` | Raiz `Cliente` | Nome não vazio; `DocumentoFiscal` precisa ser válido; documento é imutável no modelo; ativo/inativo pode ser alternado. O índice `ux_clientes_documento_fiscal` garante unicidade persistida. |
| `Veiculo` | Raiz `Veiculo` | Cliente obrigatório; placa válida; marca/modelo não vazios; ano positivo; cliente não pode ser trocado após criação pelo modelo atual. Placa única no banco; FK composta mantém o par veículo/cliente usado pela OS. |
| `Servico` | Raiz `Servico` | Nome e descrição não vazios; preço é `Dinheiro`; pode ser inativado/reativado. O orçamento conserva snapshot do preço anterior. |
| `Peca` | Raiz de catálogo `Peca` | Nome e descrição não vazios; preço é `Dinheiro`; pode ser inativada/reativada. Não contém saldo: este pertence a `EstoqueDaPeca`. |
| `OrdemDeServico` | `Orcamento` versionado, `ItemOrcamento`, `TransicaoOrdemDeServico`, `DecisaoOrcamento` | Apenas as seis situações RF10; cada ação valida o estado de origem; datas não podem anteceder o último evento/decisão; versão de orçamento positiva e não vazia; histórico e decisões permanecem associados à OS. Aprovação é booleano separado enquanto o status permanece `AguardandoAprovacao`. |
| `EstoqueDaPeca` | `ReservaDeEstoque` por OS e versão | Saldo físico e quantidades são inteiros não negativos/positivos; quantidade disponível não pode ser negativa; reserva é única por peça/OS/versão; baixa só consome reserva existente e reduz saldo físico. `xmin` detecta atualização concorrente. |

As referências entre raízes são por identificador e validadas na Application/Infrastructure. Não há entidade `Insumo` distinta: o código trata itens controlados no estoque como `Peca`. Confirmar se isso cobre o vocabulário exigido pelo grupo/FIAP.

## Objetos de valor e tipos de domínio

- `DocumentoFiscal`: normaliza e valida CPF e CNPJ numérico/alfanumérico; deriva `TipoDocumentoFiscal`.
- `Placa`: normaliza e valida formato antigo e Mercosul.
- `Dinheiro`: `decimal` não negativo, até duas casas decimais; soma preserva a regra.
- `ItemOrcamento`: snapshot imutável de tipo, item de catálogo, descrição, quantidade inteira positiva, preço e total da linha; arredonda a linha com `MidpointRounding.AwayFromZero`.
- `ReservaPecaConfirmada` e `ConfirmacaoReservaEstoque`: contratos tipados para validar OS, versão, peças e quantidades antes da transição de início. Não são assinatura criptográfica nem prova independente de estado persistido; a Application deve criá-los a partir do estoque carregado e usá-los na transação.
- `StatusOrdemDeServico` e `TipoItemOrcamento`: enums fechados no código.

IDs `Guid`, timestamps e números de sequência do banco são identificadores/tipos técnicos, não objetos de valor de negócio.

## Transições de OS implementadas

| Estado atual | Comando | Estado após sucesso | Observação |
|---|---|---|---|
| `Recebida` | `IniciarDiagnostico` | `EmDiagnostico` | Grava histórico. |
| `EmDiagnostico` | `DisponibilizarOrcamento` | `AguardandoAprovacao` | Gera nova versão com snapshots do Catálogo. |
| `AguardandoAprovacao` | `RejeitarOrcamento` | `EmDiagnostico` | Guarda decisão, versão, data e motivo opcional; preserva versões. |
| `AguardandoAprovacao` | `AprovarOrcamento` | `AguardandoAprovacao` | Registra aprovação separadamente; reserva todas as peças somente se houver saldo suficiente para o conjunto. |
| `AguardandoAprovacao` aprovado | `IniciarExecucao` | `EmExecucao` | Application valida/reserva, compõe confirmação, baixa estoque e inicia OS na mesma transação. Se faltar estoque, falha sem remover a aprovação. |
| `EmExecucao` | `FinalizarExecucao` | `Finalizada` | Grava instante de conclusão. |
| `Finalizada` | `EntregarVeiculo` | `Entregue` | Estado final da OS. |

Substituir orçamento aprovado antes da baixa libera as reservas da versão anterior na operação transacional e cria nova versão ainda não aprovada. Não há status de cancelamento nem fluxo de reparos adicionais após o início da execução.

## Decisões técnicas aplicadas

- Monólito modular em camadas .NET 10: `Domain` não referencia EF Core/ASP.NET; `Application` depende de Domain; `Infrastructure` implementa persistência; `Api` faz composição e HTTP.
- PostgreSQL + EF Core/Npgsql com migration versionada, constraints/índices, `numeric` e FKs explícitas.
- Concorrência otimista com `xmin` em OS e estoque; conflitos são convertidos em `ConcurrencyConflictException`, expostos como HTTP 409; sem retry automático.
- Transações explícitas no Unit of Work para aprovação/reserva, substituição e início/baixa. Os testes PostgreSQL reais cobrem aprovação/reserva/baixa, falha após gravação e conflito com dois `DbContext`; não cobrem ainda todas as combinações de multi-peça concorrente.
- A migration é aplicada por `dotnet ef database update`/fixture de testes; o startup da API não migra automaticamente o banco.
- Testes de persistência recebem `OFICINA_TEST_POSTGRES`; os cenários críticos limpam os próprios registros, sem truncar tabelas.

## Pendências de validação de domínio

O PDF não especifica canal de envio do orçamento, política de estoque insuficiente, identidade do cliente, cancelamento, devolução, imposto/desconto, pausas de execução ou granularidade do indicador. Decisões de negócio já registradas em `docs/decisoes-negocio.md` orientam a implementação, mas os pontos ainda abertos — em especial DB06 (métrica por OS ou serviço), devoluções/cancelamentos e acesso do cliente — continuam propostas/pendências do grupo, não regras FIAP.
