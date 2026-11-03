# Requisitos oficiais — Tech Challenge SOAT, Fase 1

**Fonte:** PDF fornecido pelo aluno, `docs/ddd/referencias/Fase 1 - Tech Challenge.pdf`, páginas 2–11. O arquivo foi conferido na Etapa 0; esta página é uma transcrição estruturada/paráfrase e não substitui o enunciado. A localização é diferente de `docs/referencias/`, caminho indicado no pedido. O desafio vale **90% da nota da fase** e é inicialmente previsto para grupo (p. 2).

## Objetivo e escopo (p. 2)
MVP do **back-end monolítico** de sistema de oficina mecânica, com gestão de OS, clientes, veículos, serviços, peças e insumos, usando DDD, qualidade e segurança. **Não há requisito de front-end na Fase 1**.

## Requisitos funcionais
| ID | Requisito | Fonte |
|---|---|---|
| RF01 | Identificar cliente por CPF/CNPJ na criação de OS | p. 3, 9 |
| RF02 | CRUD de clientes | p. 3, 10 |
| RF03 | CRUD de veículos (placa, marca, modelo, ano) | p. 3, 9–10 |
| RF04 | CRUD de serviços e inclusão dos serviços solicitados na OS | p. 3, 9–10 |
| RF05 | CRUD de peças/insumos e controle de estoque | p. 3, 9–10 |
| RF06 | Incluir peças e insumos necessários na OS | p. 3, 9 |
| RF07 | Gerar orçamento automaticamente a partir de serviços e peças | p. 3, 9 |
| RF08 | Enviar ou disponibilizar orçamento para aprovação | p. 3, 9 |
| RF09 | Registrar aprovação **ou rejeição** do orçamento | p. 9 |
| RF10 | OS com status: Recebida, Em diagnóstico, Aguardando aprovação, Em execução, Finalizada, Entregue | p. 3, 9–10 |
| RF11 | Controlar transições de status conforme ações e regras | p. 3, 10 |
| RF12 | Cliente consulta andamento da OS via API | p. 3, 10 |
| RF13 | Listar e detalhar ordens de serviço | p. 3, 10 |
| RF14 | Monitorar ou calcular tempo médio de execução dos serviços | p. 4, 10 |

## Requisitos técnicos e qualidade (p. 4, 10–11)
- RT01: back-end monolítico organizado em camadas.
- RT02: banco de dados de livre escolha **com justificativa**.
- RT03: API RESTful documentada via Swagger/OpenAPI ou equivalente.
- RT04: autenticação JWT para APIs administrativas.
- RT05: validação de CPF/CNPJ e placa.
- RT06: testes unitários e de integração dos fluxos principais.
- RT07: evidência de cobertura **mínima de 80% nos domínios críticos**.
- RT08: Dockerfile e `docker-compose.yml` para ambiente completo.
- RT09: execução local simples e README explicativo.
- RT10: tratamento adequado das transações nos fluxos principais.
- RT11: relatório ou resultado de análise de vulnerabilidades no repositório.

## Entregáveis e critérios de apresentação (p. 4–8)
- **Um PDF final** com identificação: curso SOAT, fase 1, grupo, nomes completos, RMs, usernames Discord, linguagem e tecnologias.
- Links completos e clicáveis de **todos** os repositórios, públicos no GitHub durante correção/revisão; informar **branch e hash do commit** final; versão deve corresponder ao vídeo/documentação.
- Repositório sem segredos, com README completo, testes, cobertura, documentação de API, análise de vulnerabilidades.
- Branch `main` protegida; mudanças nela somente via Pull Request.
- **Vídeo público no YouTube de até 15 minutos** mostrando execução real, arquitetura/camadas, justificativa do banco, Event Storming, DDD, criação/acompanhamento de OS, peças/serviços, orçamento/aprovação, status, JWT, Swagger, testes, análise de vulnerabilidades, participação/identificação do grupo.
- Documentação DDD acessível por link público (Miro ou equivalente), **exportada também no repositório** em `docs/ddd/`: Event Storming de criação da OS, acompanhamento da OS e gestão de peças/insumos; eventos, comandos, políticas, atores, agregados, entidades, objetos de valor, bounded contexts quando identificados, diagramas pedidos na disciplina, linguagem ubíqua, glossário e vínculo com o código.
- Diagramas e documentação devem refletir a **solução efetivamente entregue**, não exemplos genéricos.

## Conferência e questões que o PDF não define
O PDF confirma os requisitos listados acima. Também menciona acompanhar o andamento em tempo real e autorizar reparos adicionais via aplicativo como contexto do desafio. A entrega desta fase é um back-end, sem requisito de front-end ou de provedor de notificação; a aprovação/rejeição do orçamento e a consulta de OS pela API não definem a interface ou o mecanismo de identidade.

- Sem definição de provedor de envio do orçamento; o PDF aceita **envio ou disponibilização**.
- Não determina comportamento após rejeição do orçamento, nem se uma OS pode ter múltiplas versões de orçamento.
- Não define em que momento reservar/baixar estoque ou como tratar estoque insuficiente.
- Não determina mecanismo de identificação/autorização do cliente que consulta OS.
- Não determina política de preço histórico, cancelamento, impostos, descontos ou permissões administrativas granulares.
Esses pontos precisam de decisões explícitas, sem atribuí-las ao enunciado.
