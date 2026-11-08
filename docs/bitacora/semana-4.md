1. Diseño del modelo de datos

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

2. Configuración de PostgreSQL y Entity Framework Core

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

3. Creación y aplicación de migraciones

Se verificó que Entity Framework reconociera correctamente el contexto mediante:

dotnet ef dbcontext list

El resultado confirmó:

Expocentro.Web.Data.ApplicationDbContext

Posteriormente se generaron y aplicaron las migraciones necesarias para crear el esquema de la base de datos en PostgreSQL.

También se realizó una actualización de las herramientas de Entity Framework para mantener compatibilidad con la versión utilizada por el proyecto.

4. Configuración de datos iniciales

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

5. Implementación inicial de solicitudes

Se creó un formulario básico para registrar eventos internos con información como:

Nombre del evento.

Fecha de inicio.

Fecha de finalización.

Hora de inicio.

Hora de finalización.

Salón.

Cantidad de personas.

Otros requerimientos.

Comentarios.

También se creó el controlador:

SolicitudesController

para manejar:

/Solicitudes
/Solicitudes/Create

Las solicitudes nuevas se almacenan inicialmente con estado:

Borrador

6. Corrección en el manejo de fechas

Durante las pruebas se presentó un error relacionado con PostgreSQL y Npgsql:

Cannot write DateTime with Kind=Unspecified
to PostgreSQL type 'timestamp with time zone'

Se determinó que las fechas y horas de los eventos representan horarios locales y no instantes UTC.

Por esta razón se configuraron:

FechaHoraInicio
FechaHoraFin

como:

timestamp without time zone

Mientras que las fechas de auditoría:

FechaCreacion
FechaActualizacion

continúan manejándose en UTC.

7. Manejo de procesos bloqueados durante la compilación

Durante varias ejecuciones se presentó el error:

The process cannot access the file Expocentro.Web.exe
because it is being used by another process

Se identificó que existían instancias anteriores de la aplicación todavía en ejecución.

La solución utilizada fue detener la aplicación mediante:

Ctrl + C

o finalizar el proceso correspondiente desde PowerShell.

Se definió como práctica de trabajo utilizar la secuencia:

Detener aplicación
↓
Guardar cambios
↓
dotnet build
↓
dotnet run