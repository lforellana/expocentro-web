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