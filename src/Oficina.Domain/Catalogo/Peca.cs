using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.Catalogo;

/// <summary>Item de catálogo. O saldo e as reservas pertencem ao agregado de estoque.</summary>
public sealed class Peca
{
    private Peca(Guid id, string nome, string descricao, Dinheiro preco)
    {
        Id = id;
        Nome = nome;
        Descricao = descricao;
        Preco = preco;
    }

    public Guid Id { get; }

    public string Nome { get; private set; }

    public string Descricao { get; private set; }

    public Dinheiro Preco { get; private set; }

    public bool Ativo { get; private set; } = true;

    public static Peca Criar(string nome, string descricao, Dinheiro preco)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nome);
        ArgumentException.ThrowIfNullOrWhiteSpace(descricao);
        return new Peca(Guid.NewGuid(), nome.Trim(), descricao.Trim(), preco);
    }

    public void Alterar(string nome, string descricao, Dinheiro preco)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nome);
        ArgumentException.ThrowIfNullOrWhiteSpace(descricao);
        Nome = nome.Trim();
        Descricao = descricao.Trim();
        Preco = preco;
    }

    public void Inativar() => Ativo = false;

    public void Reativar() => Ativo = true;
}
