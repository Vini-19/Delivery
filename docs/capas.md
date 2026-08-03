Bitácora de decisiones técnicas

Proyecto Delivery

Esta bitácora resume las decisiones técnicas tomadas durante el desarrollo del proyecto. No todas las decisiones fueron perfectas desde el inicio. Varias aparecieron después de errores de conexión, problemas con los modelos o diferencias entre el frontend y el backend.

1. Separar la API del Consumer

Decisión: crear Delivery.API y Delivery.Consumer como aplicaciones independientes.

Motivo: la API recibe las solicitudes HTTP, mientras que el Consumer procesa los mensajes de Kafka y guarda los pedidos.

Ventajas:

La API responde más rápido.

El guardado puede continuar aunque lleguen muchos pedidos.

Cada parte se puede revisar y ejecutar por separado.

Desventajas:

Hay que mantener dos aplicaciones encendidas.

Al principio fue fácil pensar que el pedido ya estaba guardado solo porque la API respondió correctamente.

La depuración requiere revisar logs de la API, Kafka y Consumer.

Trade-off: se aceptó más complejidad a cambio de desacoplar la recepción y el procesamiento.

2. Usar Kafka como intermediario

Decisión: publicar los pedidos en el topic pedidos-delivery.

Alternativa considerada: guardar directamente en MongoDB desde el endpoint POST.

Motivo: el proyecto es de Big Data y se necesitaba trabajar con productores, consumidores y procesamiento asíncrono.

Ventajas:

La API no depende directamente del tiempo de guardado.

Kafka conserva temporalmente los mensajes.

Se pueden agregar más consumidores en el futuro.

Desventajas:

La configuración de listeners y puertos fue más difícil de lo esperado.

Un 202 Accepted confirma que Kafka recibió el mensaje, no que MongoDB ya lo guardó.

Si el Consumer tiene errores, los pedidos pueden quedarse pendientes de procesamiento.

Trade-off: Kafka agrega tolerancia y escalabilidad, pero también agrega puntos de falla.

3. Configurar tres particiones en Kafka

Decisión: crear el topic con tres particiones.

Motivo: permitir mayor paralelismo durante las pruebas de carga.

Ventajas:

Varios mensajes pueden procesarse de forma paralela.

Es más adecuado para una prueba con muchos pedidos.

Desventajas:

No se garantiza un orden global entre todos los mensajes.

Con un solo Consumer no se aprovecha completamente el paralelismo.

Para este proyecto académico, tres particiones pueden ser más de lo necesario.

Trade-off: se prefirió dejar preparada una configuración más cercana a un escenario real.

4. Usar MongoDB para almacenar pedidos

Decisión: usar MongoDB en vez de SQL Server para los pedidos.

Motivo: cada pedido contiene una lista variable de productos y notas. El modelo documental permite guardar todo junto.

Ventajas:

El detalle del pedido se recupera en una sola consulta.

No se necesitan tablas separadas para pedido y detalle.

Es flexible para pruebas con diferentes cantidades de productos.

Desventajas:

No se usan relaciones como en una base SQL.

Fue necesario entender ObjectId.

Las búsquedas deben cuidar los nombres y tipos almacenados.

Trade-off: se eligió simplicidad documental sobre relaciones estrictas.

5. Usar ObjectId como identificador

Decisión: generar identificadores con ObjectId.GenerateNewId().

Problema encontrado: inicialmente se manejaron algunos identificadores como GUID o cadenas sin el formato esperado.

Ventajas:

Es el identificador natural de MongoDB.

Se genera sin consultar primero la base de datos.

Incluye información temporal en su estructura.

Desventajas:

Debe tener 24 caracteres hexadecimales.

Cada endpoint debe validar el identificador con ObjectId.TryParse.

Un identificador inválido provoca errores antes de consultar.

Trade-off: se mantuvo ObjectId para trabajar de forma coherente con MongoDB.

6. Usar Docker para Kafka y MongoDB

Decisión: ejecutar Kafka y MongoDB mediante Docker Compose.

Ventajas:

La infraestructura se puede levantar con un solo comando.

No es necesario instalar Kafka y MongoDB directamente en Windows.

Los volúmenes conservan la información.

Desventajas:

Se deben entender los puertos internos y externos.

Los nombres de host cambian según si la conexión ocurre desde Docker o desde Windows.

Los datos permanecen aunque se reconstruyan los contenedores, debido a los volúmenes.

Trade-off: Docker facilita repetir el ambiente, pero exige entender redes y almacenamiento persistente.

7. Diferenciar puertos internos y externos de Kafka

Decisión final:

Desde aplicaciones ejecutadas en Windows: localhost:9002

Desde contenedores en la red Docker: broker:9092

Problema encontrado: se intentó usar localhost:9092, lo cual produjo errores de conexión.

Aprendizaje: localhost dentro de un contenedor se refiere al mismo contenedor, no a la computadora ni a otro servicio.

Trade-off: se mantuvieron listeners separados para soportar conexiones internas y externas.

8. Separar estados de cocina y delivery

Decisión: usar dos campos:

Estado

EstadoDelivery

Flujo actual:

Estado: Pendiente → Listo
EstadoDelivery: Pendiente → Finalizado

Ventajas:

Cocina y delivery se pueden consultar por separado.

Un pedido puede estar listo en cocina sin estar entregado.

El registro conserva ambos estados.

Desventajas:

Hay que mantener coherencia entre los dos campos.

Un valor escrito con mayúsculas o minúsculas diferentes puede quedar fuera de los filtros.

El flujo actual es sencillo y no incluye todos los estados posibles de un restaurante real.

Trade-off: se eligió un flujo pequeño para mantener el proyecto entendible.

9. Usar paginación en los endpoints

Decisión: recibir pageNumber y pageSize en las consultas.

Ventajas:

El frontend no descarga todos los pedidos.

Permite probar una cantidad grande de registros.

Reduce el uso de memoria en la API y el navegador.

Desventajas:

Hay que calcular el total de documentos por separado.

Después de finalizar un pedido puede ser necesario ajustar la página actual.

La carga masiva hace más visible cualquier error de paginación.

Trade-off: se agregó algo más de lógica a cambio de soportar mejor grandes cantidades de información.

10. Actualizar Cocina y Delivery mediante polling

Decisión: consultar la API cada dos segundos.

Alternativa considerada: usar SignalR o WebSockets.

Motivo: el polling era más rápido de implementar y suficiente para la demostración.

Ventajas:

Código fácil de explicar.

No requiere configurar un Hub.

La pantalla se mantiene relativamente actualizada.

Desventajas:

Se hacen solicitudes aunque no haya cambios.

Con muchos usuarios aumenta el tráfico.

Hubo problemas de detección de cambios en Angular y estados de carga que no terminaban.

Trade-off: se aceptó menor eficiencia para reducir la complejidad del frontend.

11. Mantener los nombres JSON en camelCase

Decisión: usar en Angular nombres como:

cliente
lugar_envio
subTotal
isv
total
detalles

Problema encontrado: algunos modelos usaban LugarEnvio, SubTotal, ISV, Total o detalle.

Consecuencia: TypeScript mostraba errores o el HTML no encontraba propiedades como productoNombre.

Aprendizaje: los nombres de las interfaces, el JSON y el HTML deben coincidir exactamente.

Trade-off: se ajustaron los modelos del frontend para seguir la respuesta real de la API.

12. Usar decimal o entero para valores monetarios

Problema encontrado: la primera prueba de carga masiva envió valores decimales y la API respondió con errores de validación.

Decisión temporal: el script Python envía montos enteros para coincidir con el DTO actual.

Mejora recomendada: cambiar los montos del backend a decimal.

Ventajas de decimal:

Representa mejor los valores monetarios.

Permite centavos sin perder precisión.

Desventajas:

Requiere actualizar modelos, DTO y posiblemente documentos existentes.

Trade-off: para la prueba se priorizó compatibilidad inmediata; para producción sería mejor usar decimal.

13. Crear scripts Python para pruebas

Decisión: crear dos scripts independientes.

carga_masiva.py

Envía cientos o miles de pedidos a la API.

flujo_restaurante.py

Simula el avance gradual de pedidos por cocina y delivery.

Ventajas:

Permiten repetir pruebas.

Generan datos más rápido que el formulario.

Ayudan a medir errores y tiempos.

Desventajas:

Una respuesta exitosa de la API no garantiza inmediatamente que MongoDB ya tenga el pedido.

Una prueba con respuestas 400 puede parecer muy rápida, pero en realidad no está midiendo Kafka ni MongoDB.

La concurrencia debe aumentarse poco a poco.

Trade-off: se ganó capacidad de prueba, pero los resultados deben interpretarse correctamente.

14. Conservar los pedidos finalizados

Decisión: no borrar el documento cuando se entrega.

Motivo: el sistema necesita un registro histórico.

Ventajas:

Se puede consultar quién hizo el pedido y cuándo terminó.

Se conserva la información para reportes futuros.

Facilita demostrar el flujo completo.

Desventajas:

La colección crece continuamente.

En un sistema real se necesitarían índices, políticas de retención o archivado.

Trade-off: se prefirió mantener trazabilidad sobre ahorrar almacenamiento.

15. Borrar datos solo para pruebas

Durante las pruebas de carga fue necesario limpiar la colección con:

db.pedidos.deleteMany({})

Se decidió no usar:

db.dropDatabase()

porque elimina toda la base y puede borrar configuraciones o colecciones adicionales.

Aprendizaje: incluso en pruebas es mejor borrar únicamente lo necesario.

Problemas relevantes encontrados

Problema

Causa

Solución aplicada

Kafka no conectaba

Se usó un puerto incorrecto

Se configuró localhost:9002 para aplicaciones locales

MongoDB rechazaba credenciales

Base de autenticación incorrecta

Se utilizó authenticationDatabase=admin

ObjectId inválido

Se enviaron identificadores con otro formato

Se validó con ObjectId.TryParse

La tarjeta no mostraba detalles

El HTML usaba propiedades con mayúsculas

Se usó productoNombre, cantidad y nota

La pantalla no se actualizaba

Polling y detección de cambios

Se ajustó la actualización del componente

Spinner infinito

El estado de carga no regresaba a false

Se controló con finalize

Carga masiva con 1000 errores

El DTO no aceptó valores decimales

El script envió enteros

Pedido aceptado pero no visible

El Consumer no había guardado aún

Se revisó la ejecución del Consumer y Kafka

Conclusión

La arquitectura final cumple con el objetivo académico de trabajar con:

Productor.

Broker de mensajes.

Consumidor.

Persistencia documental.

Visualización web.

Pruebas de carga.

La principal dificultad no fue crear cada parte por separado, sino lograr que todas trabajaran al mismo tiempo. Los errores de puertos, nombres de propiedades, ObjectId y actualización del frontend ayudaron a entender mejor cómo se comunica un sistema distribuido.