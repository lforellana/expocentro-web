# Configuración de Microsoft Entra ID

El sistema usa el inicio de sesión de Microsoft (cuentas de empleados de la CCIC) mediante
`Microsoft.Identity.Web`. Si la sección `AzureAd:ClientId` está vacía, la aplicación usa un
**acceso de prueba** (elegir un usuario demo), pero solo en *Development* o cuando
`Autenticacion:PermitirAccesoDesarrollo=true`. En cualquier otro caso la app no arranca.

## 1. Registrar la aplicación

En [portal.azure.com](https://portal.azure.com) → **Microsoft Entra ID** → **Registros de aplicaciones** → **Nuevo registro**:

| Campo | Valor |
|---|---|
| Nombre | `Expocentro Web` |
| Tipos de cuenta | *Solo cuentas de este directorio organizativo* (un solo inquilino) |
| URI de redirección (Web) | `https://<su-app>.azurewebsites.net/signin-oidc` |

Agregar también en **Autenticación**:

- URI de redirección local: `https://localhost:7171/signin-oidc`
- URL de cierre de sesión del canal frontal: `https://<su-app>.azurewebsites.net/signout-callback-oidc`
- Marcar **Tokens de id.**

## 2. Crear un secreto (opcional por ahora)

El inicio de sesión usa el flujo de *token de id.* (`response_type=id_token`), que **no necesita secreto**.
Solo hará falta cuando el sistema llame a Microsoft Graph (por ejemplo, para enviar correos).
Si se crea: **Certificados y secretos** → **Nuevo secreto de cliente** y copiar el *valor*, que solo se
muestra una vez. Anotar la fecha de vencimiento.

## 3. Definir los roles

**Roles de aplicación** → **Crear rol de aplicación** (tipo de miembros permitidos: *Usuarios/Grupos*):

| Nombre para mostrar | Valor |
|---|---|
| Coordinador | `Coordinador` |
| Administrador | `Administrador` |

No hace falta crear el rol *Solicitante*: todo empleado que inicia sesión lo tiene.

Asignar los roles en **Aplicaciones empresariales** → `Expocentro Web` → **Usuarios y grupos**.
También se pueden asignar desde la pantalla *Usuarios* del sistema (rol Administrador).

> Opcional: en **Aplicaciones empresariales → Propiedades**, activar *¿Se requiere asignación?* para
> que solo los usuarios asignados puedan entrar.

## 4. Configurar la aplicación

Valores que se necesitan: **Id. de directorio (inquilino)**, **Id. de aplicación (cliente)** y el **secreto**.

### Local (user-secrets, no se suben al repositorio)

```bash
cd expocentro-web
dotnet user-secrets set "AzureAd:TenantId" "<tenant-id>"
dotnet user-secrets set "AzureAd:ClientId" "<client-id>"
dotnet user-secrets set "AzureAd:ClientSecret" "<secreto>"   # opcional
```

Usar el perfil `https` (`dotnet run --launch-profile https`), ya que la URI de redirección es HTTPS.
Para volver al acceso de prueba local: `dotnet user-secrets remove "AzureAd:ClientId"`.

### Azure App Service

**Configuración → Variables de entorno** (doble guion bajo = sección):

| Nombre | Valor |
|---|---|
| `AzureAd__TenantId` | `<tenant-id>` |
| `AzureAd__ClientId` | `<client-id>` |
| `AzureAd__ClientSecret` | `<secreto>` (idealmente referencia a Key Vault) |
| `ConnectionStrings__DefaultConnection` | cadena de PostgreSQL |

Mientras Entra ID no esté listo, en el ambiente **de prueba** se puede usar
`Autenticacion__PermitirAccesoDesarrollo = true`. **Nunca en producción**: cualquiera podría entrar como
coordinador o administrador.

## 5. Primer administrador

1. Iniciar sesión una vez con la cuenta que será administradora (se crea como *Solicitante*).
2. Asignarle el *App role* `Administrador` en Entra ID, **o** actualizarlo en la base de datos:

```sql
UPDATE "Usuarios" SET "Rol" = 'Administrador' WHERE "Correo" = 'nombre@dominio.org';
```

3. Cerrar sesión y volver a entrar. Desde *Administración → Usuarios* asignará departamento y rol al resto.

## Cómo funciona

- Al validar el token (`OnTokenValidated` en `Program.cs`), `UsuarioSincronizacionService` busca al
  usuario por `oid` (EntraObjectId) o correo, lo crea si no existe (departamento *Sin asignar*) y
  agrega los claims `expocentro:usuario_id` y los roles.
- Los usuarios demo (Id 1–3) de los datos semilla solo sirven para el acceso de prueba.
- Si falla la validación al volver de Microsoft (usuario desactivado, token inválido), se muestra
  `/Cuenta/ErrorInicio` y el detalle queda en el log con la categoría `Expocentro.EntraId`.

## Problemas comunes

| Mensaje de Microsoft | Causa | Solución |
|---|---|---|
| `AADSTS50011` (*redirect URI mismatch*) | La URL de respuesta no está registrada | Agregar exactamente `https://<host>/signin-oidc` en **Autenticación** |
| `AADSTS700054` (*response_type 'id_token' is not enabled*) | No se marcó **Tokens de id.** | Marcarlo en **Autenticación** → *Concesión implícita y flujos híbridos* |
| `AADSTS50105` (*not assigned to a role*) | *¿Se requiere asignación?* está activo y el usuario no fue asignado | Asignarlo en **Aplicaciones empresariales → Usuarios y grupos** |
| `AADSTS90002` (*tenant not found*) | `TenantId` incorrecto | Copiar el **Id. de directorio (inquilino)** de la página *Información general* |
| Entra correctamente pero no ve *Coordinación* | El rol se asignó después de iniciar sesión | Cerrar sesión y volver a entrar |
