# Análise de vulnerabilidades e riscos — Fase 1

**Verificação registrada:** 3 de novembro de 2026. **Escopo:** dependências NuGet restauradas e revisão direcionada da API e da configuração do repositório. Os resultados abaixo são evidências pontuais e não constituem certificação, análise dinâmica ou garantia de ausência de vulnerabilidades.

## Verificações realizadas

| Verificação | Resultado | Limitação |
|---|---|---|
| `dotnet list Oficina.slnx package --vulnerable --include-transitive` | Os seis projetos não apresentaram pacotes com advisories conhecidos no feed NuGet consultado nessa data. | O resultado depende dos advisories e das versões restauradas no momento da execução. |
| Busca local por padrões de segredos | A busca heurística não encontrou tokens JWT, blocos de chave privada ou senhas evidentes; não havia arquivo `.env` rastreado. | Não foi usado scanner dedicado. A busca não comprova ausência em todo o histórico, branches, artefatos ou valores codificados. |
| `git diff --check` | Sem erros de whitespace nos diffs revisados. | Não é ferramenta de segurança. |
| CVEs da imagem Docker | Não há resultado registrado. | Análise de imagem não foi executada. |
| Gitleaks, Trivy e Grype | Não estavam disponíveis como executáveis nesta máquina durante a verificação registrada. | Nenhum resultado desses scanners foi obtido. |

## Controles observados

- Rotas administrativas usam política JWT. Chave e credenciais são configuradas por variáveis de ambiente no Compose.
- A configuração de autenticação valida o tamanho mínimo da chave de assinatura e usa issuer, audience e expiração. O token de consulta inclui o identificador da OS e a consulta compara esse claim com o ID solicitado.
- Portas do Compose são publicadas em loopback. O Compose define `Development` e HTTP para uso local; essa configuração não é apropriada para exposição pública.
- `/health` verifica o processo HTTP e não confirma disponibilidade do banco.
- Exceções não tratadas recebem resposta genérica e conflitos de concorrência são mapeados para HTTP 409.
- Migrations não são aplicadas automaticamente na inicialização da API.

## Riscos e limitações

| Risco | Evidência atual | Consideração |
|---|---|---|
| Tentativas repetidas de autenticação | Não foi identificada configuração de rate limiting ou lockout nos endpoints de autenticação. | Definir controles contra força bruta antes de exposição pública. |
| Credenciais administrativas por ambiente | Compose configura um único administrador local. | Usar gestor de segredos, rotação e processo de provisão em implantação real. |
| HTTP e perfil Development no Compose | Configuração voltada à execução local em loopback. | Usar HTTPS e configuração própria de produção antes de publicar o serviço. |
| Decisão de orçamento por administrador | As rotas de aprovação/rejeição não autenticam o cliente como autor da decisão. | Validar o ator exigido pelo requisito com o grupo; não apresentar o fluxo administrativo como aprovação do cliente. |
| Isolamento entre OS no token do cliente | O código compara claim e rota; não foi localizado teste automatizado de consulta cruzada. | Acrescentar e executar cenário de integração com duas OS. |
| Health check sem dependência do banco | `/health` indica estado do processo, não readiness PostgreSQL. | Criar health check relacional se o requisito operacional pedir. |
| Imagem e segredos sem scanner dedicado | Não há resultado de CVE da imagem nem de scanner dedicado de segredos. | Executar ferramentas aprovadas e registrar versão, data e saída. |
| Concorrência multi-item | Testes cobrem fluxos críticos PostgreSQL e conflito otimista, mas não todas as interleavings entre múltiplas peças. | Ampliar cenários se o risco e o aceite da entrega exigirem. |

## Evidências ainda ausentes

Não há resultado atual de análise CVE da imagem nem de scanner dedicado de segredos. A verificação NuGet registrada corresponde a 3 de novembro de 2026. Também não há evidência de configuração de produção, HTTPS público ou política de publicação do Swagger; o Compose documentado é voltado à execução local.
