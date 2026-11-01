# Expocentro Web

Plataforma web para **Expocentro**, el centro de convenciones y eventos de la **Cámara de Comercio e Industrias de Cortés (CCIC)**.

El proyecto se desarrolla como parte de una **práctica profesional universitaria** y tiene como objetivo modernizar la presencia digital de Expocentro y facilitar la consulta de sus espacios, servicios y solicitudes de eventos.

---

## Nombre del proyecto

**Expocentro Web**

### Nombre académico propuesto

**Desarrollo de una plataforma web para la promoción de espacios y gestión de solicitudes de eventos de Expocentro CCIC.**

---

## Problema

Expocentro dispone de diferentes espacios destinados a reuniones, capacitaciones, congresos, exposiciones y otros tipos de eventos.

Actualmente, un potencial cliente necesita comunicarse directamente con el personal para obtener información relacionada con:

- Espacios disponibles.
- Características de los salones.
- Capacidades.
- Servicios disponibles.
- Fechas de interés.
- Solicitudes de cotización.

Esto dificulta que una persona pueda conocer rápidamente la oferta de Expocentro antes de establecer contacto con el personal.

Además, la información relacionada con espacios, solicitudes y ocupaciones puede encontrarse distribuida entre diferentes medios de comunicación y procesos internos.

El proyecto busca proporcionar una plataforma centralizada que permita presentar de manera organizada la oferta de Expocentro y apoyar el proceso de atención de solicitudes de eventos.

---

## Propuesta de valor

**Expocentro Web permitirá que potenciales clientes conozcan los espacios y servicios disponibles, seleccionen alternativas de acuerdo con las características de su evento y envíen solicitudes de cotización desde una plataforma accesible desde computadoras y dispositivos móviles.**

Para el personal de Expocentro, la plataforma permitirá centralizar progresivamente la información relacionada con:

- Espacios y salones.
- Fotografías.
- Capacidades.
- Servicios.
- Solicitudes de cotización.
- Seguimiento de solicitudes.
- Calendario de ocupación.
- Eventos confirmados.
- Bloqueos por mantenimiento u otras actividades internas.

La plataforma busca mejorar tanto la experiencia del visitante como la organización interna de la información relacionada con los eventos.

---

## URL de la aplicación

### Producción

**URL:** expocentro-web-dev-evbzhaerggg7h2g4.canadacentral-01.azurewebsites.net



---

## Stack tecnológico

El proyecto será desarrollado principalmente con tecnologías del ecosistema **.NET**, utilizando PostgreSQL como motor de base de datos.

### Backend

**ASP.NET Core 10 MVC**

ASP.NET Core será utilizado como framework principal para desarrollar la aplicación web.

Se seleccionó porque:

- Permite desarrollar aplicaciones web modernas utilizando C#.
- Tiene soporte multiplataforma.
- Ofrece una arquitectura adecuada para aplicaciones empresariales.
- Integra mecanismos de autenticación, autorización, validación y seguridad.
- Puede desplegarse tanto en servidores Linux como Windows.
- Facilita una futura integración con servicios de Microsoft utilizados por CCIC.

La arquitectura MVC permitirá separar las responsabilidades entre modelos, controladores y vistas.

---

### Lenguaje

**C#**

C# será utilizado para implementar la lógica del sistema.

Se seleccionó porque:

- Tiene una integración directa con ASP.NET Core.
- Es ampliamente utilizado en aplicaciones empresariales.
- Permite desarrollar código estructurado y mantenible.
- Cuenta con un ecosistema amplio de librerías y herramientas.

---

### Frontend

**Razor Views + HTML5 + CSS3 + JavaScript**

Las vistas del sistema serán generadas utilizando Razor Views dentro de ASP.NET Core MVC.

Esto permitirá mantener inicialmente el frontend y backend dentro de una sola aplicación, reduciendo la complejidad del proyecto.

JavaScript será utilizado para funcionalidades que requieran interacción en el navegador, como:

- Filtros.
- Calendarios.
- Formularios dinámicos.
- Galerías.
- Plano interactivo de Expocentro.

---

### Framework de interfaz

**Bootstrap 5**

Bootstrap será utilizado como base para desarrollar una interfaz adaptable a diferentes tamaños de pantalla.

Se seleccionó porque:

- Facilita la creación de interfaces responsive.
- Permite desarrollar rápidamente formularios, menús, tarjetas y componentes.
- Reduce el tiempo necesario para construir la interfaz inicial.
- Puede personalizarse de acuerdo con la identidad visual de Expocentro y CCIC.

---

### Base de datos

**PostgreSQL**

PostgreSQL será el motor de base de datos principal del proyecto.

Será utilizado para almacenar información relacionada con:

- Espacios.
- Salones.
- Capacidades.
- Servicios.
- Solicitudes de cotización.
- Clientes.
- Eventos.
- Ocupaciones.
- Usuarios.
- Seguimiento de solicitudes.

Se seleccionó PostgreSQL porque:

- Es un sistema de base de datos relacional robusto.
- Es de código abierto.
- No requiere el pago de licencias del motor para utilizarse en producción.
- Es ampliamente utilizado en aplicaciones web modernas.
- Puede ejecutarse tanto localmente como en servidores y plataformas cloud.
- Permite escalar el sistema si aumenta la cantidad de información o usuarios.
- Tiene soporte para características avanzadas que pueden ser útiles para la gestión de fechas, horarios y ocupaciones.
- Permite mantener independencia respecto al proveedor de infraestructura donde se aloje la aplicación.

---

### ORM y acceso a datos

**Entity Framework Core 10**

Entity Framework Core será utilizado como ORM para comunicar la aplicación ASP.NET Core con PostgreSQL.

La conexión se realizará mediante el proveedor:

```text
Npgsql.EntityFrameworkCore.PostgreSQL
```

Se utilizará porque permite:

- Trabajar con la base de datos utilizando clases de C#.
- Reducir la cantidad de consultas SQL escritas manualmente.
- Implementar migraciones de base de datos.
- Mantener sincronizado el modelo de datos con la aplicación.
- Facilitar el mantenimiento y evolución del sistema.

---

### Autenticación

**Microsoft Entra ID**

En una etapa posterior, el panel administrativo podrá integrarse con Microsoft Entra ID para permitir que usuarios autorizados de CCIC accedan utilizando sus cuentas institucionales de Microsoft 365.

Esto evitaría tener que administrar contraseñas independientes dentro de la plataforma para el personal interno.

---

### Calendario

**FullCalendar**

FullCalendar se utilizará para visualizar la ocupación de los espacios de Expocentro.

Permitirá mostrar información relacionada con:

- Eventos.
- Reservaciones confirmadas.
- Montaje.
- Desmontaje.
- Mantenimiento.
- Bloqueos de espacios.

Inicialmente se utilizarán las funcionalidades disponibles en su versión estándar.

---

### Control de versiones

**Git + GitHub**

Git será utilizado para llevar el control de versiones del proyecto.

GitHub será utilizado como repositorio remoto para:

- Mantener el código fuente.
- Registrar el historial de cambios.
- Realizar commits durante el desarrollo.
- Gestionar versiones del proyecto.
- Documentar avances de la práctica profesional.
- Facilitar futuras implementaciones de integración y despliegue continuo.

---

## Resumen del stack

| Componente | Tecnología |
|---|---|
| Framework | ASP.NET Core 10 MVC |
| Lenguaje | C# |
| Frontend | Razor Views |
| Diseño | Bootstrap 5 |
| Interactividad | JavaScript |
| Base de datos | PostgreSQL |
| ORM | Entity Framework Core 10 |
| Provider PostgreSQL | Npgsql |
| Autenticación futura | Microsoft Entra ID |
| Calendario | FullCalendar |
| Control de versiones | Git |
| Repositorio | GitHub |

---

## Arquitectura general

```text
                 USUARIOS
                    |
                    v
           +------------------+
           |   Navegador Web  |
           +------------------+
                    |
                    v
         +-----------------------+
         | ASP.NET Core 10 MVC   |
         |                       |
         | Controllers           |
         | Views                 |
         | Models / Services     |
         +-----------------------+
                    |
                    v
          +---------------------+
          | Entity Framework    |
          | Core + Npgsql       |
          +---------------------+
                    |
                    v
          +---------------------+
          |     PostgreSQL      |
          +---------------------+
```

---

## Funcionalidades planificadas

### Sitio público

- Página principal de Expocentro.
- Catálogo de espacios.
- Fotografías de instalaciones.
- Información de capacidades.
- Información de servicios.
- Ubicación y contacto.
- Formulario de solicitud de cotización.
- Diseño responsive.
- Plano interactivo de instalaciones.

### Panel administrativo

- Administración de espacios.
- Administración de fotografías.
- Administración de servicios.
- Consulta de solicitudes.
- Seguimiento de solicitudes.
- Registro de eventos.
- Calendario de ocupación.
- Bloqueos de espacios.
- Control de acceso para usuarios autorizados.

---

## Estado actual

**En desarrollo.**

El proyecto se encuentra en su etapa inicial de análisis, configuración del repositorio, definición de arquitectura y desarrollo de la primera versión funcional.

---

## Autor

Luis Fernando Orellana Diaz
Proyecto desarrollado para Expocentro de la **Cámara de Comercio e Industrias de Cortés — CCIC**.