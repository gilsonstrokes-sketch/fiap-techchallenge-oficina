using Oficina.Application.Abstractions.Persistence;
using Oficina.Application.Models;
using Oficina.Domain.Atendimento;
using Oficina.Domain.Cadastros;
using Oficina.Domain.Catalogo;
using Oficina.Domain.Estoque;
using Oficina.Domain.ValueObjects;

namespace Oficina.Application;

public sealed class OficinaApplicationService(
    IClienteRepository clientes,
    IVeiculoRepository veiculos,
    IServicoRepository servicos,
    IPecaRepository pecas,
    IOrdemDeServicoRepository ordens,
    IEstoqueDaPecaRepository estoques,
    IUnitOfWork unitOfWork,
    TimeProvider relogio)
{
    public async Task<IReadOnlyList<ClienteDto>> ListarClientesAsync(CancellationToken ct) =>
        (await clientes.ListarAsync(ct)).Select(ClienteDto.De).ToArray();

    public async Task<ClienteDto> ObterClienteAsync(Guid id, CancellationToken ct) =>
        ClienteDto.De(await ExigirCliente(id, ct));

    public async Task<ClienteDto> CriarClienteAsync(string nome, string documentoFiscal, CancellationToken ct)
    {
        var cliente = Cliente.Criar(nome, DocumentoFiscal.Criar(documentoFiscal));
        clientes.Adicionar(cliente);
        await unitOfWork.CommitAsync(ct);
        return ClienteDto.De(cliente);
    }

    public async Task<ClienteDto> AlterarClienteAsync(Guid id, string nome, CancellationToken ct)
    {
        var cliente = await ExigirCliente(id, ct);
        cliente.AlterarNome(nome);
        await unitOfWork.CommitAsync(ct);
        return ClienteDto.De(cliente);
    }

    public async Task<ClienteDto> InativarClienteAsync(Guid id, CancellationToken ct)
    {
        var cliente = await ExigirCliente(id, ct);
        cliente.Inativar();
        await unitOfWork.CommitAsync(ct);
        return ClienteDto.De(cliente);
    }

    public async Task<ClienteDto> ReativarClienteAsync(Guid id, CancellationToken ct)
    {
        var cliente = await ExigirCliente(id, ct);
        cliente.Reativar();
        await unitOfWork.CommitAsync(ct);
        return ClienteDto.De(cliente);
    }

    public async Task<IReadOnlyList<VeiculoDto>> ListarVeiculosAsync(Guid? clienteId, CancellationToken ct)
    {
        var lista = clienteId.HasValue
            ? await veiculos.ListarPorClienteAsync(clienteId.Value, ct)
            : await veiculos.ListarAsync(ct);
        return lista.Select(VeiculoDto.De).ToArray();
    }

    public async Task<VeiculoDto> ObterVeiculoAsync(Guid id, CancellationToken ct) => VeiculoDto.De(await ExigirVeiculo(id, ct));

    public async Task<VeiculoDto> CriarVeiculoAsync(Guid clienteId, string placa, string marca, string modelo, int ano, CancellationToken ct)
    {
        var cliente = await ExigirCliente(clienteId, ct);
        ExigirAtivo(cliente.Ativo, "O cliente está inativo.");
        var veiculo = Veiculo.Criar(clienteId, Placa.Criar(placa), marca, modelo, ano);
        veiculos.Adicionar(veiculo);
        await unitOfWork.CommitAsync(ct);
        return VeiculoDto.De(veiculo);
    }

    public async Task<VeiculoDto> AlterarVeiculoAsync(Guid id, string placa, string marca, string modelo, int ano, CancellationToken ct)
    {
        var veiculo = await ExigirVeiculo(id, ct);
        veiculo.AlterarDados(Placa.Criar(placa), marca, modelo, ano);
        await unitOfWork.CommitAsync(ct);
        return VeiculoDto.De(veiculo);
    }

    public async Task<VeiculoDto> InativarVeiculoAsync(Guid id, CancellationToken ct) => await AlterarAtivoVeiculo(id, false, ct);
    public async Task<VeiculoDto> ReativarVeiculoAsync(Guid id, CancellationToken ct) => await AlterarAtivoVeiculo(id, true, ct);

    public async Task<IReadOnlyList<CatalogoDto>> ListarServicosAsync(CancellationToken ct) =>
        (await servicos.ListarAsync(ct)).Select(CatalogoDto.De).ToArray();

    public async Task<CatalogoDto> ObterServicoAsync(Guid id, CancellationToken ct) => CatalogoDto.De(await ExigirServico(id, ct));

    public async Task<CatalogoDto> CriarServicoAsync(string nome, string descricao, decimal preco, CancellationToken ct)
    {
        var servico = Servico.Criar(nome, descricao, Dinheiro.Criar(preco));
        servicos.Adicionar(servico);
        await unitOfWork.CommitAsync(ct);
        return CatalogoDto.De(servico);
    }

    public async Task<CatalogoDto> AlterarServicoAsync(Guid id, string nome, string descricao, decimal preco, CancellationToken ct)
    {
        var servico = await ExigirServico(id, ct);
        servico.Alterar(nome, descricao, Dinheiro.Criar(preco));
        await unitOfWork.CommitAsync(ct);
        return CatalogoDto.De(servico);
    }

    public async Task<CatalogoDto> InativarServicoAsync(Guid id, CancellationToken ct) => await AlterarAtivoServico(id, false, ct);
    public async Task<CatalogoDto> ReativarServicoAsync(Guid id, CancellationToken ct) => await AlterarAtivoServico(id, true, ct);

    public async Task<IReadOnlyList<CatalogoDto>> ListarPecasAsync(CancellationToken ct) =>
        (await pecas.ListarAsync(ct)).Select(CatalogoDto.De).ToArray();

    public async Task<CatalogoDto> ObterPecaAsync(Guid id, CancellationToken ct) => CatalogoDto.De(await ExigirPeca(id, ct));

    public async Task<CatalogoDto> CriarPecaAsync(string nome, string descricao, decimal preco, CancellationToken ct)
    {
        var peca = Peca.Criar(nome, descricao, Dinheiro.Criar(preco));
        pecas.Adicionar(peca);
        await unitOfWork.CommitAsync(ct);
        return CatalogoDto.De(peca);
    }

    public async Task<CatalogoDto> AlterarPecaAsync(Guid id, string nome, string descricao, decimal preco, CancellationToken ct)
    {
        var peca = await ExigirPeca(id, ct);
        peca.Alterar(nome, descricao, Dinheiro.Criar(preco));
        await unitOfWork.CommitAsync(ct);
        return CatalogoDto.De(peca);
    }

    public async Task<CatalogoDto> InativarPecaAsync(Guid id, CancellationToken ct) => await AlterarAtivoPeca(id, false, ct);
    public async Task<CatalogoDto> ReativarPecaAsync(Guid id, CancellationToken ct) => await AlterarAtivoPeca(id, true, ct);

    public async Task<EstoqueDto> ObterEstoqueAsync(Guid pecaId, CancellationToken ct) => EstoqueDtoDe(await ExigirEstoque(pecaId, ct));

    public async Task<EstoqueDto> RegistrarEntradaEstoqueAsync(Guid pecaId, decimal quantidade, CancellationToken ct)
    {
        var peca = await ExigirPeca(pecaId, ct);
        ExigirAtivo(peca.Ativo, "A peça está inativa.");
        var estoque = await estoques.ObterPorPecaIdAsync(pecaId, ct);
        if (estoque is null)
        {
            estoque = EstoqueDaPeca.Criar(pecaId, 0);
            estoques.Adicionar(estoque);
        }
        estoque.RegistrarEntrada(quantidade);
        await unitOfWork.CommitAsync(ct);
        return EstoqueDtoDe(estoque);
    }

    public async Task<OrdemDeServicoDto> CriarOrdemAsync(Guid clienteId, Guid veiculoId, CancellationToken ct)
    {
        var cliente = await ExigirCliente(clienteId, ct);
        var veiculo = await ExigirVeiculo(veiculoId, ct);
        ExigirAtivo(cliente.Ativo, "O cliente está inativo.");
        ExigirAtivo(veiculo.Ativo, "O veículo está inativo.");
        if (veiculo.ClienteId != clienteId) throw new InvalidOperationException("O veículo informado não pertence ao cliente.");

        var ordem = OrdemDeServico.Criar(clienteId, veiculoId, relogio.GetUtcNow());
        ordens.Adicionar(ordem);
        await unitOfWork.CommitAsync(ct);
        return OrdemDeServicoDto.De(ordem);
    }

    public async Task<OrdemDeServicoDto> CriarOrdemPorDocumentoAsync(string documentoFiscal, Guid veiculoId, CancellationToken ct)
    {
        var documento = DocumentoFiscal.Criar(documentoFiscal);
        var cliente = await clientes.ObterPorDocumentoAsync(documento, ct)
            ?? throw new RecursoNaoEncontradoException("Cliente não encontrado para o documento informado.");
        return await CriarOrdemAsync(cliente.Id, veiculoId, ct);
    }

    public async Task<IReadOnlyList<OrdemDeServicoDto>> ListarOrdensAsync(CancellationToken ct) =>
        (await ordens.ListarAsync(ct)).Select(OrdemDeServicoDto.De).ToArray();

    public async Task<OrdemDeServicoDto> ObterOrdemAsync(Guid id, CancellationToken ct) =>
        OrdemDeServicoDto.De(await ExigirOrdem(id, ct));

    public async Task<OrdemDeServicoDto> IniciarDiagnosticoAsync(Guid id, CancellationToken ct)
    {
        var ordem = await ExigirOrdem(id, ct);
        ordem.IniciarDiagnostico(relogio.GetUtcNow());
        await unitOfWork.CommitAsync(ct);
        return OrdemDeServicoDto.De(ordem);
    }

    public async Task<OrdemDeServicoDto> GerarOrcamentoAsync(Guid id, IReadOnlyCollection<ItemOrcamentoInput> itens, CancellationToken ct)
    {
        var ordem = await ExigirOrdem(id, ct);
        var snapshots = await CriarItensSnapshot(itens, ct);
        ordem.DisponibilizarOrcamento(snapshots, relogio.GetUtcNow());
        await unitOfWork.CommitAsync(ct);
        return OrdemDeServicoDto.De(ordem);
    }

    public async Task<OrdemDeServicoDto> SubstituirOrcamentoAprovadoAsync(Guid id, IReadOnlyCollection<ItemOrcamentoInput> itens, CancellationToken ct)
    {
        OrdemDeServico? ordem = null;
        await unitOfWork.ExecutarEmTransacaoAsync(async token =>
        {
            ordem = await ExigirOrdem(id, token);
            var anterior = ordem.ObterOrcamentoAtual() ?? throw new InvalidOperationException("A OS não possui orçamento para substituir.");
            var pecasAnteriores = anterior.Itens.Where(item => item.Tipo == TipoItemOrcamento.Peca)
                .GroupBy(item => item.ItemCatalogoId).Select(grupo => (PecaId: grupo.Key, Quantidade: grupo.Sum(item => item.Quantidade)));
            foreach (var item in pecasAnteriores)
            {
                var estoque = await ExigirEstoque(item.PecaId, token);
                if (estoque.ObterReservaConfirmada(ordem.Id, anterior.Versao) is not { } reserva || reserva.Quantidade != item.Quantidade)
                    throw new InvalidOperationException("A reserva da versão aprovada não corresponde ao estoque persistido.");
                estoque.LiberarReserva(ordem.Id, anterior.Versao);
            }
            var snapshots = await CriarItensSnapshot(itens, token);
            ordem.SubstituirOrcamentoAprovado(snapshots, relogio.GetUtcNow());
        }, ct);
        return OrdemDeServicoDto.De(ordem!);
    }

    public async Task<OrdemDeServicoDto> AprovarOrcamentoAsync(Guid id, CancellationToken ct)
    {
        OrdemDeServico? ordem = null;
        await unitOfWork.ExecutarEmTransacaoAsync(async token =>
        {
            ordem = await ExigirOrdem(id, token);
            var orcamento = ordem.ObterOrcamentoAtual() ?? throw new InvalidOperationException("A OS não possui orçamento.");
            ordem.AprovarOrcamento(relogio.GetUtcNow());
            var pecas = AgruparPecas(orcamento).ToArray();
            var estoquesDisponiveis = new List<(EstoqueDaPeca Estoque, Guid PecaId, decimal Quantidade)>();
            var estoqueSuficiente = true;
            foreach (var item in pecas)
            {
                var estoque = await estoques.ObterPorPecaIdAsync(item.PecaId, token);
                if (estoque is null || estoque.QuantidadeDisponivel < item.Quantidade)
                {
                    estoqueSuficiente = false;
                    break;
                }
                estoquesDisponiveis.Add((estoque, item.PecaId, item.Quantidade));
            }

            // A aprovação é preservada mesmo quando falta estoque; nesse caso o início tentará reservar novamente.
            if (estoqueSuficiente)
                foreach (var item in estoquesDisponiveis) item.Estoque.Reservar(ordem.Id, orcamento.Versao, item.Quantidade);
        }, ct);
        return OrdemDeServicoDto.De(ordem!);
    }

    public async Task<OrdemDeServicoDto> RejeitarOrcamentoAsync(Guid id, string? motivo, CancellationToken ct)
    {
        var ordem = await ExigirOrdem(id, ct);
        ordem.RejeitarOrcamento(relogio.GetUtcNow(), motivo);
        await unitOfWork.CommitAsync(ct);
        return OrdemDeServicoDto.De(ordem);
    }

    public async Task<OrdemDeServicoDto> IniciarExecucaoAsync(Guid id, CancellationToken ct)
    {
        OrdemDeServico? ordem = null;
        await unitOfWork.ExecutarEmTransacaoAsync(async token =>
        {
            ordem = await ExigirOrdem(id, token);
            var orcamento = ordem.ObterOrcamentoAtual() ?? throw new InvalidOperationException("A OS não possui orçamento.");
            var recibos = new List<Oficina.Domain.Contratos.ReservaPecaConfirmada>();
            var estoquesDaOrcamento = new List<EstoqueDaPeca>();
            foreach (var item in AgruparPecas(orcamento))
            {
                var estoque = await ExigirEstoque(item.PecaId, token);
                var recibo = estoque.ObterReservaConfirmada(ordem.Id, orcamento.Versao);
                if (recibo is null)
                {
                    if (estoque.QuantidadeDisponivel < item.Quantidade)
                        throw new InvalidOperationException("Estoque insuficiente; a aprovação do orçamento foi preservada.");
                    estoque.Reservar(ordem.Id, orcamento.Versao, item.Quantidade);
                    recibo = estoque.ObterReservaConfirmada(ordem.Id, orcamento.Versao);
                }
                if (recibo is null || recibo.Quantidade != item.Quantidade)
                    throw new InvalidOperationException("As reservas persistidas não correspondem às peças e quantidades do orçamento.");
                recibos.Add(recibo);
                estoquesDaOrcamento.Add(estoque);
            }
            var confirmacao = ConfirmacaoReservaEstoque.Criar(ordem.Id, orcamento.Versao, recibos);
            ordem.IniciarExecucao(relogio.GetUtcNow(), confirmacao);
            foreach (var estoque in estoquesDaOrcamento) estoque.Baixar(ordem.Id, orcamento.Versao);
        }, ct);
        return OrdemDeServicoDto.De(ordem!);
    }

    public async Task<OrdemDeServicoDto> FinalizarExecucaoAsync(Guid id, CancellationToken ct)
    {
        var ordem = await ExigirOrdem(id, ct);
        ordem.FinalizarExecucao(relogio.GetUtcNow());
        await unitOfWork.CommitAsync(ct);
        return OrdemDeServicoDto.De(ordem);
    }

    public async Task<OrdemDeServicoDto> EntregarVeiculoAsync(Guid id, CancellationToken ct)
    {
        var ordem = await ExigirOrdem(id, ct);
        ordem.EntregarVeiculo(relogio.GetUtcNow());
        await unitOfWork.CommitAsync(ct);
        return OrdemDeServicoDto.De(ordem);
    }

    public async Task<ConsultaClienteOrdemDto> ConsultarOrdemClienteAsync(Guid id, DocumentoFiscal documento, CancellationToken ct)
    {
        var ordem = await ExigirOrdem(id, ct);
        var cliente = await ExigirCliente(ordem.ClienteId, ct);
        if (cliente.DocumentoFiscal != documento) throw new RecursoNaoEncontradoException("Ordem não encontrada.");
        return ConsultaClienteDtoDe(ordem);
    }

    public async Task<ConsultaClienteOrdemDto> ObterConsultaClienteAsync(Guid id, CancellationToken ct)
    {
        var ordem = await ExigirOrdem(id, ct);
        return ConsultaClienteDtoDe(ordem);
    }

    private static ConsultaClienteOrdemDto ConsultaClienteDtoDe(OrdemDeServico ordem)
    {
        var id = ordem.Id;
        return new ConsultaClienteOrdemDto(id, ordem.Status.ToString(), ordem.ObterOrcamentoAtual()?.Total.Valor, ordem.ExecucaoIniciadaEm, ordem.ExecucaoFinalizadaEm);
    }

    public async Task<MediaExecucaoDto> ObterMediaExecucaoAsync(CancellationToken ct)
    {
        var duracoes = (await ordens.ListarAsync(ct))
            .Where(ordem => ordem.ExecucaoIniciadaEm.HasValue && ordem.ExecucaoFinalizadaEm.HasValue)
            .Select(ordem => ordem.ExecucaoFinalizadaEm!.Value - ordem.ExecucaoIniciadaEm!.Value)
            .Where(duracao => duracao >= TimeSpan.Zero)
            .ToArray();
        return new MediaExecucaoDto(duracoes.Length, duracoes.Length == 0 ? 0 : duracoes.Average(duracao => duracao.TotalHours));
    }

    private async Task<IReadOnlyList<ItemOrcamento>> CriarItensSnapshot(IReadOnlyCollection<ItemOrcamentoInput> itens, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(itens);
        var snapshots = new List<ItemOrcamento>(itens.Count);
        foreach (var entrada in itens)
        {
            switch (entrada.Tipo)
            {
                case TipoItemOrcamento.Servico:
                {
                    var servico = await ExigirServico(entrada.ItemCatalogoId, ct);
                    ExigirAtivo(servico.Ativo, "O serviço está inativo.");
                    snapshots.Add(ItemOrcamento.Criar(entrada.Tipo, servico.Id, servico.Nome, entrada.Quantidade, servico.Preco));
                    break;
                }
                case TipoItemOrcamento.Peca:
                {
                    var peca = await ExigirPeca(entrada.ItemCatalogoId, ct);
                    ExigirAtivo(peca.Ativo, "A peça está inativa.");
                    snapshots.Add(ItemOrcamento.Criar(entrada.Tipo, peca.Id, peca.Nome, entrada.Quantidade, peca.Preco));
                    break;
                }
                default: throw new ArgumentOutOfRangeException(nameof(itens), "Tipo de item inválido.");
            }
        }
        return snapshots;
    }

    private static IEnumerable<(Guid PecaId, decimal Quantidade)> AgruparPecas(Orcamento orcamento) =>
        orcamento.Itens.Where(item => item.Tipo == TipoItemOrcamento.Peca)
            .GroupBy(item => item.ItemCatalogoId)
            .Select(grupo => (grupo.Key, grupo.Sum(item => item.Quantidade)));

    private async Task<Cliente> ExigirCliente(Guid id, CancellationToken ct) =>
        await clientes.ObterPorIdAsync(id, ct) ?? throw new RecursoNaoEncontradoException("Cliente não encontrado.");
    private async Task<Veiculo> ExigirVeiculo(Guid id, CancellationToken ct) =>
        await veiculos.ObterPorIdAsync(id, ct) ?? throw new RecursoNaoEncontradoException("Veículo não encontrado.");
    private async Task<Servico> ExigirServico(Guid id, CancellationToken ct) =>
        await servicos.ObterPorIdAsync(id, ct) ?? throw new RecursoNaoEncontradoException("Serviço não encontrado.");
    private async Task<Peca> ExigirPeca(Guid id, CancellationToken ct) =>
        await pecas.ObterPorIdAsync(id, ct) ?? throw new RecursoNaoEncontradoException("Peça não encontrada.");
    private async Task<OrdemDeServico> ExigirOrdem(Guid id, CancellationToken ct) =>
        await ordens.ObterPorIdAsync(id, ct) ?? throw new RecursoNaoEncontradoException("Ordem de serviço não encontrada.");
    private async Task<EstoqueDaPeca> ExigirEstoque(Guid id, CancellationToken ct) =>
        await estoques.ObterPorPecaIdAsync(id, ct) ?? throw new InvalidOperationException($"Não há estoque cadastrado para a peça {id}.");

    private async Task<VeiculoDto> AlterarAtivoVeiculo(Guid id, bool ativo, CancellationToken ct)
    {
        var item = await ExigirVeiculo(id, ct);
        if (ativo) item.Reativar(); else item.Inativar();
        await unitOfWork.CommitAsync(ct);
        return VeiculoDto.De(item);
    }
    private async Task<CatalogoDto> AlterarAtivoServico(Guid id, bool ativo, CancellationToken ct)
    {
        var item = await ExigirServico(id, ct);
        if (ativo) item.Reativar(); else item.Inativar();
        await unitOfWork.CommitAsync(ct);
        return CatalogoDto.De(item);
    }
    private async Task<CatalogoDto> AlterarAtivoPeca(Guid id, bool ativo, CancellationToken ct)
    {
        var item = await ExigirPeca(id, ct);
        if (ativo) item.Reativar(); else item.Inativar();
        await unitOfWork.CommitAsync(ct);
        return CatalogoDto.De(item);
    }
    private static EstoqueDto EstoqueDtoDe(EstoqueDaPeca estoque) => new(estoque.PecaId, estoque.QuantidadeFisica, estoque.QuantidadeReservada, estoque.QuantidadeDisponivel);
    private static void ExigirAtivo(bool ativo, string mensagem)
    {
        if (!ativo) throw new InvalidOperationException(mensagem);
    }
}
