using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Expocentro.Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedCatalogosIniciales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SolicitudesEvento_Usuarios_ResponsableUsuarioId",
                table: "SolicitudesEvento");

            migrationBuilder.DropForeignKey(
                name: "FK_SolicitudesEvento_Usuarios_SolicitanteUsuarioId",
                table: "SolicitudesEvento");

            migrationBuilder.InsertData(
                table: "Departamentos",
                columns: new[] { "Id", "Activo", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "IT" },
                    { 2, true, "Administración" },
                    { 3, true, "Desarrollo Empresarial" },
                    { 4, true, "Capacitaciones" }
                });

            migrationBuilder.InsertData(
                table: "EstadosSolicitud",
                columns: new[] { "Id", "Activo", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "Solicitud todavía no enviada.", "Borrador" },
                    { 2, true, "Solicitud enviada para revisión.", "Enviada" },
                    { 3, true, null, "En revisión" },
                    { 4, true, null, "Aprobada" },
                    { 5, true, null, "Rechazada" }
                });

            migrationBuilder.InsertData(
                table: "Salones",
                columns: new[] { "Id", "Activo", "CapacidadMaxima", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, true, 500, "Salón para eventos y actividades institucionales.", "Centro de Convenciones Felipe Arguello" },
                    { 2, true, 150, "Espacio para reuniones y eventos.", "Los Zorzales" }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Activo", "Correo", "DepartamentoId", "EntraObjectId", "Nombre", "Rol" },
                values: new object[] { 1, true, "usuario.demo@ccichonduras.org", 1, null, "Usuario Demo", "Solicitante" });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Correo",
                table: "Usuarios",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEvento_Codigo",
                table: "SolicitudesEvento",
                column: "Codigo",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SolicitudesEvento_Usuarios_ResponsableUsuarioId",
                table: "SolicitudesEvento",
                column: "ResponsableUsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SolicitudesEvento_Usuarios_SolicitanteUsuarioId",
                table: "SolicitudesEvento",
                column: "SolicitanteUsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SolicitudesEvento_Usuarios_ResponsableUsuarioId",
                table: "SolicitudesEvento");

            migrationBuilder.DropForeignKey(
                name: "FK_SolicitudesEvento_Usuarios_SolicitanteUsuarioId",
                table: "SolicitudesEvento");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Correo",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_SolicitudesEvento_Codigo",
                table: "SolicitudesEvento");

            migrationBuilder.DeleteData(
                table: "Departamentos",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Departamentos",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Departamentos",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "EstadosSolicitud",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "EstadosSolicitud",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "EstadosSolicitud",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "EstadosSolicitud",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "EstadosSolicitud",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Salones",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Salones",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Departamentos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.AddForeignKey(
                name: "FK_SolicitudesEvento_Usuarios_ResponsableUsuarioId",
                table: "SolicitudesEvento",
                column: "ResponsableUsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SolicitudesEvento_Usuarios_SolicitanteUsuarioId",
                table: "SolicitudesEvento",
                column: "SolicitanteUsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
