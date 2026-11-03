using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.Catalogo;

public sealed class Servico
{
    private Servico(Guid id, string nome, string descricao, Dinheiro preco)
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

    public static Servico Criar(string nome, string descricao, Dinheiro preco)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nome);
        ArgumentException.ThrowIfNullOrWhiteSpace(descricao);
        return new Servico(Guid.NewGuid(), nome.Trim(), descricao.Trim(), preco);
    }

    public void Alterar(string nome, string descricao, Dinheiro preco)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nome);
        ArgumentException.ThrowIfNullOrWhiteSpace(descricao);
        Nome = nome.Trim();
        Descricao = descricao.Trim();
        Preco = preco;
    }
}
