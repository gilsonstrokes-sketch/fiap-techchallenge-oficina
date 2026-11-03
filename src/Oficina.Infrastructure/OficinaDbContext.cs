using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Oficina.Domain.Atendimento;
using Oficina.Domain.Cadastros;
using Oficina.Domain.Catalogo;
using Oficina.Domain.Estoque;
using Oficina.Domain.ValueObjects;

namespace Oficina.Infrastructure;

public sealed class OficinaDbContext(DbContextOptions<OficinaDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<Peca> Pecas => Set<Peca>();
    public DbSet<OrdemDeServico> OrdensDeServico => Set<OrdemDeServico>();
    public DbSet<EstoqueDaPeca> EstoquesDePeca => Set<EstoqueDaPeca>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var dinheiroConverter = new ValueConverter<Dinheiro, decimal>(
            dinheiro => dinheiro.Valor,
            valor => Dinheiro.Criar(valor));
        var documentoConverter = new ValueConverter<DocumentoFiscal, string>(
            documento => documento.Valor,
            valor => DocumentoFiscal.Criar(valor));
        var placaConverter = new ValueConverter<Placa, string>(
            placa => placa.Valor,
            valor => Placa.Criar(valor));

        ConfigurarCadastros(modelBuilder, dinheiroConverter, documentoConverter, placaConverter);
        ConfigurarOrdemDeServico(modelBuilder, dinheiroConverter);
        ConfigurarEstoque(modelBuilder);
    }

    private static void ConfigurarCadastros(
        ModelBuilder modelBuilder,
        ValueConverter<Dinheiro, decimal> dinheiroConverter,
        ValueConverter<DocumentoFiscal, string> documentoConverter,
        ValueConverter<Placa, string> placaConverter)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("clientes");
            entity.HasKey(cliente => cliente.Id);
            entity.Property(cliente => cliente.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(cliente => cliente.Nome).HasColumnName("nome").HasMaxLength(160).IsRequired();
            entity.Property(cliente => cliente.DocumentoFiscal)
                .HasColumnName("documento_fiscal")
                .HasConversion(documentoConverter)
                .HasMaxLength(14)
                .IsRequired();
            entity.HasIndex(cliente => cliente.DocumentoFiscal).IsUnique().HasDatabaseName("ux_clientes_documento_fiscal");
        });

        modelBuilder.Entity<Veiculo>(entity =>
        {
            entity.ToTable("veiculos", table => table.HasCheckConstraint("ck_veiculos_ano_positivo", "ano > 0"));
            entity.HasKey(veiculo => veiculo.Id);
            entity.Property(veiculo => veiculo.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(veiculo => veiculo.ClienteId).HasColumnName("cliente_id");
            entity.Property(veiculo => veiculo.Placa)
                .HasColumnName("placa")
                .HasConversion(placaConverter)
                .HasMaxLength(7)
                .IsRequired();
            entity.Property(veiculo => veiculo.Marca).HasColumnName("marca").HasMaxLength(100).IsRequired();
            entity.Property(veiculo => veiculo.Modelo).HasColumnName("modelo").HasMaxLength(120).IsRequired();
            entity.Property(veiculo => veiculo.Ano).HasColumnName("ano");
            entity.HasIndex(veiculo => veiculo.Placa).IsUnique().HasDatabaseName("ux_veiculos_placa");
            entity.HasAlternateKey(veiculo => new { veiculo.Id, veiculo.ClienteId }).HasName("ak_veiculos_id_cliente");
            entity.HasOne<Cliente>().WithMany().HasForeignKey(veiculo => veiculo.ClienteId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Servico>(entity =>
        {
            entity.ToTable("servicos", table => table.HasCheckConstraint(
                "ck_servicos_preco_valido",
                "preco >= 0 AND preco = trunc(preco, 2)"));
            entity.HasKey(servico => servico.Id);
            entity.Property(servico => servico.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(servico => servico.Nome).HasColumnName("nome").HasMaxLength(160).IsRequired();
            entity.Property(servico => servico.Descricao).HasColumnName("descricao").HasMaxLength(1000).IsRequired();
            entity.Property(servico => servico.Preco)
                .HasColumnName("preco")
                .HasConversion(dinheiroConverter)
                .HasColumnType("numeric")
                .IsRequired();
        });

        modelBuilder.Entity<Peca>(entity =>
        {
            entity.ToTable("pecas", table => table.HasCheckConstraint(
                "ck_pecas_preco_valido",
                "preco >= 0 AND preco = trunc(preco, 2)"));
            entity.HasKey(peca => peca.Id);
            entity.Property(peca => peca.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(peca => peca.Nome).HasColumnName("nome").HasMaxLength(160).IsRequired();
            entity.Property(peca => peca.Descricao).HasColumnName("descricao").HasMaxLength(1000).IsRequired();
            entity.Property(peca => peca.Preco)
                .HasColumnName("preco")
                .HasConversion(dinheiroConverter)
                .HasColumnType("numeric")
                .IsRequired();
        });
    }

    private static void ConfigurarOrdemDeServico(ModelBuilder modelBuilder, ValueConverter<Dinheiro, decimal> dinheiroConverter)
    {
        modelBuilder.Entity<OrdemDeServico>(entity =>
        {
            entity.ToTable("ordens_de_servico", table => table.HasCheckConstraint(
                "ck_os_status_oficial",
                "status IN ('Recebida', 'EmDiagnostico', 'AguardandoAprovacao', 'EmExecucao', 'Finalizada', 'Entregue')"));
            entity.HasKey(ordem => ordem.Id);
            entity.Property(ordem => ordem.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(ordem => ordem.ClienteId).HasColumnName("cliente_id");
            entity.Property(ordem => ordem.VeiculoId).HasColumnName("veiculo_id");
            entity.Property(ordem => ordem.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32);
            entity.Property(ordem => ordem.OrcamentoAprovado).HasColumnName("orcamento_aprovado");
            entity.Property(ordem => ordem.ExecucaoIniciadaEm).HasColumnName("execucao_iniciada_em").HasColumnType("timestamp with time zone");
            entity.Property(ordem => ordem.ExecucaoFinalizadaEm).HasColumnName("execucao_finalizada_em").HasColumnType("timestamp with time zone");
            entity.Property<uint>("xmin").IsRowVersion();
            entity.HasIndex(ordem => ordem.Status).HasDatabaseName("ix_os_status");
            entity.HasOne<Cliente>().WithMany().HasForeignKey(ordem => ordem.ClienteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Veiculo>().WithMany()
                .HasForeignKey(ordem => new { ordem.VeiculoId, ordem.ClienteId })
                .HasPrincipalKey(veiculo => new { veiculo.Id, veiculo.ClienteId })
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(ordem => ordem.Orcamentos).WithOne()
                .HasForeignKey("OrdemDeServicoId").OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(ordem => ordem.Orcamentos).HasField("_orcamentos").UsePropertyAccessMode(PropertyAccessMode.Field);

            entity.HasMany(ordem => ordem.Historico).WithOne()
                .HasForeignKey("OrdemDeServicoId").OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(ordem => ordem.Historico).HasField("_historico").UsePropertyAccessMode(PropertyAccessMode.Field);

            entity.HasMany(ordem => ordem.DecisoesOrcamento).WithOne()
                .HasForeignKey("OrdemDeServicoId").OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(ordem => ordem.DecisoesOrcamento).HasField("_decisoesOrcamento").UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<Orcamento>(entity =>
        {
            entity.ToTable("orcamentos", table =>
            {
                table.HasCheckConstraint("ck_orcamentos_versao_positiva", "versao > 0");
                table.HasCheckConstraint("ck_orcamentos_total_valido", "total >= 0 AND total = trunc(total, 2)");
            });
            entity.HasKey("OrdemDeServicoId", nameof(Orcamento.Versao));
            entity.Property<Guid>("OrdemDeServicoId").HasColumnName("ordem_de_servico_id");
            entity.Property(orcamento => orcamento.Versao).HasColumnName("versao");
            entity.Property(orcamento => orcamento.Total)
                .HasColumnName("total")
                .HasConversion(dinheiroConverter)
                .HasColumnType("numeric")
                .IsRequired();
            entity.Ignore(orcamento => orcamento.EhValido);
            entity.HasMany(orcamento => orcamento.Itens).WithOne()
                .HasForeignKey("OrdemDeServicoId", "VersaoOrcamento").OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(orcamento => orcamento.Itens).HasField("_itens").UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<ItemOrcamento>(entity =>
        {
            entity.ToTable("itens_orcamento", table =>
            {
                table.HasCheckConstraint("ck_itens_orcamento_quantidade_inteira_positiva", "quantidade > 0 AND quantidade = trunc(quantidade)");
                table.HasCheckConstraint("ck_itens_orcamento_total_valido", "total >= 0 AND total = trunc(total, 2)");
                table.HasCheckConstraint("ck_itens_orcamento_valor_unitario_valido", "valor_unitario >= 0 AND valor_unitario = trunc(valor_unitario, 2)");
                table.HasCheckConstraint("ck_itens_orcamento_tipo_valido", "tipo IN ('Servico', 'Peca')");
            });
            entity.Property<Guid>("OrdemDeServicoId").HasColumnName("ordem_de_servico_id");
            entity.Property<int>("VersaoOrcamento").HasColumnName("versao_orcamento");
            entity.Property<int>("NumeroLinha").HasColumnName("numero_linha").ValueGeneratedOnAdd();
            entity.HasKey("OrdemDeServicoId", "VersaoOrcamento", "NumeroLinha");
            entity.Property(item => item.Tipo).HasColumnName("tipo").HasConversion<string>().HasMaxLength(16);
            entity.Property(item => item.ItemCatalogoId).HasColumnName("item_catalogo_id");
            entity.Property(item => item.Descricao).HasColumnName("descricao").HasMaxLength(1000).IsRequired();
            entity.Property(item => item.Quantidade).HasColumnName("quantidade").HasColumnType("numeric").IsRequired();
            entity.Property(item => item.ValorUnitario)
                .HasColumnName("valor_unitario").HasConversion(dinheiroConverter).HasColumnType("numeric").IsRequired();
            entity.Property(item => item.Total)
                .HasColumnName("total").HasConversion(dinheiroConverter).HasColumnType("numeric").IsRequired();
        });

        modelBuilder.Entity<TransicaoOrdemDeServico>(entity =>
        {
            entity.ToTable("historico_os");
            entity.Property<Guid>("OrdemDeServicoId").HasColumnName("ordem_de_servico_id");
            entity.Property<int>("Sequencia").HasColumnName("sequencia").ValueGeneratedOnAdd();
            entity.HasKey("OrdemDeServicoId", "Sequencia");
            entity.Property(transicao => transicao.De).HasColumnName("de_status").HasConversion<string>().HasMaxLength(32);
            entity.Property(transicao => transicao.Para).HasColumnName("para_status").HasConversion<string>().HasMaxLength(32);
            entity.Property(transicao => transicao.Acao).HasColumnName("acao").HasMaxLength(80).IsRequired();
            entity.Property(transicao => transicao.OcorridoEm).HasColumnName("ocorrido_em").HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<DecisaoOrcamento>(entity =>
        {
            entity.ToTable("decisoes_orcamento");
            entity.Property<Guid>("OrdemDeServicoId").HasColumnName("ordem_de_servico_id");
            entity.Property<int>("Sequencia").HasColumnName("sequencia").ValueGeneratedOnAdd();
            entity.HasKey("OrdemDeServicoId", "Sequencia");
            entity.Property(decisao => decisao.Aprovado).HasColumnName("aprovado");
            entity.Property(decisao => decisao.VersaoOrcamento).HasColumnName("versao_orcamento");
            entity.Property(decisao => decisao.OcorridoEm).HasColumnName("ocorrido_em").HasColumnType("timestamp with time zone");
            entity.Property(decisao => decisao.Motivo).HasColumnName("motivo").HasMaxLength(500);
            entity.HasOne<Orcamento>().WithMany().HasForeignKey("OrdemDeServicoId", "VersaoOrcamento")
                .HasPrincipalKey("OrdemDeServicoId", nameof(Orcamento.Versao));
        });
    }

    private static void ConfigurarEstoque(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EstoqueDaPeca>(entity =>
        {
            entity.ToTable("estoques_peca", table =>
            {
                table.HasCheckConstraint(
                    "ck_estoques_quantidade_fisica_inteira_nao_negativa",
                    "quantidade_fisica >= 0 AND quantidade_fisica = trunc(quantidade_fisica)");
            });
            entity.HasKey(estoque => estoque.PecaId);
            entity.Property(estoque => estoque.PecaId).HasColumnName("peca_id").ValueGeneratedNever();
            entity.Property(estoque => estoque.QuantidadeFisica).HasColumnName("quantidade_fisica").HasColumnType("numeric").IsRequired();
            entity.Property<uint>("xmin").IsRowVersion();
            entity.HasOne<Peca>().WithMany().HasForeignKey(estoque => estoque.PecaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(estoque => estoque.Reservas).WithOne().HasForeignKey("PecaId").OnDelete(DeleteBehavior.Restrict);
            entity.Navigation(estoque => estoque.Reservas).HasField("_reservas").UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<ReservaDeEstoque>(entity =>
        {
            entity.ToTable("reservas_estoque", table => table.HasCheckConstraint(
                "ck_reservas_quantidade_inteira_positiva",
                "quantidade > 0 AND quantidade = trunc(quantidade)"));
            entity.Property<Guid>("PecaId").HasColumnName("peca_id");
            entity.Property(reserva => reserva.OrdemDeServicoId).HasColumnName("ordem_de_servico_id");
            entity.Property(reserva => reserva.VersaoOrcamento).HasColumnName("versao_orcamento");
            entity.Property(reserva => reserva.Quantidade).HasColumnName("quantidade").HasColumnType("numeric").IsRequired();
            entity.HasKey("PecaId", nameof(ReservaDeEstoque.OrdemDeServicoId), nameof(ReservaDeEstoque.VersaoOrcamento));
            entity.HasOne<OrdemDeServico>().WithMany().HasForeignKey(reserva => reserva.OrdemDeServicoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Orcamento>().WithMany().HasForeignKey(reserva => new { reserva.OrdemDeServicoId, reserva.VersaoOrcamento })
                .HasPrincipalKey("OrdemDeServicoId", nameof(Orcamento.Versao)).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
