using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Expocentro.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departamentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departamentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EstadosSolicitud",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosSolicitud", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Salones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    CapacidadMaxima = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntraObjectId = table.Column<string>(type: "text", nullable: true),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Correo = table.Column<string>(type: "text", nullable: false),
                    DepartamentoId = table.Column<int>(type: "integer", nullable: false),
                    Rol = table.Column<string>(type: "text", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Departamentos_DepartamentoId",
                        column: x => x.DepartamentoId,
                        principalTable: "Departamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesEvento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "text", nullable: false),
                    NombreEvento = table.Column<string>(type: "text", nullable: false),
                    FechaHoraInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaHoraFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CantidadPersonas = table.Column<int>(type: "integer", nullable: false),
                    SolicitanteUsuarioId = table.Column<int>(type: "integer", nullable: false),
                    ResponsableUsuarioId = table.Column<int>(type: "integer", nullable: true),
                    SalonId = table.Column<int>(type: "integer", nullable: false),
                    DepartamentoCargoId = table.Column<int>(type: "integer", nullable: false),
                    EstadoId = table.Column<int>(type: "integer", nullable: false),
                    Otros = table.Column<string>(type: "text", nullable: true),
                    Comentarios = table.Column<string>(type: "text", nullable: true),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesEvento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesEvento_Departamentos_DepartamentoCargoId",
                        column: x => x.DepartamentoCargoId,
                        principalTable: "Departamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesEvento_EstadosSolicitud_EstadoId",
                        column: x => x.EstadoId,
                        principalTable: "EstadosSolicitud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesEvento_Salones_SalonId",
                        column: x => x.SalonId,
                        principalTable: "Salones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesEvento_Usuarios_ResponsableUsuarioId",
                        column: x => x.ResponsableUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SolicitudesEvento_Usuarios_SolicitanteUsuarioId",
                        column: x => x.SolicitanteUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEvento_DepartamentoCargoId",
                table: "SolicitudesEvento",
                column: "DepartamentoCargoId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEvento_EstadoId",
                table: "SolicitudesEvento",
                column: "EstadoId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEvento_ResponsableUsuarioId",
                table: "SolicitudesEvento",
                column: "ResponsableUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEvento_SalonId",
                table: "SolicitudesEvento",
                column: "SalonId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEvento_SolicitanteUsuarioId",
                table: "SolicitudesEvento",
                column: "SolicitanteUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_DepartamentoId",
                table: "Usuarios",
                column: "DepartamentoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitudesEvento");

            migrationBuilder.DropTable(
                name: "EstadosSolicitud");

            migrationBuilder.DropTable(
                name: "Salones");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Departamentos");
        }
    }
}
