# Checklist final FIAP — Fase 1

Atualizado em 2026-11-03 após auditoria do código/documentos disponíveis. **C = concluído com evidência observada; P = parcial ou depende de validação; N = pendente/não comprovado.** Revalidar todos os itens no commit público final. Veja a [matriz de rastreabilidade](matriz-rastreabilidade.md) para detalhe por requisito.

## Produto

- [C] CRUD de clientes, veículos, serviços e peças; fluxo de estoque implementado.
- [P] O desafio menciona peças/insumos; não há cadastro separado de `Insumo`. Grupo confirma se `Peca` cobre ambos.
- [C] Validação CPF/CNPJ e placa no domínio/testes.
- [C] Criar/listar/detalhar OS, adicionar serviços/peças, orçamento versionado e histórico.
- [C] Seis status e transições controladas no agregado.
- [P] Aprovação/rejeição existe, mas somente por administrador; confirmar ator esperado. Cliente recebe token restrito para consulta, sem comando de decisão.
- [P] Consulta por cliente verifica escopo da OS no JWT; falta evidência de teste automatizado de acesso cruzado.
- [C] Estoque com reserva, baixa, transação, rollback e conflito de concorrência demonstrados por testes PostgreSQL reportados.
- [P] Indicador de tempo médio implementado; granularidade/interpretação precisa ser documentada e validada pelo grupo.

## Técnica e segurança

- [C] Monólito em camadas e PostgreSQL justificado em documentação/ADR.
- [C] REST com Swagger/OpenAPI no perfil Development.
- [C] Dockerfile/Compose; execução API/PostgreSQL reportada pelo grupo.
- [C] 63 testes conhecidos aprovados (48 Domain + 15 Integration); repetir após alteração técnica/no commit final.
- [P] Cobertura: relatório Domain 96,00% linhas/85,39% branches; não prova cobertura de todas as camadas/domínios críticos.
- [P] NuGet: zero vulnerabilidades conhecidas no feed consultado em 2026-11-03. Análise CVE da imagem não foi concluída; scanner dedicado de segredos não executado.
- [P] Revisar rate limiting/lockout, HTTPS/configuração não-Development e health check de dependências antes de exposição pública.
- [C] README atualizado com configuração, migrations, JWT, rotas e testes; testar comandos em ambiente limpo.
- [P] Busca heurística local não encontrou padrão óbvio de segredo; executar scanner dedicado e revisar histórico antes de publicar.

## DDD e documentação

- [P] Modelagem DDD derivada do código: contextos candidatos, agregados, entidades, VOs, invariantes e decisões descritos. Grupo deve validar linguagem/fronteiras.
- [P] Quatro diagramas Mermaid versionados: abertura/diagnóstico, orçamento/decisão, acompanhamento e estoque. São diagramas derivados do código.
- [N] Comprovação das sessões/workshops reais, participantes, data e decisões de Event Storming.
- [N] Link público dos boards e exports PDF/imagem versionados em `docs/ddd/`.
- [C] Matriz de rastreabilidade e roteiro de demonstração versionados.

## Publicação e avaliação

- [N] Repositório público durante correção/revisão; conferir link e acesso.
- [N] `main` protegida e fluxo por Pull Request; conferir configuração remota.
- [N] Branch e hash/tag final registrados no PDF.
- [P] Segredos: nenhum padrão óbvio na busca heurística; falta scanner dedicado e revisão do histórico.
- [N] Vídeo público no YouTube (≤15 min), execução real dos fluxos, arquitetura/DDD, JWT, Swagger, testes e segurança. Roteiro disponível em [`roteiro-demonstracao.md`](roteiro-demonstracao.md).
- [N] PDF único com curso/fase, grupo, nomes completos, RMs, Discord, stack, links completos/clicáveis e identificação do commit.
- [N] Confirmar que código, PDF, board/export, saída dos testes e vídeo correspondem à mesma versão.

## Próximas ações de fechamento

1. Resolver com o grupo as lacunas RF05, RF09, RF12 e RF14; alterar código somente se a decisão/requisito exigir.
2. Executar scanner de CVEs da imagem aprovado e scanner de segredos, registrando ferramenta/data/resultado.
3. Repetir build/testes, incluindo PostgreSQL real, no commit que será publicado.
4. Realizar/validar workshops e publicar exports/links DDD.
5. Gravar e publicar o vídeo, preparar o PDF, revisar acesso e registrar branch/hash finais.
