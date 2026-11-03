# Estratégia de testes e cobertura

Esta é uma estratégia de planejamento da Etapa 0. Nenhum projeto ou teste foi criado/executado nesta etapa.

## Escopo crítico para cobertura

O relatório de cobertura deve medir o código de domínio que concentra invariantes e cálculos, inicialmente:

- Ordem de Serviço: criação, transições permitidas/proibidas e histórico;
- Orçamento: composição, snapshots, aprovação/rejeição e revisões;
- Estoque: reserva, baixa, estorno/devolução e regra de saldo não negativo;
- Objetos de valor e validações: CPF/CNPJ, placa, dinheiro e quantidade;
- Cálculo de duração/média, após a definição de granularidade.

Meta mínima: **80% de cobertura de linhas** sobre o escopo crítico acima, com escopo/arquivos incluídos e excluídos documentados. A ferramenta deve produzir relatório legível e resultado de CI ou artefato versionado. Cobertura não substitui assertions de comportamento, revisão ou testes de integração.

## Camadas de teste

| Camada | Responsabilidade | Exemplos previstos |
|---|---|---|
| Unitário de domínio | Invariantes e cálculos sem banco/rede | Matriz de transições, orçamento, snapshots, estoque, VOs e relógio injetável |
| Integração de persistência | Mapeamento relacional, constraints, transações e concorrência | PostgreSQL real; migrations; rollback; duas operações concorrentes no mesmo saldo |
| Integração de aplicação/API | Casos de uso, autorização, serialização e HTTP | Fluxos administrativos JWT, escopo do token do cliente e acesso cruzado negado |
| Contrato/documentação | API documentada e erros coerentes | Esquemas OpenAPI, códigos HTTP e ProblemDetails |

PostgreSQL via Testcontainers é a proposta para testes de integração, a confirmar conforme a disponibilidade de Docker no ambiente de CI. Não usar provedor em memória como evidência de comportamento específico do PostgreSQL.

## Fluxos críticos a cobrir

1. Criar OS, identificar cliente e veículo, registrar diagnóstico e itens.
2. Gerar/disponibilizar orçamento, aprovar e iniciar execução com estoque consistente.
3. Rejeitar orçamento, preservar a decisão, retornar a diagnóstico e criar revisão sem reescrever histórico.
4. Rejeitar operações com estoque insuficiente e demonstrar ausência de saldo negativo sob concorrência.
5. Finalizar e entregar OS; validar cálculo de duração conforme regra aprovada.
6. Permitir consulta do cliente autorizado e negar acesso a OS de terceiros.
7. CRUD/inativação de cadastros usados por OS e perfil administrativo.

## Critérios futuros de aceite

- Comandos de teste e geração de cobertura documentados e reproduzíveis.
- Relatório mostra pelo menos 80% no escopo crítico aprovado.
- Testes de integração usam PostgreSQL e validam as constraints e transações relevantes.
- Cenários negativos e de concorrência possuem asserts explícitos.
- Resultados e limitações são informados sem declarar como testado algo não executado.
