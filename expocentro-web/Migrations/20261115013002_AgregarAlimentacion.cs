using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Expocentro.Web.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAlimentacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlimentacionesEvento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SolicitudEventoId = table.Column<int>(type: "integer", nullable: false),
                    RequiereAlimentacion = table.Column<bool>(type: "boolean", nullable: false),
                    CantidadPersonas = table.Column<int>(type: "integer", nullable: true),
                    HoraServicio = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Detalle = table.Column<string>(type: "text", nullable: true),
                    IncluyeAgua = table.Column<bool>(type: "boolean", nullable: false),
                    IncluyeCafe = table.Column<bool>(type: "boolean", nullable: false),
                    IncluyeRefrigerio = table.Column<bool>(type: "boolean", nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlimentacionesEvento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlimentacionesEvento_SolicitudesEvento_SolicitudEventoId",
                        column: x => x.SolicitudEventoId,
                        principalTable: "SolicitudesEvento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlimentacionesEvento_SolicitudEventoId",
                table: "AlimentacionesEvento",
                column: "SolicitudEventoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlimentacionesEvento");
        }
    }
}
