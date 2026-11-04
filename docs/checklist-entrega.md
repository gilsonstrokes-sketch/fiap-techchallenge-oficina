# Checklist de entrega — Fase 1

Estado documental em 3 de novembro de 2026. **C** indica evidência disponível; **P**, implementação parcial ou validação pendente; **N**, entrega sem comprovação no repositório. A [matriz de rastreabilidade](matriz-rastreabilidade.md) detalha os requisitos.

## Produto

- [C] CRUD de clientes, veículos, serviços e peças; fluxo de estoque implementado.
- [P] Não há cadastro separado de `Insumo`; validar se `Peca` atende ao enunciado.
- [C] Validação de documento fiscal e placa no domínio e em testes.
- [C] Criação e consulta de OS, diagnóstico, orçamento versionado e histórico.
- [C] Seis estados de OS e transições controladas no agregado.
- [P] Aprovação e rejeição de orçamento são administrativas; não há decisão por endpoint de cliente.
- [P] Token de cliente restringe a consulta a uma OS; falta evidência automatizada de acesso cruzado.
- [P] Reserva, baixa, transação, rollback e concorrência foram cobertos em testes PostgreSQL anteriores; revalidar a versão atual.
- [P] Indicador de tempo médio implementado; interpretação da granularidade precisa ser validada.

## Arquitetura, execução e segurança

- [C] Monólito em camadas; PostgreSQL e EF Core documentados.
- [C] API REST, Swagger/OpenAPI e esquema Bearer para endpoints protegidos.
- [C] Dockerfile e Compose versionados; execução local validada anteriormente.
- [P] Suíte atual: 64 testes. A execução desta revisão não dispõe de `OFICINA_TEST_POSTGRES`; reexecutar para validar os cenários PostgreSQL.
- [P] Cobertura registrada somente para Domain: 96,00% de linhas e 85,39% de branches.
- [P] Verificação NuGet de 3 de novembro de 2026 não encontrou advisories; CVEs da imagem e scanner dedicado de segredos pendentes.
- [P] A API de autenticação não tem rate limiting/lockout comprovado; Compose usa HTTP e `Development`, adequado à demonstração local, não a exposição pública.
- [C] README documenta configuração, migrations, JWT, endpoints e testes.
- [P] Nenhum arquivo `.env` está rastreado; busca de segredos local foi heurística e não cobre todo o histórico Git.

## DDD e documentação

- [P] Modelagem DDD, agregados, invariantes e decisões documentados; fronteiras e linguagem precisam de validação do grupo.
- [P] Quatro diagramas Mermaid representam fluxos do código atual.
- [N] Evidências das sessões de Event Storming, participantes e decisões colaborativas.
- [N] Links públicos dos boards e exports exigidos pela disciplina.
- [C] Matriz de rastreabilidade e roteiros de demonstração versionados.

## Materiais de entrega e publicação

- [N] Vídeo público de até 15 minutos; roteiro operacional em [`roteiro-gravacao-passo-a-passo.md`](roteiro-gravacao-passo-a-passo.md).
- [N] PDF único com identificação do grupo, stack, links e hash da versão entregue.
- [N] Repositório público, branch protegida e fluxo por Pull Request; configuração remota não verificada localmente.
- [N] Branch/hash finais ainda não associados ao PDF, vídeo, boards e saída da suíte.
- [P] O PDF original da FIAP está rastreado no repositório e no histórico Git. A redistribuição pública depende de autorização da FIAP.

## Fechamento

1. Resolver as lacunas RF05, RF09, RF12 e RF14 com o grupo.
2. Executar a suíte atual com PostgreSQL real em uma base dedicada e registrar a saída.
3. Concluir análise de imagem e scanner de segredos com ferramentas aprovadas.
4. Validar e registrar workshops/boards/exports reais.
5. Gravar e publicar o vídeo; preparar o PDF e verificar que todos os artefatos referenciam o mesmo hash.
