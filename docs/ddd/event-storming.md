# Fluxos de domínio em Mermaid

**Estado:** diagramas de fluxo derivados do código e da API auditados em 2026-11-03. São artefatos de apoio para Event Storming, não ata de reunião nem prova de que os três workshops foram realizados. Os participantes do grupo ainda precisam validar atores, políticas e perguntas em aberto e publicar/exportar os resultados finais.

As caixas marcadas **Comando** são intenções; as caixas **Evento** são fatos de negócio em notação. O código não publica esses fatos em broker e não implementa event sourcing.

## 1. Recebimento da OS e diagnóstico

```mermaid
flowchart LR
    recepcao[Ator: recepção / administrador]
    cmdAbrir[Comando: abrir OS por CPF/CNPJ e veículo]
    valido{Cliente e veículo existem, ativos e vinculados?}
    evRecebida((Evento: OS recebida))
    cmdDiag[Comando: iniciar diagnóstico]
    evDiag((Evento: diagnóstico iniciado))
    cmdOrcar[Comando: disponibilizar orçamento]
    snap[Política: ler catálogo e copiar snapshots]
    evOrcamento((Evento: orçamento disponibilizado, versão N))
    erro[Resultado: rejeitar comando; nenhum estado criado]

    recepcao --> cmdAbrir --> valido
    valido -- não --> erro
    valido -- sim --> evRecebida --> cmdDiag --> evDiag --> cmdOrcar --> snap --> evOrcamento

    classDef actor fill:#dbeafe,stroke:#2563eb,color:#111827
    classDef command fill:#fef3c7,stroke:#d97706,color:#111827
    classDef event fill:#ffedd5,stroke:#ea580c,color:#111827
    classDef policy fill:#ede9fe,stroke:#7c3aed,color:#111827
    class recepcao actor
    class cmdAbrir,cmdDiag,cmdOrcar command
    class evRecebida,evDiag,evOrcamento event
    class snap policy
```

**Código correspondente:** `POST /api/ordens-de-servico`, `POST /{id}/diagnostico`, `POST /{id}/orcamentos`; validação e snapshots em `OficinaApplicationService`.

## 2. Orçamento e decisão

```mermaid
flowchart TD
    disponibilizado((Evento: orçamento disponibilizado))
    cliente[Ator: cliente]
    cmdToken[Comando técnico: solicitar token com ID da OS e documento fiscal]
    token{Documento confere com a OS?}
    consulta[Leitura: consultar apenas a OS indicada pelo claim]
    negado[Resultado: negar emissão do token]
    admin[Ator atual da API: administrador]
    decisao{Decisão registrada pela API}
    cmdAprovar[Comando: aprovar orçamento]
    evAprovado((Evento: orçamento aprovado))
    reserva[Política: reservar se todas as peças têm saldo]
    semEstoque[Resultado: aprovação preservada; reserva não criada]
    cmdRejeitar[Comando: rejeitar orçamento; motivo opcional]
    evRejeitado((Evento: orçamento rejeitado))
    diagnostico((Evento: OS retorna a EmDiagnostico))
    cmdRevisar[Comando: substituir orçamento aprovado antes da baixa]
    liberar[Política: liberar reservas antigas e criar nova versão na transação]
    novaVersao((Evento: nova versão aguardando decisão))
    pergunta[Questão: o ator cliente ainda não dispõe de endpoint de aprovação/rejeição]

    disponibilizado --> cliente --> cmdToken --> token
    token -- sim --> consulta
    token -- não --> negado
    consulta -. acompanhamento, sem comando de decisão .-> pergunta
    admin --> decisao
    decisao -- aprovar --> cmdAprovar --> evAprovado --> reserva
    reserva -- saldo suficiente --> iniciar[Próxima ação: iniciar execução]
    reserva -- saldo insuficiente --> semEstoque
    decisao -- rejeitar --> cmdRejeitar --> evRejeitado --> diagnostico
    evAprovado --> cmdRevisar --> liberar --> novaVersao

    classDef actor fill:#dbeafe,stroke:#2563eb,color:#111827
    classDef command fill:#fef3c7,stroke:#d97706,color:#111827
    classDef event fill:#ffedd5,stroke:#ea580c,color:#111827
    classDef policy fill:#ede9fe,stroke:#7c3aed,color:#111827
    classDef risk fill:#fee2e2,stroke:#b91c1c,color:#111827
    class cliente,admin actor
    class cmdToken,cmdAprovar,cmdRejeitar,cmdRevisar command
    class disponibilizado,evAprovado,evRejeitado,diagnostico,novaVersao event
    class reserva,liberar policy
    class pergunta risk
```

**Código correspondente:** `POST /api/auth/cliente` emite token de consulta; `/api/consulta/ordens-de-servico/{id}` lê a OS do claim. Aprovação e rejeição estão nas rotas administrativas `POST /api/ordens-de-servico/{id}/orcamentos/aprovacao` e `/rejeicao`. O ator da decisão precisa ser validado pelo grupo: não há endpoint de decisão autenticado como cliente.

## 3. Acompanhamento da OS

```mermaid
stateDiagram-v2
    [*] --> Recebida: abrir OS
    Recebida --> EmDiagnostico: iniciar diagnóstico
    EmDiagnostico --> AguardandoAprovacao: disponibilizar versão N
    AguardandoAprovacao --> EmDiagnostico: rejeitar; guardar decisão e motivo opcional
    AguardandoAprovacao --> AguardandoAprovacao: aprovar; flag OrcamentoAprovado = true
    AguardandoAprovacao --> EmExecucao: iniciar; confirmar reserva e baixar estoque
    EmExecucao --> Finalizada: finalizar; guardar instante
    Finalizada --> Entregue: entregar veículo
    Entregue --> [*]

    state AguardandoAprovacao {
        [*] --> Pendente
        Pendente --> Aprovada: decisão gravada sem mudar status oficial
        Aprovada --> NovaVersao: substituir antes da baixa
        NovaVersao --> Pendente
    }
```

O cliente pode obter token de consulta e ler status, total do orçamento atual e horários de execução da própria OS. A consulta compara o ID pedido com `ordem_id` no JWT. As demais transições usam endpoints administrativos. O health check mede o processo, não a prontidão do banco.

## 4. Estoque, reserva e baixa

```mermaid
flowchart TD
    admin[Ator: administrador]
    entrada[Comando: registrar entrada de peça]
    saldo((Evento: saldo físico aumentado))
    aprovacao((Evento: orçamento aprovado))
    saldoSuf{Todas as peças têm quantidade disponível?}
    reservar[Política: reservar todas as quantidades na mesma transação]
    reserva((Evento: reservas persistidas por peça, OS e versão))
    aguardar[Estado: aprovado, sem reserva; início tenta reservar de novo]
    iniciar[Comando: iniciar execução]
    validar[Política: carregar estoque persistido e conferir reservas]
    temSaldo{Reserva existe ou há saldo para criá-la?}
    confirmar[Política: compor confirmação tipada internamente]
    baixar[Política: baixar reserva e avançar OS na transação]
    exec((Evento: execução iniciada; saldo físico reduzido))
    falha[Resultado: falhar sem saldo negativo; preservar aprovação]
    conflito[Resultado: xmin divergente gera conflito; sem retry automático]
    revisar[Comando: substituir versão aprovada antes da baixa]
    liberar[Política: liberar reserva antiga e salvar nova versão na mesma transação]

    admin --> entrada --> saldo --> saldoSuf
    aprovacao --> saldoSuf
    saldoSuf -- sim --> reservar --> reserva --> iniciar
    saldoSuf -- não --> aguardar --> iniciar
    iniciar --> validar --> temSaldo
    temSaldo -- sim --> confirmar --> baixar --> exec
    temSaldo -- não --> falha
    reservar -. conflito otimista .-> conflito
    baixar -. conflito otimista .-> conflito
    reserva --> revisar --> liberar --> nova((Evento: versão nova pendente de aprovação))

    classDef actor fill:#dbeafe,stroke:#2563eb,color:#111827
    classDef command fill:#fef3c7,stroke:#d97706,color:#111827
    classDef event fill:#ffedd5,stroke:#ea580c,color:#111827
    classDef policy fill:#ede9fe,stroke:#7c3aed,color:#111827
    classDef risk fill:#fee2e2,stroke:#b91c1c,color:#111827
    class admin actor
    class entrada,iniciar,revisar command
    class saldo,aprovacao,reserva,exec,nova event
    class reservar,validar,confirmar,baixar,liberar policy
    class falha,conflito risk
```

**Invariantes:** `disponível = físico − reservado`; ambos são quantidades inteiras; não reservar acima do disponível; baixar somente reserva existente; mudança concorrente em OS/estoque resulta HTTP 409. Os testes atuais cobrem o cenário com uma peça e conflito de atualização da raiz de estoque; não comprovam ainda todas as corridas simultâneas multi-peça.

## Pontos para as sessões do grupo

1. Confirmar quem pode aprovar/rejeitar e se cliente precisa de endpoint autenticado de decisão.
2. Validar se um fluxo com `Peca` atende também ao termo “insumo” do desafio.
3. Decidir devolução, cancelamento e reparos adicionais; não há fluxos correspondentes implementados.
4. Confirmar se tempo médio deve ser medido por OS inteira ou por serviço individual.
5. Anexar atores/participantes, data, decisões e export público dos boards reais antes de marcar os workshops como concluídos.
