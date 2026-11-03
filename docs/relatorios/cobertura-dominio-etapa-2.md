# Cobertura do domínio — Etapa 2

**Data:** 2026-11-03  
**Comando:** `dotnet test tests/Oficina.Domain.Tests/Oficina.Domain.Tests.csproj --no-build --no-restore --collect:"XPlat Code Coverage" --verbosity minimal -m:1`  
**Resultado:** 48 testes aprovados; 0 falhas.

## Cobertura coletada

| Métrica | Resultado | Cobertura |
|---|---:|---:|
| Linhas | 481 / 501 | 96,00% |
| Branches | 152 / 178 | 85,39% |

O relatório Cobertura foi gerado em `tests/Oficina.Domain.Tests/TestResults/` (diretório ignorado pelo Git). O comando acima regenera o XML detalhado localmente. Os números deste resumo foram lidos do XML da execução indicada.

## Escopo e limites

O relatório mede a assembly `Oficina.Domain`, incluindo validações de documentos, placas e dinheiro, entidades de cadastro, versões/linhas de orçamento, transições de OS e estoque por peça. Persistência, atomicidade/concorrência entre transações PostgreSQL e políticas ainda não aprovadas não fazem parte do código desta etapa e não estão cobertas por este resultado.
