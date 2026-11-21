using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Expocentro.Web.Migrations
{
    /// <inheritdoc />
    public partial class MvpReservasEntraId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SolicitudesEvento_SalonId",
                table: "SolicitudesEvento");

            migrationBuilder.AddColumn<string>(
                name: "ClienteContacto",
                table: "SolicitudesEvento",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClienteCorreo",
                table: "SolicitudesEvento",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClienteNombre",
                table: "SolicitudesEvento",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClienteTelefono",
                table: "SolicitudesEvento",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenciaSap",
                table: "SolicitudesEvento",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoEvento",
                table: "SolicitudesEvento",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.InsertData(
                table: "Departamentos",
                columns: new[] { "Id", "Activo", "Nombre" },
                values: new object[] { 5, true, "Sin asignar" });

            migrationBuilder.InsertData(
                table: "EstadosSolicitud",
                columns: new[] { "Id", "Activo", "Descripcion", "Nombre" },
                values: new object[] { 6, true, "Solicitud cancelada por el solicitante o Coordinación.", "Cancelada" });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Activo", "Correo", "DepartamentoId", "EntraObjectId", "Nombre", "Rol" },
                values: new object[,]
                {
                    { 2, true, "coordinador.demo@ccichonduras.org", 2, null, "Coordinador Demo", "Coordinador" },
                    { 3, true, "admin.demo@ccichonduras.org", 1, null, "Administrador Demo", "Administrador" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_EntraObjectId",
                table: "Usuarios",
                column: "EntraObjectId",
                unique: true,
                filter: "\"EntraObjectId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEvento_ReferenciaSap",
                table: "SolicitudesEvento",
                column: "ReferenciaSap",
                unique: true,
                filter: "\"ReferenciaSap\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEvento_SalonId_FechaHoraInicio_FechaHoraFin",
                table: "SolicitudesEvento",
                columns: new[] { "SalonId", "FechaHoraInicio", "FechaHoraFin" });

            // Los datos semilla insertan Ids explícitos y no avanzan la
            // secuencia identity; se sincroniza para que los usuarios
            // creados desde Entra ID no choquen con los Ids existentes.
            foreach (var tabla in new[] { "Usuarios", "Departamentos", "EstadosSolicitud", "Salones", "Servicios", "TiposMontaje" })
            {
                migrationBuilder.Sql(
                    $"SELECT setval(pg_get_serial_sequence('\"{tabla}\"', 'Id'), " +
                    $"GREATEST((SELECT COALESCE(MAX(\"Id\"), 0) FROM \"{tabla}\"), 1));");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuarios_EntraObjectId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_SolicitudesEvento_ReferenciaSap",
                table: "SolicitudesEvento");

            migrationBuilder.DropIndex(
                name: "IX_SolicitudesEvento_SalonId_FechaHoraInicio_FechaHoraFin",
                table: "SolicitudesEvento");

            migrationBuilder.DeleteData(
                table: "Departamentos",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "EstadosSolicitud",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "ClienteContacto",
                table: "SolicitudesEvento");

            migrationBuilder.DropColumn(
                name: "ClienteCorreo",
                table: "SolicitudesEvento");

            migrationBuilder.DropColumn(
                name: "ClienteNombre",
                table: "SolicitudesEvento");

            migrationBuilder.DropColumn(
                name: "ClienteTelefono",
                table: "SolicitudesEvento");

            migrationBuilder.DropColumn(
                name: "ReferenciaSap",
                table: "SolicitudesEvento");

            migrationBuilder.DropColumn(
                name: "TipoEvento",
                table: "SolicitudesEvento");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEvento_SalonId",
                table: "SolicitudesEvento",
                column: "SalonId");
        }
    }
}
