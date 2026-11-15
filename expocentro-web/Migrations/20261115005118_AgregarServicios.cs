using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Expocentro.Web.Migrations
{
    /// <inheritdoc />
    public partial class AgregarServicios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Servicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesServicio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SolicitudEventoId = table.Column<int>(type: "integer", nullable: false),
                    ServicioId = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesServicio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesServicio_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesServicio_SolicitudesEvento_SolicitudEventoId",
                        column: x => x.SolicitudEventoId,
                        principalTable: "SolicitudesEvento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Servicios",
                columns: new[] { "Id", "Activo", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "Pantalla" },
                    { 2, true, "Data Show" },
                    { 3, true, "Audio" },
                    { 4, true, "Micrófono" },
                    { 5, true, "Computadora" },
                    { 6, true, "Pizarra" },
                    { 7, true, "Videoconferencia" },
                    { 8, true, "Internet" },
                    { 9, true, "Sin servicios adicionales" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesServicio_ServicioId",
                table: "SolicitudesServicio",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesServicio_SolicitudEventoId_ServicioId",
                table: "SolicitudesServicio",
                columns: new[] { "SolicitudEventoId", "ServicioId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitudesServicio");

            migrationBuilder.DropTable(
                name: "Servicios");
        }
    }
}
