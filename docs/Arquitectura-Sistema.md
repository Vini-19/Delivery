flowchart LR

    U[Usuario del restaurante]

    A[Frontend Angular]

    API[Delivery.API<br/>ASP.NET Core]

    KP[KafkaProducerService]

    K[(Kafka<br/>Topic: pedidos-delivery)]

    C[Delivery.Consumer<br/>Worker Service]

    M[(MongoDB<br/>Base: delivery<br/>Colección: pedidos)]

    P[Scripts Python<br/>Carga masiva y simulación]

    D[Docker Compose]

    U --> A

    A -->|HTTP POST, GET y PATCH| API

    P -->|Solicitudes HTTP masivas| API

    API -->|Publica pedido| KP

    KP -->|Mensaje JSON| K

    K -->|Consume mensajes| C

    C -->|Guarda pedidos| M

    API -->|Consulta y actualiza pedidos| M

    M -->|Resultados paginados| API

    API -->|JSON| A

    D -.-> K
    D -.-> M