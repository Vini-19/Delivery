import asyncio
import random
import time
from collections import Counter

import aiohttp


API_URL = "https://localhost:7159/api/Pedidos"

TOTAL_PEDIDOS = 50000
CONCURRENCIA = 25
TIMEOUT_SEGUNDOS = 30


PRODUCTOS = [
    {
        "nombre": "Hamburguesa clásica",
        "precio": 85
    },
    {
        "nombre": "Hamburguesa doble",
        "precio": 125
    },
    {
        "nombre": "Pizza personal",
        "precio": 110
    },
    {
        "nombre": "Pollo con papas",
        "precio": 120
    },
    {
        "nombre": "Tacos de pollo",
        "precio": 75
    },
    {
        "nombre": "Baleada especial",
        "precio": 60
    },
    {
        "nombre": "Alitas BBQ",
        "precio": 135
    },
    {
        "nombre": "Refresco",
        "precio": 25
    },
    {
        "nombre": "Jugo natural",
        "precio": 35
    },
    {
        "nombre": "Papas fritas",
        "precio": 45
    }
]


CLIENTES = [
    "Carlos Martínez",
    "Ana López",
    "José Hernández",
    "María Rodríguez",
    "Luis García",
    "Sofía Flores",
    "Pedro Castillo",
    "Daniela Reyes",
    "Miguel Torres",
    "Gabriela Cruz",
    "Fernando Mejía",
    "Paola Ramírez",
    "Roberto Sánchez",
    "Valeria Díaz",
    "Jorge Molina"
]


DIRECCIONES = [
    "Colonia Kennedy",
    "Residencial Honduras",
    "Colonia Miraflores",
    "Colonia Palmira",
    "Boulevard Morazán",
    "Colonia América",
    "Comayagüela centro",
    "Colonia Alameda",
    "Residencial Las Uvas",
    "Colonia El Prado",
    "Colonia Hato de Enmedio",
    "Colonia La Granja",
    "Residencial Plaza",
    "Colonia Tiloarque",
    "Barrio La Leona"
]


NOTAS = [
    None,
    None,
    None,
    "Sin cebolla",
    "Sin tomate",
    "Extra queso",
    "Salsa aparte",
    "Sin mayonesa",
    "Bien cocido",
    "Agregar servilletas",
    "Sin chile",
    "Con bastante salsa"
]


def crear_pedido(numero: int) -> dict:
    cantidad_productos = random.randint(1, 5)

    productos_seleccionados = random.sample(
        PRODUCTOS,
        cantidad_productos
    )

    detalles = []
    subtotal = 0

    for producto in productos_seleccionados:
        cantidad = random.randint(1, 4)

        subtotal_producto = (
            producto["precio"] *
            cantidad
        )

        subtotal += subtotal_producto

        detalles.append({
            "productoNombre": producto["nombre"],
            "cantidad": cantidad,
            "nota": random.choice(NOTAS)
        })

    isv = round(subtotal * 0.15)
    total = subtotal + isv

    return {
        "cliente": (
            f"{random.choice(CLIENTES)} "
            f"#{numero}"
        ),
        "lugar_envio": random.choice(DIRECCIONES),
        "subTotal": int(subtotal),
        "isv": int(isv),
        "total": int(total),
        "detalles": detalles
    }


async def enviar_pedido(
    session: aiohttp.ClientSession,
    semaforo: asyncio.Semaphore,
    numero: int
) -> dict:

    pedido = crear_pedido(numero)

    async with semaforo:
        inicio = time.perf_counter()

        try:
            async with session.post(
                API_URL,
                json=pedido
            ) as respuesta:

                duracion = (
                    time.perf_counter() -
                    inicio
                )

                contenido = await respuesta.text()

                return {
                    "numero": numero,
                    "exitoso": (
                        200 <= respuesta.status < 300
                    ),
                    "estado": respuesta.status,
                    "duracion": duracion,
                    "respuesta": contenido
                }

        except asyncio.TimeoutError:
            return {
                "numero": numero,
                "exitoso": False,
                "estado": 0,
                "duracion": (
                    time.perf_counter() -
                    inicio
                ),
                "respuesta": (
                    "La petición superó el tiempo límite."
                )
            }

        except aiohttp.ClientConnectorError as error:
            return {
                "numero": numero,
                "exitoso": False,
                "estado": 0,
                "duracion": (
                    time.perf_counter() -
                    inicio
                ),
                "respuesta": (
                    f"No se pudo conectar con la API: "
                    f"{error}"
                )
            }

        except Exception as error:
            return {
                "numero": numero,
                "exitoso": False,
                "estado": 0,
                "duracion": (
                    time.perf_counter() -
                    inicio
                ),
                "respuesta": str(error)
            }


async def ejecutar_prueba() -> None:
    print()
    print("PRUEBA DE CARGA MASIVA")
    print("-----------------------")
    print(f"API: {API_URL}")
    print(f"Pedidos: {TOTAL_PEDIDOS}")
    print(f"Concurrencia: {CONCURRENCIA}")
    print()

    semaforo = asyncio.Semaphore(
        CONCURRENCIA
    )

    timeout = aiohttp.ClientTimeout(
        total=TIMEOUT_SEGUNDOS
    )

    conector = aiohttp.TCPConnector(
        ssl=False,
        limit=CONCURRENCIA
    )

    resultados = []

    inicio_total = time.perf_counter()

    async with aiohttp.ClientSession(
        timeout=timeout,
        connector=conector
    ) as session:

        tareas = [
            enviar_pedido(
                session,
                semaforo,
                numero
            )
            for numero in range(
                1,
                TOTAL_PEDIDOS + 1
            )
        ]

        for tarea in asyncio.as_completed(
            tareas
        ):
            resultado = await tarea

            resultados.append(
                resultado
            )

            procesados = len(resultados)

            if (
                procesados % 50 == 0 or
                procesados == TOTAL_PEDIDOS
            ):
                exitosos_actuales = sum(
                    1
                    for item in resultados
                    if item["exitoso"]
                )

                fallidos_actuales = (
                    procesados -
                    exitosos_actuales
                )

                print(
                    f"Procesados: "
                    f"{procesados}/"
                    f"{TOTAL_PEDIDOS} | "
                    f"Exitosos: "
                    f"{exitosos_actuales} | "
                    f"Fallidos: "
                    f"{fallidos_actuales}"
                )

    duracion_total = (
        time.perf_counter() -
        inicio_total
    )

    exitosos = [
        resultado
        for resultado in resultados
        if resultado["exitoso"]
    ]

    fallidos = [
        resultado
        for resultado in resultados
        if not resultado["exitoso"]
    ]

    tiempos = [
        resultado["duracion"]
        for resultado in resultados
    ]

    tiempo_promedio = (
        sum(tiempos) / len(tiempos)
        if tiempos
        else 0
    )

    tiempo_minimo = (
        min(tiempos)
        if tiempos
        else 0
    )

    tiempo_maximo = (
        max(tiempos)
        if tiempos
        else 0
    )

    solicitudes_por_segundo = (
        len(resultados) /
        duracion_total
        if duracion_total > 0
        else 0
    )

    estados_http = Counter(
        resultado["estado"]
        for resultado in resultados
    )

    print()
    print("RESULTADO DE LA PRUEBA")
    print("-----------------------")
    print(
        f"Pedidos enviados: "
        f"{len(resultados)}"
    )
    print(
        f"Exitosos: "
        f"{len(exitosos)}"
    )
    print(
        f"Fallidos: "
        f"{len(fallidos)}"
    )
    print(
        f"Tiempo total: "
        f"{duracion_total:.2f} segundos"
    )
    print(
        f"Tiempo promedio: "
        f"{tiempo_promedio:.3f} segundos"
    )
    print(
        f"Tiempo mínimo: "
        f"{tiempo_minimo:.3f} segundos"
    )
    print(
        f"Tiempo máximo: "
        f"{tiempo_maximo:.3f} segundos"
    )
    print(
        f"Solicitudes por segundo: "
        f"{solicitudes_por_segundo:.2f}"
    )

    print()
    print("ESTADOS HTTP")
    print("------------")

    for estado, cantidad in sorted(
        estados_http.items()
    ):
        nombre_estado = (
            "Error de conexión"
            if estado == 0
            else str(estado)
        )

        print(
            f"{nombre_estado}: "
            f"{cantidad}"
        )

    if fallidos:
        print()
        print("PRIMEROS ERRORES")
        print("----------------")

        for error in fallidos[:10]:
            print()
            print(
                f"Pedido: "
                f"{error['numero']}"
            )
            print(
                f"Estado: "
                f"{error['estado']}"
            )
            print(
                f"Respuesta: "
                f"{error['respuesta'][:500]}"
            )


if __name__ == "__main__":
    try:
        asyncio.run(
            ejecutar_prueba()
        )
    except KeyboardInterrupt:
        print()
        print(
            "Prueba cancelada por el usuario."
        )