Diagrama Entidad-Relación


    DEPARTAMENTOS ||--o{ USUARIOS : "agrupa"

    USUARIOS ||--o{ SOLICITUDES_EVENTO : "solicita"

    USUARIOS ||--o{ SOLICITUDES_EVENTO : "es responsable"

    DEPARTAMENTOS ||--o{ SOLICITUDES_EVENTO : "se carga a"

    SALONES ||--o{ SOLICITUDES_EVENTO : "se solicita para"

    ESTADOS_SOLICITUD ||--o{ SOLICITUDES_EVENTO : "define estado actual"


    SOLICITUDES_EVENTO ||--o{ SOLICITUD_MONTAJES : "requiere"

    TIPOS_MONTAJE ||--o{ SOLICITUD_MONTAJES : "se utiliza en"


    SOLICITUDES_EVENTO ||--o{ SOLICITUD_SERVICIOS : "requiere"

    SERVICIOS ||--o{ SOLICITUD_SERVICIOS : "se solicita en"


    SOLICITUDES_EVENTO ||--o| ALIMENTACION_EVENTO : "puede incluir"


    SOLICITUDES_EVENTO ||--o{ HISTORIAL_ESTADOS : "genera"

    ESTADOS_SOLICITUD ||--o{ HISTORIAL_ESTADOS : "registra"

    USUARIOS ||--o{ HISTORIAL_ESTADOS : "actualiza"



     DEPARTAMENTOS {
        int id PK
        varchar nombre
        boolean activo
    }

    USUARIOS {
        int id PK
        varchar entra_object_id UK
        varchar nombre
        varchar correo
        int departamento_id FK
        varchar rol
        boolean activo
    }

    SALONES {
        int id PK
        varchar nombre
        text descripcion
        int capacidad_maxima
        boolean activo
    }

    ESTADOS_SOLICITUD {
        int id PK
        varchar nombre
        varchar descripcion
        boolean activo
    }

    SOLICITUDES_EVENTO {
        int id PK
        varchar codigo UK
        varchar nombre_evento
        timestamp fecha_hora_inicio
        timestamp fecha_hora_fin
        int cantidad_personas
        int solicitante_usuario_id FK
        int responsable_usuario_id FK
        int salon_id FK
        int departamento_cargo_id FK
        int estado_id FK
        text otros
        text comentarios
        timestamp fecha_creacion
        timestamp fecha_actualizacion
    }

    TIPOS_MONTAJE {
        int id PK
        varchar nombre
        boolean requiere_cantidad
        varchar unidad_cantidad
        boolean activo
    }

    SOLICITUD_MONTAJES {
        int id PK
        int solicitud_evento_id FK
        int tipo_montaje_id FK
        int cantidad
        varchar observaciones
    }

    SERVICIOS {
        int id PK
        varchar nombre
        varchar categoria
        boolean requiere_cantidad
        varchar unidad_cantidad
        boolean activo
    }

    SOLICITUD_SERVICIOS {
        int id PK
        int solicitud_evento_id FK
        int servicio_id FK
        int cantidad
        varchar observaciones
    }

    ALIMENTACION_EVENTO {
        int id PK
        int solicitud_evento_id FK
        boolean requiere_alimentacion
        text detalle_menu
        int cantidad_personas
        numeric precio_menu
        time horario_servicio
        text observaciones
    }

    HISTORIAL_ESTADOS {
        int id PK
        int solicitud_evento_id FK
        int estado_id FK
        int usuario_id FK
        text comentario
        timestamp fecha_cambio
    }


    DEPARTAMENTOS ||--o{ USUARIOS : "tiene"

    USUARIOS ||--o{ SOLICITUDES_EVENTO : "crea"

    USUARIOS ||--o{ SOLICITUDES_EVENTO : "responsable"

    DEPARTAMENTOS ||--o{ SOLICITUDES_EVENTO : "carga"

    SALONES ||--o{ SOLICITUDES_EVENTO : "recibe"

    ESTADOS_SOLICITUD ||--o{ SOLICITUDES_EVENTO : "estado"


    SOLICITUDES_EVENTO ||--o{ SOLICITUD_MONTAJES : "incluye"

    TIPOS_MONTAJE ||--o{ SOLICITUD_MONTAJES : "tipo"


    SOLICITUDES_EVENTO ||--o{ SOLICITUD_SERVICIOS : "incluye"

    SERVICIOS ||--o{ SOLICITUD_SERVICIOS : "servicio"


    SOLICITUDES_EVENTO ||--o| ALIMENTACION_EVENTO : "alimentacion"


    SOLICITUDES_EVENTO ||--o{ HISTORIAL_ESTADOS : "historial"

    ESTADOS_SOLICITUD ||--o{ HISTORIAL_ESTADOS : "estado"

    USUARIOS ||--o{ HISTORIAL_ESTADOS : "realiza"