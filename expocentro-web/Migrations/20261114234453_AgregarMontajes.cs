using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Expocentro.Web.Migrations
{
    /// <inheritdoc />
    public partial class AgregarMontajes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TiposMontaje",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposMontaje", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesMontaje",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SolicitudEventoId = table.Column<int>(type: "integer", nullable: false),
                    TipoMontajeId = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesMontaje", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesMontaje_SolicitudesEvento_SolicitudEventoId",
                        column: x => x.SolicitudEventoId,
                        principalTable: "SolicitudesEvento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesMontaje_TiposMontaje_TipoMontajeId",
                        column: x => x.TipoMontajeId,
                        principalTable: "TiposMontaje",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TiposMontaje",
                columns: new[] { "Id", "Activo", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "Auditorio" },
                    { 2, true, "Escuela" },
                    { 3, true, "Banquete" },
                    { 4, true, "Imperial" },
                    { 5, true, "Mesa en U" },
                    { 6, true, "Mesa principal" },
                    { 7, true, "Mesa de inscripción" },
                    { 8, true, "Mesa coctelera" },
                    { 9, true, "Buffet" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesMontaje_SolicitudEventoId_TipoMontajeId",
                table: "SolicitudesMontaje",
                columns: new[] { "SolicitudEventoId", "TipoMontajeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesMontaje_TipoMontajeId",
                table: "SolicitudesMontaje",
                column: "TipoMontajeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitudesMontaje");

            migrationBuilder.DropTable(
                name: "TiposMontaje");
        }
    }
}
