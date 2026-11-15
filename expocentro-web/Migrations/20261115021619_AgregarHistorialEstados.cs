using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Expocentro.Web.Migrations
{
    /// <inheritdoc />
    public partial class AgregarHistorialEstados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistorialEstadosSolicitud",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SolicitudEventoId = table.Column<int>(type: "integer", nullable: false),
                    EstadoSolicitudId = table.Column<int>(type: "integer", nullable: false),
                    FechaCambio = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Comentario = table.Column<string>(type: "text", nullable: true),
                    Actor = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialEstadosSolicitud", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialEstadosSolicitud_EstadosSolicitud_EstadoSolicitudId",
                        column: x => x.EstadoSolicitudId,
                        principalTable: "EstadosSolicitud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialEstadosSolicitud_SolicitudesEvento_SolicitudEvento~",
                        column: x => x.SolicitudEventoId,
                        principalTable: "SolicitudesEvento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstadosSolicitud_EstadoSolicitudId",
                table: "HistorialEstadosSolicitud",
                column: "EstadoSolicitudId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstadosSolicitud_SolicitudEventoId",
                table: "HistorialEstadosSolicitud",
                column: "SolicitudEventoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistorialEstadosSolicitud");
        }
    }
}
