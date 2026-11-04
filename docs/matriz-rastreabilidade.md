# Matriz de rastreabilidade — Fase 1

Auditoria documental/código em 2026-11-03. Fonte dos requisitos: [`requisitos-fiap.md`](requisitos-fiap.md), transcrição do PDF oficial citado no próprio documento. Matriz descreve o que foi observado no código e nas evidências disponíveis; não substitui validação pelo grupo nem uma execução no commit final.

## Legenda

- **Concluído (C):** implementação/evidência observada para o escopo indicado.
- **Parcial (P):** existe implementação, mas há lacuna explícita de comportamento, teste, escopo ou evidência.
- **Pendente/não comprovado (N):** não foi possível comprovar no repositório/execução acessível; não significa necessariamente ausência de trabalho fora dele.

## Requisitos funcionais

| ID | Status | Evidência observada | Lacuna ou ação restante |
|---|---|---|---|
| RF01 | C | Criação de OS recebe cliente/veículo; domínio usa DocumentoFiscal; testes unitários e PostgreSQL cobrem agregados. | Repetir execução no commit final. |
| RF02 | C | CRUD `/api/clientes`, protegido por política administrativa; persistência EF. | Demonstrar e guardar saída associada ao commit final. |
| RF03 | C | CRUD `/api/veiculos`; VO Placa e constraint única de placa. | Demonstrar validação e unicidade. |
| RF04 | C | CRUD `/api/servicos`; serviços são itens do orçamento/OS e preservam snapshot. | Evidência de ponta a ponta deve ser gravada. |
| RF05 | P | CRUD `/api/pecas`, entrada e consulta de estoque, reserva/baixa e testes de fluxo crítico. | Não há entidade separada `Insumo`; confirmar se `Peca` satisfaz o enunciado. CRUD de ajuste/saída não foi evidenciado além do fluxo operacional. |
| RF06 | C | OS aceita serviços/peças usados na composição do orçamento; testes de domínio e fluxo PostgreSQL. | Demonstrar inclusão por API no vídeo. |
| RF07 | C | Orçamento versionado calculado a partir de itens/snapshots; total persistido e testado. | Confirmar cenário demonstrado no commit final. |
| RF08 | C | Orçamento é disponibilizado por fluxo/API para decisão; sem provedor de envio externo, que não é exigido pelo texto (“enviar ou disponibilizar”). | Descrever como disponibilização pela API. |
| RF09 | P | API registra aprovação/rejeição e histórico; aprovação pode reservar estoque. | Ambos endpoints estão protegidos como administrativos. Não existe decisão autenticada pelo cliente; validar ator esperado pelo grupo/FIAP. |
| RF10 | C | Domínio representa Recebida, EmDiagnostico, AguardandoAprovacao, EmExecucao, Finalizada e Entregue. | Evidência da demonstração cobrindo transições. |
| RF11 | C | Transições e invariantes residem no agregado de OS; testes unitários cobrem estados inválidos. | Repetir a suíte final. |
| RF12 | P | Token de cliente tem escopo de uma OS; consulta verifica claim `ordem_id`. | Não foi comprovado teste automatizado de tentativa de consultar outra OS; decisão do orçamento pelo cliente também ausente. |
| RF13 | C | Endpoints administrativos listam/detalham OS e exigem JWT administrador. | Demonstrar autorização e respostas na gravação. |
| RF14 | P | Endpoint administrativo de tempo médio de execução implementado. | Regra de agregação/granularidade ainda não está suficientemente determinada pelo requisito; demonstrar resultado e explicitar interpretação adotada. |

## Requisitos técnicos

| ID | Status | Evidência observada | Lacuna ou ação restante |
|---|---|---|---|
| RT01 | C | Solution separa Api/Application/Domain/Infrastructure; arquitetura e referências revisadas. | Confirmar diagrama/reflexo final depois do commit. |
| RT02 | C | PostgreSQL e Npgsql configurados; justificativa documentada no README/ADR. | — |
| RT03 | C | Swagger e `/openapi/v1.json` disponíveis no ambiente Development do Compose. | Não publicar a configuração Development como produção. |
| RT04 | C | Rotas administrativas usam política JWT; testes de autenticação e execução API anterior confirmaram 401 sem token e acesso autenticado. | Anexar saída/vídeo no commit final sem revelar token. |
| RT05 | C | DocumentoFiscal e Placa no Domain com testes unitários. | — |
| RT06 | C | 63 testes conhecidos: 48 Domain + 15 Integration, incluindo PostgreSQL real conforme execução reportada pelo grupo. | Reexecutar após alterações técnicas ou no commit de entrega. |
| RT07 | P | Relatório versionado informa Domain: 96,00% linhas e 85,39% branches. | Escopo é apenas Domain; não prova cobertura ≥80% de todos os domínios críticos/camadas. Delimitar e produzir evidência final verificável. |
| RT08 | C | Dockerfile e Compose presentes; API/PostgreSQL executados previamente pelo grupo. | Repetir `docker compose up --build` no ambiente final. |
| RT09 | C | README atualizado com arquitetura, configuração, endpoints, migrations e testes. | Validar comandos no ambiente limpo/commit final. |
| RT10 | C | Transações no fluxo de orçamento/estoque; testes PostgreSQL de rollback pós-save e concorrência com dois DbContexts. | Corridas multi-peça e todas as interleavings não estão comprovadas. |
| RT11 | P | `dotnet list ... --vulnerable --include-transitive` reportou zero pacotes vulneráveis nos seis projetos em 2026-11-03. Revisão de código e busca heurística documentadas. | Scanner de imagem não executado: tentativa Docker Scout bloqueada pela revisão automática, pois pode enviar metadados/SBOM a serviço externo. Sem scanner dedicado de segredos. Consultar relatório. |

## Entregáveis transversais

| Entregável | Status | Evidência/ação restante |
|---|---|---|
| README | C | README versionado; validar comandos e links no commit final. |
| DDD e diagramas versionados | P | Modelagem e Mermaid presentes e alinhados ao código auditado. Ainda falta validar com grupo as fronteiras, linguagem ubíqua e decisões de workshop. |
| Workshop Event Storming | N | Diagramas Mermaid são derivados do código, não comprovam sessão, participantes, data ou decisões colaborativas. Realizar/registrar e anexar links/exports públicos. |
| Exports PDF/imagem em `docs/ddd/` | N | Não comprovados nesta auditoria; gerar a partir dos boards finais e versionar. |
| Análise de vulnerabilidades | P | Evidência NuGet e revisão registrada. CVE de imagem e scanner dedicado ainda pendentes. |
| Repositório público, branch protegida e PR | N | Estado remoto/permissões não comprovados localmente; verificar no GitHub. |
| Branch/hash final | N | Informar no PDF e confirmar que vídeo, relatório e código correspondem ao mesmo commit. |
| Vídeo público ≤15 min | N | Roteiro de 9 min preparado; gravar, publicar e conferir acesso/link. |
| PDF único FIAP | N | Preparar identificação, grupo, RMs, Discord, stack, links clicáveis, branch/hash e evidências. |
| Repositório sem segredos | P | `.env` ignorado e busca heurística sem ocorrência óbvia; executar scanner dedicado e revisar histórico antes de tornar público. |

## Lacunas que exigem decisão ou evidência

1. Validar se aprovação/rejeição deve ser feita pelo cliente ou pelo administrador; a API atual oferece apenas decisão administrativa.
2. Confirmar se o modelo `Peca` cobre o termo “insumo” ou se a FIAP espera cadastro distinto.
3. Criar teste de autorização cruzada para consulta de OS com token de outra OS.
4. Definir/documentar interpretação do tempo médio do requisito RF14.
5. Completar cobertura do domínio crítico no escopo aprovado; o relatório atual é somente Domain.
6. Concluir análise de CVEs da imagem com ferramenta e política aprovadas; Docker Scout não foi executado por bloqueio automático relacionado à possível transmissão externa de metadados/SBOM.
7. Produzir evidências externas: board/exports reais de Event Storming, vídeo e PDF final; verificar publicação e correspondência com o commit entregue.
