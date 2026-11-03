# Arquitetura aprovada — detalhes sujeitos a revisão

## Objetivo
Monólito em camadas, **.NET 10** e **PostgreSQL**, atendendo o enunciado sem superengenharia. O usuário aprovou esta arquitetura para o projeto; ela é uma escolha técnica, não uma exigência de stack da FIAP. As decisões de negócio e detalhes de persistência permanecem sujeitos à validação descrita nos documentos da Etapa 0.

## Solução sugerida
```text
Oficina.slnx
src/
  Oficina.Api/              # controllers/endpoints, JWT, OpenAPI, composition root
  Oficina.Application/      # casos de uso, DTOs, interfaces de portas
  Oficina.Domain/           # entidades, VOs, invariantes, eventos de domínio
  Oficina.Infrastructure/   # EF Core, migrations, repositórios, auth implementation
tests/
  Oficina.Domain.Tests/     # testes de regras, transições, cálculos
  Oficina.Integration.Tests/# API + PostgreSQL real via Testcontainers (proposta)
docs/
  ddd/                     # Event Storming, diagramas, glossário
  adr/                     # decisões arquiteturais
  fases/                   # marcos da implementação
```

## Dependências permitidas
- `Oficina.Domain`: sem referências a Application, Infrastructure, Api, EF Core, ASP.NET ou banco.
- `Oficina.Application` → `Oficina.Domain`.
- `Oficina.Infrastructure` → `Oficina.Application` + `Oficina.Domain`; contém EF Core/Npgsql, migrations e implementações de persistência/identidade.
- `Oficina.Api` → `Oficina.Application` + `Oficina.Infrastructure` apenas para composição/DI e configuração de hospedagem.
- `Oficina.Domain.Tests` → `Oficina.Domain`.
- `Oficina.Integration.Tests` → projetos necessários para subir/testar API e persistência; não deve ser referência de produção.
- Não criar dependência circular; contratos específicos de casos de uso pertencem a Application, regras invariantes pertencem a Domain.

## Módulos lógicos (dentro do mesmo processo)
- **Cadastro**: Cliente, Veículo.
- **Catálogo e Estoque**: Serviço, Peça/Insumo, movimentações.
- **Atendimento**: Ordem de Serviço, itens de serviço, orçamento, aprovação, status.
- **Identidade**: autenticação de administração e autorização de consulta de cliente.
Esses módulos são candidatos de organização, **não bounded contexts validados**.

## Persistência e banco
- PostgreSQL em Docker Compose; EF Core 10 com Npgsql; migrations versionadas.
- Preferir tabelas e constraints explícitas; índices para CPF/CNPJ normalizado, placa e consultas por status/data.
- Salvar preço e quantidade dos itens na OS/orçamento como **snapshot** para evitar alteração retroativa quando catálogo mudar (proposta a validar).
- Transações para criação/atualização de OS, aprovação de orçamento e movimentação de estoque; definir estratégia de concorrência antes de implementar baixa de estoque.
- Decisão para a Etapa 3: concorrência otimista PostgreSQL por `xmin` nos agregados OS e Estoque; traduzir conflito explicitamente, sem retry automático. Application coordena agregados e Infrastructure controla persistência/transações.

## API e segurança
- REST JSON, documentação OpenAPI/Swagger UI, códigos HTTP coerentes, ProblemDetails para erros.
- JWT Bearer nas rotas administrativas; nunca usar credenciais de demonstração fixas em código.
- Consulta de cliente via API **não deve expor OS por ID previsível sem autorização**. Desenhar autenticação/escopo do cliente ou token de consulta limitado antes de codificar.
- Validar CPF/CNPJ (dígitos verificadores) e placa (formatos aplicáveis), com testes de borda.

## Testabilidade
- Regras de negócio no Domain; unitários de transições, orçamento, estoque.
- Integração de API/persistência com PostgreSQL real, preferencialmente Testcontainers.
- Escopo planejado de cobertura (OS, orçamento, estoque, VOs e cálculo de duração), relatório e camadas de teste estão em `docs/estrategia-testes.md`.
- Gerar evidência de cobertura >=80% nos domínios críticos; registrar definição do escopo medido. Esse percentual não prova correção.
- Análise de vulnerabilidades com ferramenta escolhida e resultado exportado; incluir dependências e imagem Docker quando viável.

## Restrições
Não adicionar front-end na Fase 1. Evitar microserviços, broker, event sourcing, mensageria e padrões extras sem necessidade demonstrável. Não confundir eventos de domínio usados em DDD/Event Storming com obrigação de infraestrutura assíncrona.
