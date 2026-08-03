Arquitectura del sistema Delivery

Este documento describe la arquitectura general del proyecto Delivery, desarrollado para simular el flujo de pedidos de un restaurante usando Angular, ASP.NET Core, Apache Kafka, un consumidor en segundo plano y MongoDB.

Diagrama general

flowchart LR
    U["Usuario del restaurante"]
    F["Frontend Angular"]
    API["Delivery.API<br/>ASP.NET Core"]
    P["KafkaProducerService"]
    K[("Apache Kafka<br/>Topic: pedidos-delivery")]
    C["Delivery.Consumer<br/>Worker Service"]
    M[("MongoDB<br/>Base: delivery<br/>Colección: pedidos")]
    PY["Scripts Python<br/>Carga masiva y simulación"]
    DK["Docker Compose"]

    U -->|"Usa la aplicación"| F
    F -->|"POST, GET y PATCH"| API
    PY -->|"Pruebas HTTP"| API

    API -->|"Publica el pedido"| P
    P -->|"Mensaje JSON"| K
    K -->|"Entrega mensajes"| C
    C -->|"Guarda pedidos"| M

    API -->|"Consulta y actualiza"| M
    M -->|"Pedidos y estados"| API
    API -->|"Respuesta JSON"| F

    DK -.->|"Levanta"| K
    DK -.->|"Levanta"| M

Flujo de un pedido

sequenceDiagram
    actor Usuario
    participant Angular
    participant API as Delivery.API
    participant Kafka
    participant Consumer as Delivery.Consumer
    participant MongoDB

    Usuario->>Angular: Registra una orden
    Angular->>API: POST /api/Pedidos
    API->>Kafka: Publica el pedido
    API-->>Angular: 202 Accepted
    Kafka->>Consumer: Entrega el mensaje
    Consumer->>MongoDB: Guarda el pedido
    MongoDB-->>Consumer: Confirmación

    Angular->>API: GET /api/Pedidos/Cocina/Pendientes
    API->>MongoDB: Consulta pedidos pendientes
    MongoDB-->>API: Lista de pedidos
    API-->>Angular: Respuesta paginada

    Angular->>API: PATCH /api/Pedidos/{id}/finalizar-cocina
    API->>MongoDB: Cambia Estado a Listo
    MongoDB-->>API: Confirmación
    API-->>Angular: Pedido listo

    Angular->>API: GET /api/Pedidos/delivery/disponibles
    API->>MongoDB: Consulta Estado Listo y Delivery Pendiente
    MongoDB-->>API: Pedidos disponibles
    API-->>Angular: Respuesta paginada

    Angular->>API: PATCH /api/Pedidos/{id}/finalizar-delivery
    API->>MongoDB: Cambia EstadoDelivery a Finalizado
    MongoDB-->>API: Confirmación
    API-->>Angular: Pedido entregado

Componentes principales

Frontend Angular

Se encarga de la interacción con el usuario. Incluye pantallas para:

Crear órdenes.

Consultar el registro de pedidos.

Ver el detalle de cada pedido.

Consultar pedidos pendientes en cocina.

Consultar pedidos disponibles para delivery.

Actualizar los estados de cocina y entrega.

Delivery.API

Expone los endpoints HTTP del sistema. Sus responsabilidades principales son:

Validar los datos recibidos.

Generar el identificador del pedido.

Publicar pedidos nuevos en Kafka.

Consultar pedidos almacenados en MongoDB.

Cambiar los estados de cocina y delivery.

Devolver resultados paginados al frontend.

Apache Kafka

Se utiliza como intermediario de mensajería entre la API y el consumidor.

Configuración principal:

Topic: pedidos-delivery

Particiones: 3

Puerto externo: 9002

Puerto interno de Docker: 9092

Kafka permite que la API reciba una orden sin tener que esperar a que MongoDB complete el guardado.

Delivery.Consumer

Es un Worker Service que permanece escuchando el topic pedidos-delivery.

Cuando recibe un mensaje:

Convierte el JSON al modelo del pedido.

Valida la información recibida.

Guarda el pedido en MongoDB.

Registra en consola si el procesamiento fue correcto o falló.

MongoDB

Almacena los pedidos y sus detalles como documentos.

Configuración usada:

Base de datos: delivery

Colección: pedidos

Puerto del host: 27018

Puerto del contenedor: 27017

Cada pedido contiene sus productos dentro del mismo documento, lo cual simplifica la consulta del detalle completo.

Scripts Python

El proyecto incluye scripts para probar el sistema:

carga_masiva.py: envía una cantidad grande de pedidos a la API.

flujo_restaurante.py: simula el cambio gradual de pedidos entre cocina y delivery.

Estos scripts ayudan a revisar el comportamiento de la API, Kafka, el consumidor y MongoDB bajo carga.

Flujo de estados

stateDiagram-v2
    [*] --> Pendiente
    Pendiente --> Listo: Cocina finaliza el pedido
    Listo --> Finalizado: Delivery entrega el pedido
    Finalizado --> [*]

El sistema maneja dos campos relacionados:

Estado: representa el estado del pedido en cocina.

EstadoDelivery: representa el estado de entrega.

Un pedido aparece en Delivery cuando cumple:

Estado = Listo
EstadoDelivery = Pendiente

Infraestructura con Docker

Docker Compose levanta:

Apache Kafka.

Inicializador del topic.

MongoDB.

Red interna delivery-network.

Volúmenes para conservar datos de Kafka y MongoDB.

La API, el Consumer y el frontend se ejecutan por separado durante el desarrollo.

Resumen de comunicación

Origen

Destino

Comunicación

Angular

Delivery.API

HTTP/JSON

Scripts Python

Delivery.API

HTTP/JSON

Delivery.API

Kafka

Mensajes JSON

Kafka

Delivery.Consumer

Consumo de mensajes

Delivery.Consumer

MongoDB

Driver oficial de MongoDB

Delivery.API

MongoDB

Consultas y actualizaciones

Observación importante

Para probar el flujo completo deben estar ejecutándose al mismo tiempo:

Docker Compose.

Delivery.API.

Delivery.Consumer.

Frontend Angular.

Si el Consumer está detenido, Kafka puede aceptar los pedidos, pero estos no aparecerán en MongoDB hasta que el Consumer vuelva a ejecutarse.