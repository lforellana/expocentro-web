using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Expocentro.Web.Migrations
{
    /// <inheritdoc />
    public partial class SalonesDepartamentosCCIC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Salones",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "CapacidadMaxima",
                table: "Salones",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "SalonPadreId",
                table: "Salones",
                type: "integer",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Departamentos",
                columns: new[] { "Id", "Activo", "Nombre" },
                values: new object[,]
                {
                    { 6, true, "Logística" },
                    { 7, true, "Presidencia" },
                    { 8, true, "Recursos Humanos y RSE" },
                    { 9, true, "Servicios Generales" },
                    { 10, true, "Bazar del Sábado" },
                    { 11, true, "Eventos" },
                    { 12, true, "Cobros" },
                    { 13, true, "Comunicaciones y Mercadeo" },
                    { 14, true, "Compras" },
                    { 15, true, "Contabilidad" },
                    { 16, true, "Legal" }
                });

            migrationBuilder.UpdateData(
                table: "Salones",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Nombre", "SalonPadreId" },
                values: new object[] { "Centro de convenciones Felipe Arguello", null });

            migrationBuilder.UpdateData(
                table: "Salones",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Nombre", "SalonPadreId" },
                values: new object[] { "Edificio Los Zorzales", null });

            migrationBuilder.InsertData(
                table: "Salones",
                columns: new[] { "Id", "Activo", "CapacidadMaxima", "Descripcion", "Nombre", "SalonPadreId" },
                values: new object[,]
                {
                    { 3, true, null, null, "Sala Rafael Pastor", null },
                    { 4, true, null, null, "Sala Mario Belot", null },
                    { 5, true, null, null, "Sala Alberto Smit", null },
                    { 6, true, null, null, "Sala VIP #1 Carlos Katan", null },
                    { 7, true, null, null, "Sala VIP #2 Roberto Reyes Silva", null },
                    { 11, true, null, null, "Sala de Juntas Cervecería Hondureña", null },
                    { 12, true, null, null, "Edificio Los Laureles", null },
                    { 14, true, null, null, "SALA VIP #3", null },
                    { 15, true, null, null, "SALA VIP #4", null },
                    { 16, true, null, null, "Centro de Convenciones Emprendedores", null },
                    { 18, true, null, null, "Salon Forjadores Completo", null },
                    { 8, true, null, null, "Salon Forjadores 1", 18 },
                    { 9, true, null, null, "Salon Forjadores 2", 18 },
                    { 10, true, null, null, "Salon Forjadores 3", 18 },
                    { 13, true, null, null, "Emprendedores 2", 16 },
                    { 17, true, null, null, "Emprendedores 1", 16 },
                    { 19, true, null, null, "Emprendedores 3", 16 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Salones_SalonPadreId",
                table: "Salones",
                column: "SalonPadreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Salones_Salones_SalonPadreId",
                table: "Salones",
                column: "SalonPadreId",
                principalTable: "Salones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Los Ids sembrados no avanzan las secuencias de PostgreSQL
            foreach (var tabla in new[] { "Salones", "Departamentos" })
            {
                migrationBuilder.Sql(
                    $"SELECT setval(pg_get_serial_sequence('\"{tabla}\"', 'Id'), " +
                    $"GREATEST((SELECT COALESCE(MAX(\"Id\"), 0) FROM \"{tabla}\"), 1));");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Salones_Salones_SalonPadreId",
                table: "Salones");

            migrationBuilder.DropIndex(
                name: "IX_Salones_SalonPadreId",
                table: "Salones");

            migrationBuilder.DeleteData(
                table: "Departamentos",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Departamentos",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Departamentos",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Departamentos",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Departamentos",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Departamentos",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Departamentos",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Departamentos",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Departamentos",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Departamentos",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Departamentos",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Salones",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Salones",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Salones",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Salones",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Salones",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Salones",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Salones",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Salones",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Salones",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Salones",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Salones",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Salones",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Salones",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Salones",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Salones",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Salones",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Salones",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DropColumn(
                name: "SalonPadreId",
                table: "Salones");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Salones",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<int>(
                name: "CapacidadMaxima",
                table: "Salones",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Salones",
                keyColumn: "Id",
                keyValue: 1,
                column: "Nombre",
                value: "Centro de Convenciones Felipe Arguello");

            migrationBuilder.UpdateData(
                table: "Salones",
                keyColumn: "Id",
                keyValue: 2,
                column: "Nombre",
                value: "Los Zorzales");
        }
    }
}
