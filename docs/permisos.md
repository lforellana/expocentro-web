# Matriz de permisos

Roles de la aplicación (definidos en `Seguridad/Roles.cs`):

| Rol | Quién | Cómo se asigna |
|---|---|---|
| **Solicitante** | Cualquier empleado de la CCIC | Automático la primera vez que inicia sesión con Entra ID |
| **Coordinador** | Personal de Coordinación de eventos | *App role* `Coordinador` en Entra ID **o** pantalla *Usuarios* |
| **Administrador** | Responsable del sistema (TI) | *App role* `Administrador` en Entra ID **o** pantalla *Usuarios* |

Los roles efectivos son la **unión** del rol guardado en la tabla `Usuarios` y los *App roles* del token de Entra ID.
Al iniciar sesión, si el *App role* de Entra ID es mayor que el rol guardado
(Solicitante < Coordinador < Administrador), se actualiza el rol en la tabla `Usuarios`; así, por ejemplo,
el usuario empieza a recibir las notificaciones de Coordinación. El rol guardado **nunca se baja**
automáticamente: si se quita el *App role* en Entra ID, hay que bajar el rol también en la pantalla *Usuarios*.
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
| Administrar salones (capacidad, salones combinados, activo) | ❌ | ❌ | ❌ | ✅ |

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
- **Salones combinados:** un salón que *forma parte de* otro (ej. *Salon Forjadores 1* de
  *Salon Forjadores Completo*) choca con el combinado, y el combinado choca con todas sus partes.
  Las partes entre sí (Forjadores 1 y Forjadores 2) no chocan.
- La cantidad de personas no puede superar la capacidad máxima del salón. Si la capacidad está
  *por definir*, no se valida el aforo.
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
