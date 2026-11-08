Creación Diagrama ER.

Diseño del modelo de datos

Se definió el modelo de datos inicial utilizando PostgreSQL.

Las entidades principales identificadas fueron:

Departamentos.

Usuarios.

Salones.

Estados de solicitud.

Solicitudes de evento.

Tipos de montaje.

Solicitudes de montaje.

Servicios.

Solicitudes de servicios.

Alimentación del evento.

Historial de estados.

Para el primer incremento se decidió implementar únicamente las entidades necesarias para registrar una solicitud básica:

Departamentos
Usuarios
Salones
EstadosSolicitud
SolicitudesEvento

La estructura fue diseñada para permitir futuras ampliaciones sin necesidad de modificar significativamente la tabla principal de solicitudes.

4. Configuración de PostgreSQL y Entity Framework Core

Se configuró el proyecto ASP.NET Core para utilizar:

Entity Framework Core.

Npgsql.

PostgreSQL.

Se creó la clase:

ApplicationDbContext

dentro de:

Data/ApplicationDbContext.cs

También se crearon los modelos:

Departamento.cs
Usuario.cs
Salon.cs
EstadoSolicitud.cs
SolicitudEvento.cs

Se configuraron los DbSet correspondientes dentro de ApplicationDbContext.

5. Creación y aplicación de migraciones

Se verificó que Entity Framework reconociera correctamente el contexto mediante:

dotnet ef dbcontext list

El resultado confirmó:

Expocentro.Web.Data.ApplicationDbContext

Posteriormente se generaron y aplicaron las migraciones necesarias para crear el esquema de la base de datos en PostgreSQL.

También se realizó una actualización de las herramientas de Entity Framework para mantener compatibilidad con la versión utilizada por el proyecto.

6. Configuración de datos iniciales

Se incorporaron datos iniciales mediante HasData() en el método:

OnModelCreating()

de ApplicationDbContext.

Se agregaron registros iniciales para:

Departamentos.

Salones.

Estados de solicitud.

Usuario temporal de prueba.

Los estados definidos actualmente son:

Borrador
Enviada
En revisión
Aprobada
Rechazada

El usuario temporal permitirá desarrollar el flujo funcional antes de incorporar Microsoft Entra ID.