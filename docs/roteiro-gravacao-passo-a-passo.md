# Roteiro operacional de gravação — Tech Challenge FIAP, Fase 1

**Duração estimada:** 10 min 40 s, com margem para pausas curtas. **Escopo:** execução real da API .NET 10 e PostgreSQL já validados pelo grupo. **Dados:** fictícios, gerados para cada gravação e verificados contra os cadastros atuais. **Alterações de banco:** somente inserts e movimentos normais da demonstração; nenhum dado será apagado.

Este roteiro foi conferido com `AGENTS.md`, `README.md`, `docs/roteiro-demonstracao.md`, os Controllers, contratos/DTOs, `OficinaApplicationService`, objetos de valor e respostas OpenAPI observadas em `http://localhost:8080/openapi/v1.json`.

## Antes de iniciar a captura

1. Confirme que Docker Desktop está com `api` e `db` saudáveis e que `/health` responde `Healthy`.
2. Abra estas janelas: repositório GitHub na branch `feature/etapa-3-application`; Docker Desktop; uma janela Swagger autorizada; uma janela anônima/privada Swagger sem JWT; editor com documentação DDD e de segurança.
3. Tenha acesso às credenciais administrativas locais configuradas por `AUTHENTICATION_ADMIN_USERNAME` e `AUTHENTICATION_ADMIN_PASSWORD`. **Não as imprima, não as grave no roteiro e não mostre senha, chave ou JWT completo.**
4. Gere um sufixo para nomear os registros da gravação e escolha um CPF e uma placa livres entre os candidatos abaixo. Faça a consulta de clientes/veículos no Swagger já autenticado antes de gravar; não mostre as listas gerais na captura, pois podem conter dados de terceiros.
5. Faça login e clique **Authorize** na janela principal antes de iniciar a captura. Assim, a resposta contendo o token e o valor digitado no modal não aparecem no vídeo. Deixe essa janela autorizada aberta; use a janela privada somente para demonstrar o `401`.
6. Configure o recorte/desfoque da captura para ocultar o modal **Authorize**, o bloco **Curl** e cabeçalhos da requisição: o Swagger pode exibir `Authorization: Bearer <JWT>` no exemplo gerado após cada chamada protegida. Faça uma chamada de ensaio com a gravação desligada e confira que o token não fica legível. Feche ferramentas de desenvolvedor/Network.
7. Depois de colar o JWT no Swagger, limpe a área de transferência (por exemplo, `Set-Clipboard -Value ''` no PowerShell) e feche o diálogo. Não reabra o modal durante a gravação.
8. Não execute `DELETE`, `docker compose down -v`, limpeza de tabelas ou comandos contra a base principal. Se algum passo falhar, pare e recupere o ambiente; não simule respostas HTTP.

## Preparação de dados — executar antes da gravação

Gere um identificador curto de execução e mantenha-o para os nomes de catálogo:

```powershell
$runId = Get-Date -Format 'yyyyMMdd-HHmmss'
$runId
```

Os CPFs abaixo são válidos pelo algoritmo implementado no VO `DocumentoFiscal`; use o primeiro que não aparecer em `Clientes` → `GET /api/clientes`. Se existir, verifique o próximo. As placas são válidas no formato Mercosul; escolha a primeira ausente em `Veiculos` → `GET /api/veiculos`.

```text
CPFs candidatos: 84620173517, 73164820526, 59283741609
Placas candidatas: QXZ7A84, RJT5B62, PKM3C19
```

Após conferir que estão livres, use os valores concretos selecionados nos JSONs: substitua apenas `<CPF_LIVRE>`, `<PLACA_LIVRE>` e `<RUN_ID>` pelos valores anotados. IDs de cliente, veículo, catálogo e OS serão gerados pelo servidor e copiados das respostas.

## Login administrativo e autorização Bearer no Swagger

Faça este pré-voo **fora da gravação**, pois o endpoint devolve o token e o formulário de login contém senha:

1. Abra `http://localhost:8080/swagger`.
2. Expanda a seção `Auth` → `POST /api/auth/admin` → clique **Try it out**.
3. Preencha o corpo com o usuário/senha locais, execute e copie `accessToken` da resposta sem divulgar. O endpoint responde **200 OK** com `accessToken` e `expiresAt`; a senha não é retornada.

```json
{
  "usuario": "<usuário administrativo configurado localmente>",
  "senha": "<senha administrativa configurada localmente>"
}
```

4. Clique no botão **Authorize** no topo da página. No diálogo do esquema `Bearer`, cole **somente o JWT**, sem escrever `Bearer ` antes; o Swagger acrescenta esse prefixo no cabeçalho HTTP.
5. Clique **Authorize** dentro do diálogo e depois **Close**. O cadeado das operações protegidas deve aparecer fechado/autorizado. Não reabra o diálogo durante a gravação, porque ele pode revelar o token.
6. Antes de gravar, confirme `Clientes` → `GET /api/clientes` → **200 OK** na janela autorizada e **401 Unauthorized** na janela privada. Não clique **Logout** na janela principal durante a gravação.

O OpenAPI associa Bearer apenas a operações com metadado `[Authorize]`; `POST /api/auth/admin` e `POST /api/auth/cliente`, marcados `[AllowAnonymous]`, não exigem JWT. A API continua validando assinatura, issuer, audience, expiração e role. O token administrativo expira em oito horas.

## Sequência e tempos

| # | Etapa | Tempo |
|---:|---|---:|
| 1 | Repositório e arquitetura DDD | 0:55 |
| 2 | Docker Desktop | 0:20 |
| 3 | Swagger, JWT e resposta sem autenticação | 1:00 |
| 4 | Cadastrar e consultar cliente | 0:45 |
| 5 | Cadastrar e consultar veículo | 0:40 |
| 6 | Cadastrar/consultar serviço, peça e estoque | 0:50 |
| 7 | Criar OS | 0:35 |
| 8 | Diagnóstico e orçamento | 1:05 |
| 9 | Aprovação administrativa | 0:30 |
| 10 | Execução, reserva e baixa | 1:05 |
| 11 | Finalização e entrega | 0:40 |
| 12 | Consulta administrativa e indicador | 0:40 |
| 13 | Testes automatizados | 0:45 |
| 14 | Event Storming e segurança | 0:50 |
|  | **Total estimado** | **10:40** |

## 1. Repositório GitHub e arquitetura DDD — 0:55

1. No GitHub, abra o repositório público correto, confirme visualmente a branch `feature/etapa-3-application` e mostre o commit que corresponde ao vídeo. Não mostre página de configurações, variáveis de ambiente ou arquivos secretos.
2. Abra o `README.md` e mostre a stack: .NET 10, ASP.NET Core, EF Core e PostgreSQL.
3. No explorador do repositório, destaque `src/Oficina.Api`, `Application`, `Domain` e `Infrastructure`. Diga que é um monólito em camadas e que o Domain mantém as regras de OS/estoque sem dependência de EF Core ou ASP.NET.
4. Diga que PostgreSQL foi escolhido para persistência relacional, constraints, transações e concorrência otimista. Os limites Cadastro, Catálogo, Atendimento e Estoque são contextos candidatos descritos nos documentos, não microsserviços.

**Destaque visual:** branch/hash, pastas da solution e separação de camadas. **Não diga:** que os bounded contexts foram validados em workshop sem mostrar a evidência do grupo.

## 2. Docker Desktop — 0:20

1. Abra Docker Desktop → **Containers**.
2. Localize o projeto Compose `fiap-techchallenge-oficina` (o nome pode vir do diretório) e mostre os serviços `api` e `db` em execução.
3. Expanda os detalhes apenas o suficiente para mostrar API e PostgreSQL saudáveis. Não abra a seção de ambiente/inspeção que pode revelar variáveis e credenciais.
4. Mostre rapidamente no navegador `http://localhost:8080/health`; a resposta esperada é texto `Healthy` com HTTP `200`.

**Destaque visual:** dois containers ativos e health `Healthy`. O endpoint `/health` mede o processo da API; não o apresente como verificação independente da conexão ao banco.

## 3. Swagger e JWT administrativo — 1:00

1. Na janela privada sem token, abra `http://localhost:8080/swagger`. Na seção `Auth`, expanda `POST /api/auth/admin` e explique que o login foi preparado fora da gravação na janela principal.
2. Mostre a chamada administrativa sem token: seção `Clientes` → `GET /api/clientes` → **Try it out** → **Execute**. Resultado esperado: **401 Unauthorized**.
3. Troque para a janela principal já autorizada. Mostre o cadeado nas operações protegidas e execute a mesma chamada `GET /api/clientes`; resultado esperado: **200 OK**.
4. Explique que a API protege cadastros, OS, estoque e indicadores pela policy `Administrador`, baseada no JWT obtido em `POST /api/auth/admin`. Não abra o diálogo **Authorize** nem mostre o bloco **Curl/cabeçalhos** durante a gravação; eles podem revelar o token. Use o recorte/desfoque preparado no pré-voo e deixe visível o código HTTP e a resposta JSON.

**JSON do login, somente para conferência do contrato (não gravar credenciais):**

```json
{
  "usuario": "<usuário administrativo configurado localmente>",
  "senha": "<senha administrativa configurada localmente>"
}
```

Campos vêm de `LoginAdministradorRequest`. O token do retorno não deve aparecer em vídeo, relatório, terminal compartilhado ou commit.

## 4. Cadastro e consulta de cliente — 0:45

**Swagger:** seção `Clientes`.

1. Expanda `POST /api/clientes` → **Try it out**. Cole este JSON, substituindo apenas os dois valores indicados pelo pré-voo:

```json
{
  "nome": "Cliente Demonstração FIAP <RUN_ID>",
  "documentoFiscal": "<CPF_LIVRE>"
}
```

2. Clique **Execute**. O status real esperado é **201 Created** (o Controller usa `CreatedAtAction`); a resposta traz `id`, `nome`, `documentoFiscal` e `ativo: true`.
3. Copie o campo `id`. Expanda `GET /api/clientes/{id}` → **Try it out**, cole o UUID no parâmetro `id` e clique **Execute**. Não há body. Esperado: **200 OK**, mesmo ID e documento normalizado sem pontuação. Não execute `GET /api/clientes` autenticado na gravação: a lista pode conter dados de outras pessoas.

**Destaque visual:** CPF fictício válido, `ativo: true` e consistência do ID entre criação/consulta. Não deixe a lista geral de clientes na tela.

## 5. Cadastro e consulta de veículo — 0:40

**Swagger:** seção `Veiculos`.

1. Expanda `POST /api/veiculos` → **Try it out**. Cole o JSON completo, trocando os placeholders por `$clienteId` e `$placaDemo` anotados:

```json
{
  "clienteId": "<copie $clienteId da resposta de cliente>",
  "placa": "<placaDemo livre: QXZ7A84, RJT5B62 ou PKM3C19>",
  "marca": "Fiat",
  "modelo": "Argo",
  "ano": 2022
}
```

2. Clique **Execute**. A placa escolhida no pré-voo não aparece em `GET /api/veiculos`. Esperado: **201 Created**; resposta com `id`, `clienteId`, placa normalizada em maiúsculas e `ativo: true`. Copie `id` como `$veiculoId`.
3. Expanda `GET /api/veiculos/{id}` → **Try it out**, cole o UUID copiado no parâmetro `id` e clique **Execute**. Não há body. Esperado: **200 OK** e `clienteId` igual ao cliente criado.

**Destaque visual:** vínculo cliente-veículo, placa no formato Mercosul e ID copiado.

## 6. Cadastro/consulta de serviços, peças e estoque — 0:50

**Swagger:** seções `Servicos`, `Pecas` e `Estoque`.

1. Na seção `Servicos`, expanda `POST /api/servicos` → **Try it out**. Cole o corpo integral, substituindo `<RUN_ID>` pelo valor gerado:

```json
{
  "nome": "Revisão preventiva FIAP <runId do pré-voo>",
  "descricao": "Inspeção demonstrativa de freios e fluidos",
  "preco": 189.90
}
```

Clique **Execute**. Esperado: **201 Created**. Copie `id` como `$servicoId`. Depois expanda `GET /api/servicos/{id}` → **Try it out**, informe o ID e clique **Execute** (sem body); esperado **200 OK**.

2. Na seção `Pecas`, expanda `POST /api/pecas` → **Try it out**. Cole o corpo integral, substituindo `<RUN_ID>`:

```json
{
  "nome": "Filtro demonstrativo FIAP <runId do pré-voo>",
  "descricao": "Peça fictícia utilizada na demonstração",
  "preco": 47.50
}
```

Clique **Execute**. Esperado: **201 Created**; copie `id` como `$pecaId`. Em seguida expanda `GET /api/pecas/{id}` → **Try it out**, informe esse ID e clique **Execute** (sem body); esperado **200 OK**.

3. Para criar saldo suficiente para duas unidades, seção `Estoque` → `POST /api/estoque/pecas/{pecaId}/entradas` → **Try it out**. Path: `$pecaId`. Cole este JSON:

```json
{
  "quantidade": 10
}
```

Clique **Execute**; esperado **200 OK**. Depois expanda `GET /api/estoque/pecas/{pecaId}` → **Try it out**, use o mesmo `$pecaId` e clique **Execute** (sem body). Esperado **200 OK** com `quantidadeFisica: 10`, `quantidadeReservada: 0`, `quantidadeDisponivel: 10`.

**Destaque visual:** catálogo não guarda saldo da peça; o saldo aparece no endpoint próprio de estoque. Não usar DELETE para desfazer os cadastros.

## 7. Criação de ordem de serviço — 0:35

**Swagger:** seção `OrdensDeServico`, `POST /api/ordens-de-servico` → **Try it out**.

JSON completo:

```json
{
  "documentoFiscal": "<mesmo cpfDemo usado no cadastro do cliente>",
  "veiculoId": "<copie $veiculoId da resposta do veículo>"
}
```

Clique **Execute**. Esperado: **201 Created** (`CreatedAtAction`; o OpenAPI descreve resposta `200` para este POST). Copie `id` como `$ordemId`. DTO deve mostrar `status: "Recebida"`, `orcamentoAprovado: false`, `orcamentoAtual: null` e histórico inicial.

**Destaque visual:** criação por CPF e ID do veículo, sem aceitar `clienteId` no pedido; Application resolve o cliente pelo documento e valida vínculo/atividade do veículo.

## 8. Diagnóstico e geração do orçamento — 1:05

### Iniciar diagnóstico

1. Swagger → `OrdensDeServico` → `POST /api/ordens-de-servico/{id}/diagnostico` → **Try it out**. Informe `$ordemId`; esta ação não tem JSON/body.
2. Clique **Execute**; não há body. Esperado: **200 OK**, `status: "EmDiagnostico"`, histórico com ação `IniciarDiagnostico`.

### Gerar e disponibilizar orçamento

1. Swagger → `OrdensDeServico` → `POST /api/ordens-de-servico/{id}/orcamentos` → **Try it out**. Informe `$ordemId` e use este corpo. O enum é numérico no contrato atual: `0 = Servico`, `1 = Peca`.

```json
{
  "itens": [
    {
      "tipo": 0,
      "itemCatalogoId": "<copie $servicoId>",
      "quantidade": 1
    },
    {
      "tipo": 1,
      "itemCatalogoId": "<copie $pecaId>",
      "quantidade": 2
    }
  ]
}
```

2. Substitua os UUIDs no JSON pelos valores copiados como `$servicoId` e `$pecaId`; informe `$ordemId` no parâmetro `id`. Clique **Execute**. Esperado: **200 OK**, `status: "AguardandoAprovacao"`, orçamento versão `1`, dois itens com descrição/preço snapshot e total esperado **284.90** (189.90 + 2 × 47.50). Confirme `orcamentoAtual.total`, `orcamentoAtual.versao` e as linhas.

**Destaque visual:** disponibilizar o orçamento não envia mensagem externa; o orçamento fica persistido para consulta. O preço e a descrição estão nos snapshots da versão.

## 9. Aprovação administrativa do orçamento — 0:30

**Swagger:** `OrdensDeServico` → `POST /api/ordens-de-servico/{id}/orcamentos/aprovacao` → **Try it out**; informe `$ordemId`; sem body.

Informe `$ordemId` no parâmetro `id` e clique **Execute**; não há body. Esperado: **200 OK**, `orcamentoAprovado: true`, uma decisão em `decisoes` com `aprovado: true` e versão `1`. O status continua **`AguardandoAprovacao`**: aprovar não inicia a execução.

**Fala obrigatória:** “Nesta implementação, aprovação e rejeição são registradas por uma rota administrativa protegida. A API ainda não fornece um endpoint para o cliente aprovar; não estamos apresentando esta chamada como aprovação do cliente.”

**Destaque visual:** status permanece aguardando, flag e histórico de decisão mudam; estoque reserva duas peças se houver disponibilidade.

## 10. Iniciar execução, reserva e baixa — 1:05

### Conferir reserva depois da aprovação

Swagger → seção `Estoque` → `GET /api/estoque/pecas/{pecaId}` → **Try it out**; informe `$pecaId` e clique **Execute**. Esperado **200 OK**: `quantidadeFisica: 10`, `quantidadeReservada: 2`, `quantidadeDisponivel: 8`. Se a quantidade não for esta, não prossiga com narração de reserva bem-sucedida; confira os IDs e a resposta da aprovação.

### Iniciar execução e baixar estoque

1. Swagger → `OrdensDeServico` → `POST /api/ordens-de-servico/{id}/execucao/inicio` → **Try it out**; informe `$ordemId`; sem body.
2. Clique **Execute**; não há body. Esperado **200 OK**, `status: "EmExecucao"`, `execucaoIniciadaEm` preenchido. Consulte novamente `GET /api/estoque/pecas/{pecaId}` usando **Try it out** → ID da peça → **Execute**. Esperado **200 OK**: `quantidadeFisica: 8`, `quantidadeReservada: 0`, `quantidadeDisponivel: 8`.

**Explique:** aprovação tenta reservar todas as peças atomicamente; o início confirma a reserva persistida, baixa o saldo e avança a OS na mesma transação. A quantidade não fica negativa. O roteiro usa estoque suficiente; não simule nem force uma falha.

## 11. Finalização e entrega da OS — 0:40

### Finalizar execução

Swagger → `OrdensDeServico` → `POST /api/ordens-de-servico/{id}/execucao/finalizacao` → **Try it out**; path `$ordemId`; sem body.

Clique **Execute**. Esperado **200 OK**, status `Finalizada` e `execucaoFinalizadaEm` preenchido.

### Entregar veículo

Swagger → `OrdensDeServico` → `POST /api/ordens-de-servico/{id}/entrega` → **Try it out**; path `$ordemId`; sem body.

Clique **Execute**. Esperado **200 OK**, status final `Entregue`; histórico contém finalização e entrega.

**Destaque visual:** sequência `EmExecucao` → `Finalizada` → `Entregue`; não há salto direto entre estados.

## 12. Consulta administrativa e indicadores — 0:40

1. Swagger → `OrdensDeServico` → `GET /api/ordens-de-servico/{id}` → **Try it out**. Informe `$ordemId` no parâmetro `id`, clique **Execute**, sem body. Esperado **200 OK** com OS, orçamento atual, status e histórico.
2. Swagger → `Indicadores` → `GET /api/indicadores/tempo-medio-execucao` → **Try it out** → **Execute**. Não há path, query nem body. Esperado **200 OK** com `ordensConsideradas` e `mediaHoras`. O indicador mede duração entre início e finalização por OS; a interpretação deve ser apresentada como a implementação atual, pois a granularidade do requisito ainda é uma pendência de validação do grupo. Não prometa valor de média específico: varia conforme as OS concluídas no banco.

## 13. Testes automatizados — 0:45

Antes de gravar, confira que `OFICINA_TEST_POSTGRES` aponta para a base dedicada `oficina_test`, nunca `oficina`. A variável pode conter credencial e não deve ser exibida.

```powershell
if ($env:OFICINA_TEST_POSTGRES -notmatch 'Database=oficina_test') {
    throw 'Configure OFICINA_TEST_POSTGRES para uma base descartável chamada oficina_test antes da gravação.'
}
dotnet test Oficina.slnx --no-build
```

Mostre a saída final da execução correspondente ao commit gravado. A suíte atual contém **64 testes**. A execução anterior reportada com PostgreSQL real registrou 63 aprovados antes do teste de OpenAPI mais recente. Para a gravação, configure `OFICINA_TEST_POSTGRES` para `oficina_test` e registre os números efetivamente exibidos; qualquer teste ignorado significa que a validação PostgreSQL não foi integral. Não mostre connection string ou senha.

**Destaque visual:** os testes `PostgresCriticalFlowTests` verificam aprovação/reserva/baixa, rollback após falha e conflito de concorrência com dois `DbContext`; `PostgresRoundTripTests` usa PostgreSQL real. Os testes não apagam o banco principal.

## 14. Event Storming e documentação de segurança — 0:50

1. Abra `docs/ddd/modelagem-inicial.md`; destaque agregados `OrdemDeServico` e `EstoqueDaPeca`, VOs `DocumentoFiscal`, `Placa` e `Dinheiro`, invariantes e transições.
2. Abra `docs/ddd/event-storming.md` e passe rapidamente pelos quatro diagramas Mermaid: recebimento/diagnóstico, orçamento e decisão, acompanhamento e estoque/reserva/baixa.
3. Diga explicitamente que os diagramas Mermaid são derivados do código e documentam o fluxo atual; não provam por si só que o grupo realizou workshops. Mostre board/export externo apenas se realmente existir e for público.
4. Abra `docs/relatorios/analise-vulnerabilidades.md`. Mostre o resultado NuGet de zero advisories conhecidos na data registrada e explique as limitações: scanner de imagem não executado e busca de segredos foi heurística.
5. Encerre com as lacunas reais: decisão de orçamento ainda é administrativa; não há teste automatizado comprovado de acesso cruzado entre OS; `Peca` não é uma entidade distinta de `Insumo`; e a varredura CVE da imagem precisa ser concluída se exigida na entrega.

**Não afirme:** certificação de segurança, aprovação pelo cliente, cobertura integral de todos os projetos ou workshop comprovado sem as evidências correspondentes.

## Mapa de IDs que devem ser copiados

| Resposta | Guardar como | Usado em |
|---|---|---|
| Criar cliente → `id` | `$clienteId` | Criar e consultar veículo; conferir vínculo |
| Criar veículo → `id` | `$veiculoId` | Criar OS |
| Criar serviço → `id` | `$servicoId` | Consultar catálogo e orçamento (`tipo: 0`) |
| Criar peça → `id` | `$pecaId` | Estoque/entrada/consulta e orçamento (`tipo: 1`) |
| Criar OS → `id` | `$ordemId` | Diagnóstico, orçamento, aprovação, execução, finalização, entrega e consulta |

Anote os IDs da tabela durante a execução. Em cada chamada, copie somente o UUID do campo `id` da resposta indicada; não use o ID de outro registro.

## Respostas e transições esperadas — consulta rápida

| Operação | HTTP esperado | Estado/sinal que deve aparecer |
|---|---:|---|
| `POST /api/auth/admin` | 200 | Resposta contém JWT; manter oculto |
| `GET /api/clientes` sem Bearer | 401 | Proteção administrativa |
| `POST` cliente/veículo/serviço/peça/OS | 201 | Criado; `Location` e DTO com `id` (OpenAPI atual documenta `200`, retorno runtime é `201`) |
| `GET` cadastro/estoque/OS/indicador | 200 | DTO consultado |
| Diagnóstico | 200 | `EmDiagnostico` |
| Disponibilizar orçamento | 200 | `AguardandoAprovacao`, versão 1, total 284.90 |
| Aprovação administrativa | 200 | `orcamentoAprovado: true`, status ainda `AguardandoAprovacao`, duas unidades reservadas |
| Início de execução | 200 | `EmExecucao`, baixa de duas unidades |
| Finalização | 200 | `Finalizada` e instante final |
| Entrega | 200 | `Entregue` |

## Funcionalidades que não serão encenadas como existentes

- Não há aprovação/rejeição do orçamento por endpoint de cliente; a decisão mostrada é administrativa.
- Login/JWT deve ser preparado antes da captura; não exiba o token completo nem o diálogo Authorize com o valor preenchido.
- Não serão apagados registros depois da gravação. Os dados criados ficam identificados pelo CPF/placa escolhidos e pelo sufixo temporal do nome.
- O indicador é por duração de OS entre início/fim e não apresenta cálculo individual por serviço.
- O health check apresentado não é um health check de banco.
- A varredura de CVEs da imagem e as sessões/exports reais de Event Storming não serão alegadas sem evidência.
