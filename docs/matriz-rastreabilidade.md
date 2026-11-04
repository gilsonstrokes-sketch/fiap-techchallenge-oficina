# Matriz de rastreabilidade — Fase 1

Esta matriz relaciona os requisitos do [enunciado FIAP](requisitos-fiap.md) à implementação e às evidências disponíveis no repositório. O status descreve o que está documentado e verificável nesta cópia; itens externos dependem das evidências do grupo.

## Legenda

- **Concluído (C):** implementação e evidência compatíveis com o escopo indicado.
- **Parcial (P):** há implementação, com lacuna de comportamento, teste ou validação.
- **Pendente/não comprovado (N):** não há evidência suficiente no repositório ou nesta execução.

## Requisitos funcionais

| ID | Status | Implementação/evidência | Lacuna ou validação restante |
|---|---|---|---|
| RF01 | C | Criação de OS associa cliente e veículo; o domínio valida DocumentoFiscal. Testes cobrem o agregado e persistência. | Repetir a validação PostgreSQL no commit de entrega. |
| RF02 | C | CRUD de clientes em `/api/clientes`, protegido por política administrativa e persistido por EF Core. | Demonstrar no vídeo. |
| RF03 | C | CRUD de veículos; VO `Placa` e constraint única no banco. | Demonstrar validação e unicidade. |
| RF04 | C | CRUD de serviços e inclusão em orçamento com snapshot de dados. | Registrar fluxo ponta a ponta na demonstração. |
| RF05 | P | Peças possuem catálogo, saldo, entrada, reserva e baixa operacional. | Não existe cadastro separado de `Insumo`; validar se `Peca` atende ao enunciado. Não foi comprovado CRUD independente de ajuste/saída. |
| RF06 | C | OS e orçamento aceitam serviços e peças; testes de domínio e fluxo PostgreSQL. | Demonstrar inclusão pela API. |
| RF07 | C | Orçamentos versionados, com itens, snapshots e total persistido. | Repetir no commit de entrega. |
| RF08 | C | O orçamento fica disponível pela API para consulta e decisão. Não há integração de envio externo. | Explicitar a disponibilização pela API na apresentação. |
| RF09 | P | API registra aprovação/rejeição e histórico. | As rotas de decisão são administrativas; não há decisão autenticada pelo cliente. Confirmar o ator esperado com o grupo. |
| RF10 | C | Domínio implementa `Recebida`, `EmDiagnostico`, `AguardandoAprovacao`, `EmExecucao`, `Finalizada` e `Entregue`. | Demonstrar transições na apresentação. |
| RF11 | C | Agregado de OS mantém transições e invariantes; testes unitários cobrem estados inválidos. | Repetir a suíte no commit de entrega. |
| RF12 | P | Token de consulta do cliente é limitado por `ordem_id` e comparado com a OS da rota. | Não foi localizado teste automatizado de tentativa de acesso cruzado a outra OS. |
| RF13 | C | Endpoints administrativos de consulta/listagem de OS exigem JWT de administrador. | Demonstrar autorização. |
| RF14 | P | Endpoint administrativo de tempo médio de execução implementado. | A granularidade do cálculo ainda requer validação pelo grupo. |

## Requisitos técnicos

| ID | Status | Implementação/evidência | Lacuna ou validação restante |
|---|---|---|---|
| RT01 | C | Solution separada em Api, Application, Domain e Infrastructure; Domain não depende de frameworks de infraestrutura. | Manter documentação coerente com o código final. |
| RT02 | C | PostgreSQL/Npgsql configurados e justificados no README e ADR. | — |
| RT03 | C | Swagger e OpenAPI disponíveis no perfil Development; OpenAPI declara esquema Bearer e requisitos nas operações protegidas. | Não publicar o perfil Development diretamente em produção. |
| RT04 | C | Rotas administrativas protegidas por JWT; testes cobrem autorização. | Associar evidência da execução final ao commit entregue. |
| RT05 | C | VOs `DocumentoFiscal` e `Placa` possuem validação e testes. | — |
| RT06 | P | A suíte atual tem 64 testes. Nesta revisão, `OFICINA_TEST_POSTGRES` não está configurada; os testes PostgreSQL serão ignorados. A execução anterior reportada com banco real ocorreu antes do teste OpenAPI mais recente. | Reexecutar a suíte atual com PostgreSQL real e registrar o resultado. |
| RT07 | P | Relatório versionado registra 96,00% de linhas e 85,39% de branches em `Oficina.Domain`. | O relatório não mede todas as camadas nem comprova 80% para todo o escopo de domínios críticos. |
| RT08 | C | Dockerfile e Compose versionados; execução de API/PostgreSQL foi validada anteriormente pelo grupo. | Repetir no ambiente final de gravação. |
| RT09 | C | README descreve arquitetura, configuração, migrations, JWT, rotas e testes. | Confirmar comandos no ambiente de entrega. |
| RT10 | P | Fluxos PostgreSQL cobrem transação, rollback após falha e conflito otimista com dois DbContexts. | A suíte atual com PostgreSQL precisa ser repetida; combinações concorrentes multi-peça não estão exauridas. |
| RT11 | P | Verificação NuGet registrada em 2026-11-03 não encontrou advisories nos seis projetos. Busca de segredos foi heurística. | Scanner de imagem e scanner dedicado de segredos não foram executados; revisar histórico antes de publicação. |

## Entregáveis

| Entregável | Status | Evidência ou pendência |
|---|---|---|
| README | C | Documenta execução e arquitetura da aplicação. |
| Modelagem DDD e Mermaid | P | Modelagem e quatro diagramas versionados; são derivados do código e precisam de validação do grupo. |
| Sessões de Event Storming | N | Não há atas, participantes, datas ou registro de workshop no repositório. |
| Boards e exports da disciplina | N | Links públicos e exports aprovados não estão comprovados. |
| Relatório de segurança | P | Verificação NuGet registrada; CVEs da imagem e scanner dedicado de segredos pendentes. |
| Vídeo | N | Roteiro operacional disponível; gravação e link público pendentes. |
| PDF final | N | Ainda requer identificação do grupo, links, branch/hash e evidências. |
| Publicação e proteção GitHub | N | Acesso público, proteção de `main` e fluxo de PR dependem de verificação no GitHub. |
| Identificação da versão entregue | N | Registrar branch e hash correspondentes ao PDF, vídeo, testes e código publicado. |
| Ausência de segredos | P | Nenhum `.env` está rastreado; a busca local foi heurística e não cobre todo o histórico Git. |

## Pendências de validação

1. Confirmar com o grupo se `Peca` cobre o conceito de insumo do enunciado.
2. Confirmar o ator esperado para aprovar ou rejeitar orçamento; a implementação atual exige administrador.
3. Adicionar ou apresentar evidência de isolamento entre OS para o token de consulta do cliente.
4. Validar a interpretação do indicador de tempo médio.
5. Executar a suíte atual com `OFICINA_TEST_POSTGRES` apontando para uma base dedicada e registrar o resultado.
6. Completar a análise CVE da imagem e a varredura dedicada de segredos com ferramentas aprovadas.
7. Anexar evidências reais das sessões/boards, vídeo, PDF e publicação final.
