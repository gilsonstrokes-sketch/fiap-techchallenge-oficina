# Análise de vulnerabilidades e riscos — Fase 1

**Data da verificação:** 2026-11-03. **Escopo:** dependências NuGet restauradas e revisão estática direcionada do repositório/API. Este documento registra os comandos e limitações observáveis; não é certificação de segurança nem substitui análise de imagem, DAST, revisão independente ou reexecução no commit de entrega.

## Evidências executadas

| Verificação | Resultado | Limite |
|---|---|---|
| `dotnet list Oficina.slnx package --vulnerable --include-transitive` | Executada com acesso ao feed NuGet oficial em 2026-11-03. Os seis projetos reportaram **nenhum pacote vulnerável conhecido** no feed consultado. | Resultado depende do advisory/feed naquele instante e dos pacotes restaurados; reexecutar no commit final. |
| Busca heurística no conteúdo versionado por padrões óbvios de token JWT, bloco de chave privada e senha literal longa | Nenhum padrão óbvio encontrado; nenhum `.env` rastreado foi identificado. | Busca heurística, não detector de segredos dedicado; não prova ausência de credenciais em todo histórico, branches, artefatos ou formatos codificados. |
| `git diff --check` | Sem erros de whitespace nos diffs revisados. | Não é scanner de segurança. |
| Docker Scout | CLI local `docker scout version` disponível: 1.27.0. A tentativa de `docker scout cves local://fiap-techchallenge-oficina-api --only-severity critical,high,medium,low` foi bloqueada pela revisão automática antes de executar. | A análise pode enviar metadados da imagem, pacotes ou SBOM local a um serviço externo. Nenhum resultado de CVE da imagem foi obtido. É necessária autorização explícita para essa transferência. |
| Trivy, Grype e Gitleaks | Executáveis não disponíveis neste computador durante a checagem. | Nenhuma análise foi atribuída a essas ferramentas. |

## Controles observados no código e configuração

- As rotas administrativas usam política JWT de administrador; segredo e credenciais vêm de variáveis de ambiente no Compose. Não há segredos reais no exemplo versionado.
- A chave de assinatura é validada por tamanho mínimo configurado; tokens incluem emissor/audiência/expiração. Token de consulta de cliente é vinculado à OS e a rota compara o ID com o claim `ordem_id`.
- A consulta de saúde é local e o Compose publica portas em loopback. Swagger/OpenAPI é habilitado no ambiente `Development` definido pelo Compose; não publicar essa configuração de desenvolvimento diretamente na internet.
- O middleware retorna erros HTTP genéricos para falhas não previstas e mapeia conflito de concorrência para `409`.
- Migrations não são aplicadas implicitamente no startup. A implantação precisa aplicar migrations com credenciais controladas.

## Riscos e limitações que permanecem

| Risco | Estado/evidência | Ação antes de produção |
|---|---|---|
| Sem limitação de tentativas/lockout observada no fluxo de autenticação | Não foi identificada política de rate limiting ou lockout nos endpoints de autenticação. | Definir proteção contra força bruta e monitoramento antes de exposição pública. |
| Credenciais de administrador configuradas por ambiente | Adequado para execução local; Compose configura um administrador único. | Usar gestor de segredos no ambiente implantado, rotação e processo de provisão; não reutilizar credenciais locais. |
| API do Compose usa `Development` e HTTP | Adequado para validação local em loopback, não para tráfego público. | Usar HTTPS, configuração de produção e política de publicação de Swagger conforme ambiente. |
| Endpoint de decisão do orçamento exige administrador | Consulta do cliente é limitada à OS, mas o cliente não aprova/rejeita via endpoint próprio. | Validar ator e requisito com o grupo. Não demonstrar como autorização de cliente algo que a API não oferece. |
| Teste de isolamento de consulta entre OS não comprovado | O código compara claim e rota, mas não foi localizado teste automatizado de tentativa cruzada. | Adicionar cenário de integração com duas OS e comprovar resposta negada antes da entrega/produção. |
| Health check não testa o banco | `/health` comprova processo HTTP, não readiness relacional. | Se requisito operacional exigir, criar health check de dependência explicitamente; não interpretar o atual como verificação do banco. |
| Varredura da imagem e scanner dedicado de segredos ausentes | Docker Scout bloqueado por revisão automática devido à possível transferência de metadados/SBOM; Trivy/Grype/Gitleaks ausentes. | Obter autorização para o serviço externo ou executar ferramenta aprovada em ambiente controlado e anexar saída integral. |
| Concorrência multi-item não exaurida | Testes existentes cobrem reserva/baixa crítica e conflito otimista em cenário PostgreSQL; não todas as interleavings multi-peça. | Expandir matriz de corridas se o risco/aceite exigir; evidência atual não cobre toda combinação. |

## Reexecução no commit final

1. Executar `dotnet list Oficina.slnx package --vulnerable --include-transitive` com fonte NuGet confiável habilitada.
2. Executar scanner de segredos aprovado sobre working tree e histórico relevante, sem publicar resultados que contenham segredos.
3. Fazer análise de CVEs da imagem final e registrar ferramenta, data, digest da imagem e severidades. Esta análise não foi concluída nesta verificação.
4. Revisar configuração efetiva do ambiente de demonstração, permissões, HTTPS e ausência de segredos nos artefatos públicos.

Não foram feitas alterações de regra de negócio nem foi removida qualquer constraint para melhorar resultados desta análise.
