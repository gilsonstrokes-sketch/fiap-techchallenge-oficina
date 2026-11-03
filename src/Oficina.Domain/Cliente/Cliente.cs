using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.Cadastros;

public sealed class Cliente
{
    private Cliente(Guid id, string nome, DocumentoFiscal documentoFiscal)
    {
        Id = id;
        Nome = nome;
        DocumentoFiscal = documentoFiscal;
    }

    public Guid Id { get; }

    public string Nome { get; private set; }

    public DocumentoFiscal DocumentoFiscal { get; }

    public bool Ativo { get; private set; } = true;

    public static Cliente Criar(string nome, DocumentoFiscal documentoFiscal)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nome);
        ArgumentNullException.ThrowIfNull(documentoFiscal);
        return new Cliente(Guid.NewGuid(), nome.Trim(), documentoFiscal);
    }

    public void AlterarNome(string nome)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nome);
        Nome = nome.Trim();
    }

    public void Inativar() => Ativo = false;

    public void Reativar() => Ativo = true;
}
