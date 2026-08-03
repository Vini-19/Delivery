Delivery - Sistema de pedidos con Kafka y MongoDB

Proyecto académico desarrollado para la clase de Big Data. La idea principal es simular el flujo de pedidos de un restaurante utilizando una arquitectura desacoplada:

El cliente registra una orden desde Angular.

La API publica el pedido en Apache Kafka.

Un Worker consume el mensaje.

El pedido se guarda en MongoDB.

Cocina cambia el pedido a Listo.

Delivery lo marca como Finalizado.

No se pretende presentar como un sistema listo para producción. Es un proyecto para practicar mensajería, persistencia NoSQL, procesamiento asíncrono, pruebas de carga y separación de responsabilidades.

Tecnologías utilizadas

Backend

.NET 10

ASP.NET Core Web API

Worker Service

Confluent.Kafka 2.15.0

MongoDB.Driver 3.10.0

Swagger / OpenAPI

Frontend

Angular 21

TypeScript 5.9

Bootstrap 5.3

RxJS

ngx-toastr

Infraestructura y pruebas

Apache Kafka 4.3.1

MongoDB 8.0

Docker Compose

Python 3

aiohttp

Arquitectura general

flowchart LR
    A[Angular] -->|POST pedido| B[Delivery API]
    B -->|Publica mensaje| C[Kafka]
    C -->|Consume mensaje| D[Delivery Consumer]
    D -->|Guarda pedido| E[(MongoDB)]
    A -->|Consulta pedidos| B
    B -->|Lee y actualiza| E

La API no guarda directamente el pedido nuevo en MongoDB. Primero lo publica en Kafka y devuelve 202 Accepted. El guardado se completa cuando Delivery.Consumer procesa el mensaje.

Esto significa que para probar correctamente el proyecto deben estar ejecutándose la API y el Consumer al mismo tiempo.

Flujo de estados

Pedido creado
     |
     v
Estado = Pendiente
EstadoDelivery = Pendiente
     |
     | Cocina termina el pedido
     v
Estado = Listo
EstadoDelivery = Pendiente
     |
     | Delivery entrega el pedido
     v
Estado = Listo
EstadoDelivery = Finalizado
Finalizado = fecha UTC

Los pedidos disponibles para cocina cumplen:

Estado = Pendiente
EstadoDelivery = Pendiente

Los pedidos disponibles para delivery cumplen:

Estado = Listo
EstadoDelivery = Pendiente

Estructura del repositorio

Delivery/
|
|-- BACKEND/
|   |-- Delivery.API/
|   |-- Delivery.Consumer/
|   |-- Delivery.Infraestructura/
|   `-- Delivery.Shared/
|
|-- Fronted/
|   `-- src/
|       |-- Models/
|       |-- app/
|       `-- environments/
|
|-- archivopy/
|   |-- carga_masiva.py
|   `-- flujo_restaurante.py
|
|-- docker-compose.yml
|-- mongo-init.js
|-- CrearUsuaioDB.txt
`-- README.md

La carpeta se llama actualmente Fronted y no Frontend. Los comandos de este documento utilizan el nombre que existe en el repositorio.

Proyectos del backend

Delivery.API

Expone los endpoints REST, valida los pedidos, genera el ObjectId y publica el mensaje en Kafka.

Delivery.Consumer

Es un Worker que escucha el topic pedidos-delivery, valida el mensaje y lo guarda en MongoDB. El commit de Kafka se realiza después de procesar el pedido.

Delivery.Infraestructura

Contiene la configuración de MongoDB y la implementación de PedidosRepositorio.

Delivery.Shared

Contiene los modelos compartidos y la interfaz IPedidosRepositorio.

Pantallas del frontend

Inicio.

Crear orden.

Registro general de pedidos.

Pedidos pendientes de cocina.

Pedidos disponibles para delivery.

Consulta del detalle de un pedido.

Las pantallas de cocina y delivery consultan periódicamente la API para reflejar cambios sin tener que recargar manualmente la página.

Requisitos

Antes de iniciar se necesita:

Git.

Docker Desktop.

SDK de .NET 10.

Node.js y npm compatibles con Angular 21.

Python 3, solamente para las pruebas de carga.

Comprobaciones rápidas:

docker --version
docker compose version
dotnet --version
node --version
npm --version
python --version

Clonar el repositorio

git clone https://github.com/Vini-19/Delivery.git
cd Delivery

1. Levantar Kafka y MongoDB

Desde la raíz del repositorio:

docker compose up -d

Verificar los contenedores:

docker compose ps

También se puede utilizar:

docker ps

Los contenedores principales son:

Servicio

Contenedor

Puerto local

Kafka

delivery-kafka

9002

MongoDB

delivery-mongo

27018

El contenedor delivery-kafka-init se ejecuta una vez para crear el topic:

pedidos-delivery

El topic se crea con tres particiones y factor de replicación 1.

Verificar el topic

docker exec -it delivery-kafka /opt/kafka/bin/kafka-topics.sh --bootstrap-server localhost:9092 --list

Verificar MongoDB

docker exec -it delivery-mongo mongosh -u admin -p Mongo123456 --authenticationDatabase admin

Dentro de mongosh:

use delivery
show collections
db.pedidos.countDocuments({})

Para salir:

exit

Configuración local de MongoDB

El archivo mongo-init.js crea:

Base de datos: delivery
Colección: pedidos
Usuario aplicación: delivery_app
Contraseña aplicación: DeliveryMongo123
Mecanismo: SCRAM-SHA-256

La cadena utilizada por la API y el Consumer es:

mongodb://delivery_app:DeliveryMongo123@localhost:27018/delivery?authSource=delivery&authMechanism=SCRAM-SHA-256

MongoDB también crea índices para ordenar y filtrar por fecha y estados.

El script de inicialización de MongoDB se ejecuta cuando el volumen se crea por primera vez. Si ya existe el volumen y se cambia mongo-init.js, el script no se vuelve a ejecutar automáticamente.

Si el usuario delivery_app no existe, se pueden seguir los comandos incluidos en CrearUsuaioDB.txt.

2. Ejecutar la API

Desde la raíz:

dotnet restore BACKEND/Delivery.API/Delivery.API.csproj
dotnet run --project BACKEND/Delivery.API/Delivery.API.csproj --launch-profile https

Direcciones locales:

Swagger: https://localhost:7159/swagger
API HTTPS: https://localhost:7159
API HTTP:  http://localhost:5159

La API permite solicitudes CORS desde:

http://localhost:4200

3. Ejecutar el Consumer

Abrir otra terminal y ejecutar:

dotnet restore BACKEND/Delivery.Consumer/Delivery.Consumer.csproj
dotnet run --project BACKEND/Delivery.Consumer/Delivery.Consumer.csproj

El Consumer utiliza:

BootstrapServers: localhost:9002
Topic: pedidos-delivery
GroupId: delivery-pedidos-consumer

Cuando recibe un pedido válido debe aparecer un mensaje similar a:

Pedido <id> guardado para el cliente <cliente>.

4. Ejecutar Angular

Abrir otra terminal:

cd Fronted
npm install
npm start

Abrir:

http://localhost:4200

La configuración actual del frontend apunta a:

https://localhost:7159/api

Si el navegador no confía en el certificado de desarrollo, abrir primero Swagger y aceptar el certificado local.

Rutas actuales

/Inicio
/Inicio/RegistroPedidos
/Inicio/PedidosCocina
/Inicio/CrearOden
/Inicio/PedidosDelivery

CrearOden está escrito así en app.routes.ts. Se mantiene aquí para que la documentación coincida con el código actual.

Endpoints principales

Método

Endpoint

Descripción

POST

/api/Pedidos

Publica un pedido en Kafka

GET

/api/Pedidos

Obtiene todos los pedidos paginados

GET

/api/Pedidos/{pedidoId}

Obtiene el detalle de un pedido

GET

/api/Pedidos/Cocina/Pendientes

Obtiene pedidos pendientes de cocina

PATCH

/api/Pedidos/{pedidoId}/finalizar-cocina

Cambia el estado de cocina a Listo

GET

/api/Pedidos/delivery/disponibles

Obtiene pedidos disponibles para delivery

PATCH

/api/Pedidos/{pedidoId}/finalizar-delivery

Marca el pedido como entregado

Los endpoints paginados reciben:

pageNumber
pageSize

El repositorio limita pageSize a un máximo de 100 registros por solicitud.

Ejemplo para crear un pedido

{
  "cliente": "Carlos Martínez",
  "lugar_envio": "Colonia Kennedy",
  "subTotal": 170,
  "isv": 26,
  "total": 196,
  "detalles": [
    {
      "productoNombre": "Hamburguesa clásica",
      "cantidad": 2,
      "nota": "Sin cebolla"
    }
  ]
}

Respuesta esperada:

202 Accepted

{
  "mensaje": "Pedido enviado a Kafka correctamente.",
  "pedidoId": "ObjectId generado"
}

Nota sobre los valores monetarios

En el modelo almacenado los valores son decimal, pero el DTO de creación utiliza actualmente int. Por eso, para probar directamente la API conviene enviar números enteros.

Esta diferencia no impide ejecutar el proyecto, pero es algo que debería unificarse si se continúa desarrollando.

Modelo guardado en MongoDB

{
  "_id": "ObjectId",
  "cliente": "Carlos Martínez",
  "estado": "Pendiente",
  "estadoDelivery": "Pendiente",
  "lugar_envio": "Colonia Kennedy",
  "subTotal": 170,
  "isv": 26,
  "total": 196,
  "creado": "Fecha UTC",
  "finalizado": null,
  "detalles": [
    {
      "productoNombre": "Hamburguesa clásica",
      "cantidad": 2,
      "nota": "Sin cebolla"
    }
  ]
}

El identificador se genera en la API como un ObjectId válido antes de publicar el mensaje.

Pruebas de carga

El proyecto incluye dos scripts dentro de archivopy.

Instalar la dependencia

python -m pip install aiohttp

Carga masiva

cd archivopy
python carga_masiva.py

El archivo permite configurar:

TOTAL_PEDIDOS = 50000
CONCURRENCIA = 25
TIMEOUT_SEGUNDOS = 30

La prueba genera pedidos con productos, cantidades, clientes, direcciones y notas aleatorias. Luego muestra:

Pedidos enviados.

Solicitudes exitosas.

Solicitudes fallidas.

Tiempo mínimo, máximo y promedio.

Solicitudes por segundo.

Estados HTTP.

Primeros errores encontrados.

Para comenzar es mejor usar una cantidad pequeña:

TOTAL_PEDIDOS = 100
CONCURRENCIA = 10

Luego se puede aumentar gradualmente. Enviar 50,000 pedidos de una sola vez sin observar Kafka y el Consumer no aporta demasiado; lo importante es verificar también cuántos mensajes terminan persistidos.

Simulación del restaurante

python flujo_restaurante.py

Este script toma pedidos pendientes, los pasa por cocina y posteriormente los finaliza en delivery. Sirve para observar cómo cambian las pantallas mientras el sistema está trabajando.

Para detenerlo:

Ctrl + C

Comandos útiles

Ver logs de Kafka

docker logs -f delivery-kafka

Ver logs de MongoDB

docker logs -f delivery-mongo

Ver el estado de los contenedores

docker compose ps

Detener la infraestructura

docker compose down

Detener y eliminar también los datos

docker compose down -v

El comando anterior elimina los volúmenes de Kafka y MongoDB. Se pierde toda la información local.

Borrar únicamente los pedidos

docker exec -it delivery-mongo mongosh -u admin -p Mongo123456 --authenticationDatabase admin --eval "db.getSiblingDB('delivery').pedidos.deleteMany({})"

Contar los pedidos

docker exec -it delivery-mongo mongosh -u admin -p Mongo123456 --authenticationDatabase admin --eval "print(db.getSiblingDB('delivery').pedidos.countDocuments({}))"

Problemas comunes

La API responde 202, pero el pedido no aparece

Revisar que Delivery.Consumer esté ejecutándose. La API solo publica el mensaje; el Consumer es quien lo guarda en MongoDB.

Kafka intenta conectarse a localhost:9092

Para aplicaciones ejecutadas desde Windows debe utilizarse:

localhost:9002

El puerto 9092 es el listener interno utilizado entre contenedores.

MongoDB rechaza la autenticación

Comprobar que la cadena contenga:

authSource=delivery
authMechanism=SCRAM-SHA-256

También revisar que el usuario delivery_app exista en la base delivery.

Error de ObjectId

Los identificadores deben tener el formato válido de MongoDB. El API genera el ID antes de publicar el pedido y el Consumer vuelve a validarlo antes de guardarlo.

Angular no logra conectarse a la API

Revisar:

Que la API esté ejecutándose en https://localhost:7159.

Que el certificado local haya sido aceptado.

Que environment.development.ts apunte a la URL correcta.

Que Angular esté en http://localhost:4200, porque esa es la dirección configurada en CORS.

El script de Python muestra error de conexión

Confirmar que la API esté ejecutándose y que API_URL sea:

API_URL = "https://localhost:7159/api/Pedidos"

Decisiones y observaciones del desarrollo

Este proyecto fue creciendo por partes. Primero se comprobó que la API pudiera publicar en Kafka, después se agregó el Consumer, luego MongoDB y finalmente las pantallas de cocina y delivery.

Algunas cosas que parecían pequeñas terminaron causando bastante tiempo de revisión, por ejemplo:

Diferenciar el puerto interno y externo de Kafka.

Usar un ObjectId válido en lugar de un GUID.

Recordar que un 202 Accepted no significa que MongoDB ya guardó el pedido.

Mantener iguales los nombres de las propiedades entre C#, JSON y TypeScript.

Actualizar las pantallas de cocina y delivery sin recargar el navegador.

No confundir localhost:27017 del contenedor con localhost:27018 desde Windows.

Se dejaron estas notas porque normalmente son justo las cosas que uno olvida cuando vuelve a abrir el proyecto unos días después.

Limitaciones actuales

Las credenciales están visibles porque el proyecto está preparado para ambiente local y académico.

No existe autenticación de usuarios ni autorización por roles.

No hay una estrategia de reintentos con cola de mensajes muertos.

El frontend consulta cocina y delivery periódicamente; todavía no utiliza SignalR o WebSockets.

Los productos del formulario de creación están definidos directamente en Angular.

El DTO de creación usa enteros para dinero, mientras el modelo de MongoDB usa decimales.

No hay pruebas automatizadas que cubran el flujo completo API → Kafka → Consumer → MongoDB.

Kafka utiliza un solo broker y factor de replicación 1.

No existe todavía una configuración separada y segura para producción.

Estas limitaciones son aceptables para la demostración, pero serían los primeros puntos por mejorar en una siguiente versión.

Posibles mejoras

Agregar catálogo de productos en MongoDB.

Implementar usuarios y roles para caja, cocina y delivery.

Cambiar los montos del DTO a decimal.

Incorporar SignalR para actualizaciones en tiempo real.

Agregar reintentos controlados y una Dead Letter Queue.

Incluir métricas de Kafka, API y Consumer.

Agregar pruebas unitarias y de integración.

Mover contraseñas a variables de entorno o secretos.

Crear imágenes Docker para la API, el Consumer y Angular.

Añadir una interfaz de monitoreo para Kafka.

Seguridad

Las contraseñas incluidas son únicamente para ejecución local. No deben reutilizarse en un servidor real.

Antes de publicar el sistema se deberían cambiar:

Mongo123456
DeliveryMongo123

También se recomienda mover la configuración a variables de entorno y no guardar credenciales reales dentro de Git.

Autor

Proyecto desarrollado por Vini-19 como práctica académica de Big Data, integración de sistemas y procesamiento de pedidos mediante Kafka y MongoDB.

Estado del proyecto

Funcional para demostración local:

Creación de pedidos.

Publicación en Kafka.

Consumo y persistencia en MongoDB.

Consulta paginada.

Flujo de cocina.

Flujo de delivery.

Visualización del detalle.

Pruebas de carga masiva.

Simulación automática del restaurante.

Todavía hay cosas por pulir, pero el flujo principal ya puede ejecutarse de principio a fin.
