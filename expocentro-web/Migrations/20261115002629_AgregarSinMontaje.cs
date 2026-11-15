using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Expocentro.Web.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSinMontaje : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TiposMontaje",
                columns: new[] { "Id", "Activo", "Nombre" },
                values: new object[] { 10, true, "Sin montaje" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TiposMontaje",
                keyColumn: "Id",
                keyValue: 10);
        }
    }
}
