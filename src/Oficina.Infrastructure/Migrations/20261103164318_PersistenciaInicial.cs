using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Oficina.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PersistenciaInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    documento_fiscal = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pecas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    preco = table.Column<decimal>(type: "numeric", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pecas", x => x.id);
                    table.CheckConstraint("ck_pecas_preco_valido", "preco >= 0 AND preco = trunc(preco, 2)");
                });

            migrationBuilder.CreateTable(
                name: "servicos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    preco = table.Column<decimal>(type: "numeric", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_servicos", x => x.id);
                    table.CheckConstraint("ck_servicos_preco_valido", "preco >= 0 AND preco = trunc(preco, 2)");
                });

            migrationBuilder.CreateTable(
                name: "veiculos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    placa = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    marca = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modelo = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ano = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_veiculos", x => x.id);
                    table.UniqueConstraint("ak_veiculos_id_cliente", x => new { x.id, x.cliente_id });
                    table.CheckConstraint("ck_veiculos_ano_positivo", "ano > 0");
                    table.ForeignKey(
                        name: "FK_veiculos_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "estoques_peca",
                columns: table => new
                {
                    peca_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade_fisica = table.Column<decimal>(type: "numeric", nullable: false),
                    revisao = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estoques_peca", x => x.peca_id);
                    table.CheckConstraint("ck_estoques_quantidade_fisica_inteira_nao_negativa", "quantidade_fisica >= 0 AND quantidade_fisica = trunc(quantidade_fisica)");
                    table.ForeignKey(
                        name: "FK_estoques_peca_pecas_peca_id",
                        column: x => x.peca_id,
                        principalTable: "pecas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ordens_de_servico",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    veiculo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    orcamento_aprovado = table.Column<bool>(type: "boolean", nullable: false),
                    execucao_iniciada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    execucao_finalizada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordens_de_servico", x => x.id);
                    table.CheckConstraint("ck_os_status_oficial", "status IN ('Recebida', 'EmDiagnostico', 'AguardandoAprovacao', 'EmExecucao', 'Finalizada', 'Entregue')");
                    table.ForeignKey(
                        name: "FK_ordens_de_servico_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ordens_de_servico_veiculos_veiculo_id_cliente_id",
                        columns: x => new { x.veiculo_id, x.cliente_id },
                        principalTable: "veiculos",
                        principalColumns: new[] { "id", "cliente_id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "historico_os",
                columns: table => new
                {
                    ordem_de_servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    de_status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    para_status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    acao = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ocorrido_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historico_os", x => new { x.ordem_de_servico_id, x.sequencia });
                    table.ForeignKey(
                        name: "FK_historico_os_ordens_de_servico_ordem_de_servico_id",
                        column: x => x.ordem_de_servico_id,
                        principalTable: "ordens_de_servico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orcamentos",
                columns: table => new
                {
                    versao = table.Column<int>(type: "integer", nullable: false),
                    ordem_de_servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    total = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orcamentos", x => new { x.ordem_de_servico_id, x.versao });
                    table.CheckConstraint("ck_orcamentos_total_valido", "total >= 0 AND total = trunc(total, 2)");
                    table.CheckConstraint("ck_orcamentos_versao_positiva", "versao > 0");
                    table.ForeignKey(
                        name: "FK_orcamentos_ordens_de_servico_ordem_de_servico_id",
                        column: x => x.ordem_de_servico_id,
                        principalTable: "ordens_de_servico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "decisoes_orcamento",
                columns: table => new
                {
                    ordem_de_servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    aprovado = table.Column<bool>(type: "boolean", nullable: false),
                    versao_orcamento = table.Column<int>(type: "integer", nullable: false),
                    ocorrido_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_decisoes_orcamento", x => new { x.ordem_de_servico_id, x.sequencia });
                    table.ForeignKey(
                        name: "FK_decisoes_orcamento_orcamentos_ordem_de_servico_id_versao_or~",
                        columns: x => new { x.ordem_de_servico_id, x.versao_orcamento },
                        principalTable: "orcamentos",
                        principalColumns: new[] { "ordem_de_servico_id", "versao" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_decisoes_orcamento_ordens_de_servico_ordem_de_servico_id",
                        column: x => x.ordem_de_servico_id,
                        principalTable: "ordens_de_servico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "itens_orcamento",
                columns: table => new
                {
                    ordem_de_servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    versao_orcamento = table.Column<int>(type: "integer", nullable: false),
                    numero_linha = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipo = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    item_catalogo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric", nullable: false),
                    valor_unitario = table.Column<decimal>(type: "numeric", nullable: false),
                    total = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itens_orcamento", x => new { x.ordem_de_servico_id, x.versao_orcamento, x.numero_linha });
                    table.CheckConstraint("ck_itens_orcamento_quantidade_inteira_positiva", "quantidade > 0 AND quantidade = trunc(quantidade)");
                    table.CheckConstraint("ck_itens_orcamento_tipo_valido", "tipo IN ('Servico', 'Peca')");
                    table.CheckConstraint("ck_itens_orcamento_total_valido", "total >= 0 AND total = trunc(total, 2)");
                    table.CheckConstraint("ck_itens_orcamento_valor_unitario_valido", "valor_unitario >= 0 AND valor_unitario = trunc(valor_unitario, 2)");
                    table.ForeignKey(
                        name: "FK_itens_orcamento_orcamentos_ordem_de_servico_id_versao_orcam~",
                        columns: x => new { x.ordem_de_servico_id, x.versao_orcamento },
                        principalTable: "orcamentos",
                        principalColumns: new[] { "ordem_de_servico_id", "versao" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reservas_estoque",
                columns: table => new
                {
                    ordem_de_servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    versao_orcamento = table.Column<int>(type: "integer", nullable: false),
                    peca_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservas_estoque", x => new { x.peca_id, x.ordem_de_servico_id, x.versao_orcamento });
                    table.CheckConstraint("ck_reservas_quantidade_inteira_positiva", "quantidade > 0 AND quantidade = trunc(quantidade)");
                    table.ForeignKey(
                        name: "FK_reservas_estoque_estoques_peca_peca_id",
                        column: x => x.peca_id,
                        principalTable: "estoques_peca",
                        principalColumn: "peca_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reservas_estoque_orcamentos_ordem_de_servico_id_versao_orca~",
                        columns: x => new { x.ordem_de_servico_id, x.versao_orcamento },
                        principalTable: "orcamentos",
                        principalColumns: new[] { "ordem_de_servico_id", "versao" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reservas_estoque_ordens_de_servico_ordem_de_servico_id",
                        column: x => x.ordem_de_servico_id,
                        principalTable: "ordens_de_servico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ux_clientes_documento_fiscal",
                table: "clientes",
                column: "documento_fiscal",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_decisoes_orcamento_ordem_de_servico_id_versao_orcamento",
                table: "decisoes_orcamento",
                columns: new[] { "ordem_de_servico_id", "versao_orcamento" });

            migrationBuilder.CreateIndex(
                name: "IX_ordens_de_servico_cliente_id",
                table: "ordens_de_servico",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_ordens_de_servico_veiculo_id_cliente_id",
                table: "ordens_de_servico",
                columns: new[] { "veiculo_id", "cliente_id" });

            migrationBuilder.CreateIndex(
                name: "ix_os_status",
                table: "ordens_de_servico",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_reservas_estoque_ordem_de_servico_id_versao_orcamento",
                table: "reservas_estoque",
                columns: new[] { "ordem_de_servico_id", "versao_orcamento" });

            migrationBuilder.CreateIndex(
                name: "IX_veiculos_cliente_id",
                table: "veiculos",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ux_veiculos_placa",
                table: "veiculos",
                column: "placa",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "decisoes_orcamento");

            migrationBuilder.DropTable(
                name: "historico_os");

            migrationBuilder.DropTable(
                name: "itens_orcamento");

            migrationBuilder.DropTable(
                name: "reservas_estoque");

            migrationBuilder.DropTable(
                name: "servicos");

            migrationBuilder.DropTable(
                name: "estoques_peca");

            migrationBuilder.DropTable(
                name: "orcamentos");

            migrationBuilder.DropTable(
                name: "pecas");

            migrationBuilder.DropTable(
                name: "ordens_de_servico");

            migrationBuilder.DropTable(
                name: "veiculos");

            migrationBuilder.DropTable(
                name: "clientes");
        }
    }
}
