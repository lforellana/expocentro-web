# Notificaciones por correo

El sistema envía correos con **Microsoft Graph** desde un buzón de la CCIC, usando el mismo registro
de Entra ID del inicio de sesión (*Sistema Integrado CCIC*).

## Cuándo se envía un correo

| Evento | Destinatarios |
|---|---|
| Un solicitante **envía** una solicitud | Coordinación |
| Coordinación **aprueba**, **rechaza** o **cancela** una solicitud interna | El solicitante (con el comentario o motivo) |
| El solicitante **cancela** una solicitud ya enviada | Coordinación |

**Coordinación** = los correos de `Notificaciones:CorreosCoordinacion` (por ejemplo, una lista de
distribución) **más** los usuarios activos con rol *Coordinador* en la pantalla *Usuarios*.
Los eventos externos no generan correos al solicitante.

Los correos se envían en segundo plano (hasta 3 intentos). Si Graph falla, la operación del
usuario no se afecta y el error queda en el log. La cola vive en memoria: si la app se reinicia
justo en ese momento, un correo pendiente puede perderse.

## Configuración en Azure (una sola vez)

### 1. Buzón remitente

Crear en Exchange un **buzón compartido**, por ejemplo `eventos@ccichonduras.org`. Los buzones
compartidos no requieren licencia.

### 2. Secreto de cliente

En **App registrations → Sistema Integrado CCIC → Certificates & secrets → New client secret**.
Copiar el *Value* y anotar la fecha de vencimiento (al vencer, los correos dejan de salir).

### 3. Permiso para enviar, limitado al buzón (recomendado)

Con **RBAC para aplicaciones de Exchange Online** la app solo puede enviar desde ese buzón. En
PowerShell, con un administrador de Exchange:

```powershell
Install-Module ExchangeOnlineManagement   # si no está instalado
Connect-ExchangeOnline

# Object ID de la aplicación empresarial (Enterprise applications → Sistema Integrado CCIC → Object ID)
New-ServicePrincipal -AppId 3854439a-5a14-4947-860e-6bcbe42cc106 -ObjectId <object-id-enterprise-app> -DisplayName "Sistema Integrado CCIC"

New-ManagementScope -Name "Buzon Expocentro" -RecipientRestrictionFilter "PrimarySmtpAddress -eq 'eventos@ccichonduras.org'"

New-ManagementRoleAssignment -App 3854439a-5a14-4947-860e-6bcbe42cc106 -Role "Application Mail.Send" -CustomResourceScope "Buzon Expocentro"
```

> **No** agregue además el permiso `Mail.Send` en *API permissions* de Entra ID: ese permiso aplica a
> **todos** los buzones de la organización y anularía la restricción.
>
> Alternativa rápida (menos segura): *API permissions → Add → Microsoft Graph → Application
> permissions → Mail.Send → Grant admin consent*. La app podría enviar como cualquier usuario.

Los cambios de permisos pueden tardar entre 30 minutos y 2 horas en aplicarse.

### 4. Variables del App Service

| Nombre | Valor |
|---|---|
| `AzureAd__ClientSecret` | *Value* del secreto del paso 2 |
| `Notificaciones__Habilitado` | `true` |
| `Notificaciones__Remitente` | `eventos@ccichonduras.org` |
| `Notificaciones__CorreosCoordinacion__0` | correo o lista de Coordinación (opcional; `__1`, `__2`... para más) |
| `Notificaciones__UrlBase` | URL pública de la app (opcional; para los enlaces de los correos) |

Si `Habilitado` es `true` pero falta el remitente o el secreto, la app arranca igual, deja una
advertencia en el log y solo registra los correos sin enviarlos.

## Desarrollo local

Sin configuración, los correos solo aparecen en el log (`[Correo no enviado ...]`). Para ver cómo
se ven, guárdelos como archivos `.html`:

```bash
dotnet user-secrets set "Notificaciones:CarpetaPrueba" "C:\temp\correos"
```

Para enviar de verdad desde local, configure en user-secrets `AzureAd:ClientSecret`,
`Notificaciones:Habilitado=true` y `Notificaciones:Remitente`.
