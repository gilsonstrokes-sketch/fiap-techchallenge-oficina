using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.Cadastros;

public sealed class Veiculo
{
    private Veiculo(Guid id, Guid clienteId, Placa placa, string marca, string modelo, int ano)
    {
        Id = id;
        ClienteId = clienteId;
        Placa = placa;
        Marca = marca;
        Modelo = modelo;
        Ano = ano;
    }

    public Guid Id { get; }

    public Guid ClienteId { get; }

    public Placa Placa { get; private set; }

    public string Marca { get; private set; }

    public string Modelo { get; private set; }

    public int Ano { get; private set; }

    public bool Ativo { get; private set; } = true;

    public static Veiculo Criar(Guid clienteId, Placa placa, string marca, string modelo, int ano)
    {
        if (clienteId == Guid.Empty) throw new ArgumentException("O cliente é obrigatório.", nameof(clienteId));
        ArgumentNullException.ThrowIfNull(placa);
        ArgumentException.ThrowIfNullOrWhiteSpace(marca);
        ArgumentException.ThrowIfNullOrWhiteSpace(modelo);
        if (ano <= 0) throw new ArgumentOutOfRangeException(nameof(ano), "O ano deve ser positivo.");

        return new Veiculo(Guid.NewGuid(), clienteId, placa, marca.Trim(), modelo.Trim(), ano);
    }

    public void AlterarDados(Placa placa, string marca, string modelo, int ano)
    {
        ArgumentNullException.ThrowIfNull(placa);
        ArgumentException.ThrowIfNullOrWhiteSpace(marca);
        ArgumentException.ThrowIfNullOrWhiteSpace(modelo);
        if (ano <= 0) throw new ArgumentOutOfRangeException(nameof(ano), "O ano deve ser positivo.");

        Placa = placa;
        Marca = marca.Trim();
        Modelo = modelo.Trim();
        Ano = ano;
    }

    public void Inativar() => Ativo = false;

    public void Reativar() => Ativo = true;
}
