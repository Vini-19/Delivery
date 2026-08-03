import asyncio
import json
from datetime import datetime

import aiohttp


API_URL = "https://localhost:7159/api/Pedidos"

COCINA_CADA_SEGUNDOS = 0.5
DELIVERY_CADA_SEGUNDOS = 2

PAGE_SIZE = 100
TIMEOUT_SEGUNDOS = 15


def hora_actual() -> str:
    return datetime.now().strftime("%H:%M:%S")


async def obtener_pedidos(
    session: aiohttp.ClientSession,
    ruta: str
) -> list:

    parametros = {
        "pageNumber": 1,
        "pageSize": PAGE_SIZE
    }

    async with session.get(
        f"{API_URL}/{ruta}",
        params=parametros
    ) as respuesta:

        texto = await respuesta.text()

        if respuesta.status != 200:
            raise RuntimeError(
                f"Estado {respuesta.status}: {texto}"
            )

        datos = json.loads(texto)

        return (
            datos.get("pedidos")
            or datos.get("Pedidos")
            or []
        )


async def actualizar_estado(
    session: aiohttp.ClientSession,
    pedido_id: str,
    accion: str
) -> tuple[bool, str]:

    async with session.patch(
        f"{API_URL}/{pedido_id}/{accion}",
        json={}
    ) as respuesta:

        texto = await respuesta.text()

        if 200 <= respuesta.status < 300:
            return True, texto

        return False, (
            f"Estado {respuesta.status}: {texto}"
        )


async def procesar_cocina(
    session: aiohttp.ClientSession
) -> str:

    pedidos = await obtener_pedidos(
        session,
        "Cocina/Pendientes"
    )

    if not pedidos:
        return "COCINA: sin pedidos pendientes"

    pedido = pedidos[0]

    pedido_id = (
        pedido.get("id")
        or pedido.get("Id")
        or ""
    )

    cliente = (
        pedido.get("cliente")
        or pedido.get("Cliente")
        or "Sin cliente"
    )

    exitoso, respuesta = await actualizar_estado(
        session,
        pedido_id,
        "finalizar-cocina"
    )

    if exitoso:
        return (
            f"COCINA: {cliente} | "
            f"{pedido_id} → Listo"
        )

    return (
        f"COCINA: error con {pedido_id} | "
        f"{respuesta}"
    )


async def procesar_delivery(
    session: aiohttp.ClientSession
) -> str:

    pedidos = await obtener_pedidos(
        session,
        "delivery/disponibles"
    )

    if not pedidos:
        return "DELIVERY: sin pedidos disponibles"

    pedido = pedidos[0]

    pedido_id = (
        pedido.get("id")
        or pedido.get("Id")
        or ""
    )

    cliente = (
        pedido.get("cliente")
        or pedido.get("Cliente")
        or "Sin cliente"
    )

    exitoso, respuesta = await actualizar_estado(
        session,
        pedido_id,
        "finalizar-delivery"
    )

    if exitoso:
        return (
            f"DELIVERY: {cliente} | "
            f"{pedido_id} → Entregado"
        )

    return (
        f"DELIVERY: error con {pedido_id} | "
        f"{respuesta}"
    )


async def ejecutar_simulacion() -> None:

    timeout = aiohttp.ClientTimeout(
        total=TIMEOUT_SEGUNDOS
    )

    conector = aiohttp.TCPConnector(
        ssl=False
    )

    segundo = 0
    total_cocina = 0
    total_delivery = 0

    print()
    print("SIMULACIÓN DE RESTAURANTE")
    print("-------------------------")
    print(
        f"Cocina: 1 pedido cada "
        f"{COCINA_CADA_SEGUNDOS} segundo"
    )
    print(
        f"Delivery: 1 pedido cada "
        f"{DELIVERY_CADA_SEGUNDOS} segundos"
    )
    print("Presiona Ctrl + C para detener.")
    print()

    async with aiohttp.ClientSession(
        timeout=timeout,
        connector=conector
    ) as session:

        while True:
            segundo += 1

            mensajes = []

            if (
                segundo %
                COCINA_CADA_SEGUNDOS
                == 0
            ):
                try:
                    resultado_cocina = (
                        await procesar_cocina(
                            session
                        )
                    )

                    mensajes.append(
                        resultado_cocina
                    )

                    if "→ Listo" in resultado_cocina:
                        total_cocina += 1

                except Exception as error:
                    mensajes.append(
                        f"COCINA: error | {error}"
                    )

            if (
                segundo %
                DELIVERY_CADA_SEGUNDOS
                == 0
            ):
                try:
                    resultado_delivery = (
                        await procesar_delivery(
                            session
                        )
                    )

                    mensajes.append(
                        resultado_delivery
                    )

                    if (
                        "→ Entregado"
                        in resultado_delivery
                    ):
                        total_delivery += 1

                except Exception as error:
                    mensajes.append(
                        f"DELIVERY: error | {error}"
                    )

            print(
                f"[{hora_actual()}] "
                f"Segundo {segundo} | "
                + " | ".join(mensajes)
            )

            await asyncio.sleep(1)


async def main() -> None:
    try:
        await ejecutar_simulacion()

    except KeyboardInterrupt:
        pass


if __name__ == "__main__":
    try:
        asyncio.run(main())

    except KeyboardInterrupt:
        print()
        print("Simulación detenida.")