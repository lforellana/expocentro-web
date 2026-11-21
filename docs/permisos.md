# Matriz de permisos

Roles de la aplicación (definidos en `Seguridad/Roles.cs`):

| Rol | Quién | Cómo se asigna |
|---|---|---|
| **Solicitante** | Cualquier empleado de la CCIC | Automático la primera vez que inicia sesión con Entra ID |
| **Coordinador** | Personal de Coordinación de eventos | *App role* `Coordinador` en Entra ID **o** pantalla *Usuarios* |
| **Administrador** | Responsable del sistema (TI) | *App role* `Administrador` en Entra ID **o** pantalla *Usuarios* |

Los roles efectivos son la **unión** del rol guardado en la tabla `Usuarios` y los *App roles* del token de Entra ID.
Un usuario marcado como inactivo no puede iniciar sesión.

## Acciones por rol

Leyenda: ✅ permitido · 🔒 solo sobre datos propios · ❌ no permitido

| Acción | Anónimo | Solicitante | Coordinador | Administrador |
|---|:-:|:-:|:-:|:-:|
| Ver espacios, servicios, cómo llegar, cotización | ✅ | ✅ | ✅ | ✅ |
| Ver panel de inicio | ❌ | ✅ | ✅ | ✅ |
| Ver calendario de ocupación de salones | ❌ | ✅ | ✅ | ✅ |
| Crear solicitud de evento interno | ❌ | ✅ | ✅ | ✅ |
| Editar solicitud (datos, montaje, servicios, alimentación) | ❌ | 🔒 en Borrador | 🔒 en Borrador | 🔒 en Borrador |
| Enviar solicitud a Coordinación | ❌ | 🔒 | 🔒 | 🔒 |
| Ver detalle e historial de una solicitud | ❌ | 🔒 | ✅ todas | ✅ todas |
| Cancelar solicitud | ❌ | 🔒 antes de que termine el evento | ✅ con motivo | ✅ con motivo |
| Ver bandeja de Coordinación | ❌ | ❌ | ✅ | ✅ |
| Tomar en revisión / aprobar / rechazar | ❌ | ❌ | ✅ | ✅ |
| Registrar y editar eventos externos | ❌ | ❌ | ✅ | ✅ |
| Administrar usuarios (rol, departamento, activo) | ❌ | ❌ | ❌ | ✅ |

## Reglas de propiedad del dato

- Un solicitante solo ve y modifica solicitudes donde `SolicitanteUsuarioId` es su propio Id
  (filtro en `SolicitudesController.MisSolicitudes()`); las ajenas devuelven 404.
- Solo se modifica una solicitud en estado **Borrador**. Después del envío, solo cambia de estado.
- En el calendario, todos ven la ocupación de los salones, pero solo pueden abrir el detalle de
  sus propias solicitudes (Coordinación puede abrir todas).
- Todo cambio de estado queda en `HistorialEstadosSolicitud` con el nombre de quien lo hizo.
- Un administrador no puede quitarse a sí mismo el rol de administrador ni desactivarse.

## Reglas de disponibilidad

- Un evento **Aprobado** (interno o externo) bloquea el salón: no se puede crear, enviar ni aprobar
  otro evento que se traslape en el mismo salón.
- Las solicitudes **Enviadas** o **En revisión** que se traslapan solo generan advertencia;
  Coordinación decide cuál aprobar.
- La cantidad de personas no puede superar la capacidad máxima del salón.
- Los eventos **Cancelados** o **Rechazados** liberan el salón.

## Flujo de estados

```
Borrador ──enviar──▶ Enviada ──revisar──▶ En revisión
                        │                    │
                        ├──aprobar───────────┼──▶ Aprobada
                        └──rechazar──────────┴──▶ Rechazada

Borrador / Enviada / En revisión / Aprobada ──cancelar──▶ Cancelada
Evento externo: se registra directamente como Aprobada.
```
