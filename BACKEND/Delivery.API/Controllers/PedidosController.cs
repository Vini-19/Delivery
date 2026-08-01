using Confluent.Kafka;
using Delivery.API.DTO;
using Delivery.API.Servicio;
using Delivery.Shared.Interfaz;
using Delivery.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Delivery.API.Controllers
{
    [Route("api/Pedidos")]
    [ApiController]
    public class PedidosController : ControllerBase
    {
        private readonly KafkaProducerService
            _kafkaProducerService;

        private readonly IPedidosRepositorio
            _pedidosRepositorio;

        public PedidosController(
            KafkaProducerService kafkaProducerService,
            IPedidosRepositorio pedidosRepositorio)
        {
            _kafkaProducerService =
                kafkaProducerService;

            _pedidosRepositorio =
                pedidosRepositorio;
        }

        [HttpPost]
        public async Task<IActionResult> GuardarPedido(
            [FromBody] PedidoCrearDTO dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "El pedido no puede ser nulo."
                    });
                }

                if (string.IsNullOrWhiteSpace(
                    dto.Cliente))
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "El cliente es obligatorio."
                    });
                }

                if (string.IsNullOrWhiteSpace(
                    dto.Lugar_envio))
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "El lugar de envío es obligatorio."
                    });
                }

                if (dto.Detalles == null ||
                    dto.Detalles.Count == 0)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "Debe agregar al menos un producto."
                    });
                }

                foreach (var detalle in dto.Detalles)
                {
                    if (string.IsNullOrWhiteSpace(
                        detalle.ProductoNombre))
                    {
                        return BadRequest(new
                        {
                            mensaje =
                                "Todos los productos deben tener nombre."
                        });
                    }

                    if (detalle.Cantidad <= 0)
                    {
                        return BadRequest(new
                        {
                            mensaje =
                                "La cantidad debe ser mayor que cero."
                        });
                    }

                    detalle.ProductoNombre =
                        detalle.ProductoNombre.Trim();

                    detalle.Nota =
                        string.IsNullOrWhiteSpace(
                            detalle.Nota
                        )
                            ? null
                            : detalle.Nota.Trim();
                }

                if (dto.SubTotal < 0 ||
                    dto.ISV < 0 ||
                    dto.Total <= 0)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "Los valores monetarios no son válidos."
                    });
                }

                var pedido = new Pedidos
                {
                    Id = ObjectId
                        .GenerateNewId()
                        .ToString(),

                    Cliente = dto.Cliente.Trim(),

                    Estado = "Pendiente",

                    EstadoDelivery = "Pendiente",

                    Lugar_envio =
                        dto.Lugar_envio.Trim(),

                    SubTotal = dto.SubTotal,

                    ISV = dto.ISV,

                    Total = dto.Total,

                    Creado = DateTime.UtcNow,

                    Finalizado = null,

                    Detalles = dto.Detalles
                };

                await _kafkaProducerService
                    .PublicarPedidoAsync(pedido);

                return Accepted(new
                {
                    mensaje =
                        "Pedido enviado a Kafka correctamente.",

                    pedidoId = pedido.Id
                });
            }
            catch (
                ProduceException<string, string> ex)
            {
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    new
                    {
                        mensaje =
                            "No se pudo publicar el pedido en Kafka.",

                        error = ex.Error.Reason
                    }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Error interno del servidor.",

                        error = ex.Message
                    }
                );
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPedidos(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                pageNumber =
                    pageNumber <= 0 ? 1 : pageNumber;

                pageSize =
                    pageSize <= 0 ? 10 : pageSize;

                var totalPedidos =
                    await _pedidosRepositorio
                        .GetTotalPedidos();

                var pedidos =
                    await _pedidosRepositorio
                        .GetPedidosAsync(
                            pageNumber,
                            pageSize
                        );

                return Ok(new
                {
                    Pedidos = pedidos,

                    TotalPedidos = totalPedidos,

                    PaginaNumero = pageNumber,

                    TamañoPagina = pageSize,

                    TotalPaginas = (int)Math.Ceiling(
                        totalPedidos /
                        (double)pageSize
                    )
                });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "No se pudieron recuperar los pedidos.",

                        error = ex.Message
                    }
                );
            }
        }

        [HttpGet("delivery/disponibles")]
        public async Task<IActionResult>
            ObtenerPedidosDelivery(
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10)
        {
            try
            {
                pageNumber =
                    pageNumber <= 0 ? 1 : pageNumber;

                pageSize =
                    pageSize <= 0 ? 10 : pageSize;

                var totalPedidos =
                    await _pedidosRepositorio
                        .GetTotalPedidosDelivery();

                var pedidos =
                    await _pedidosRepositorio
                        .GetPedidosDeliveryAsync(
                            pageNumber,
                            pageSize
                        );

                return Ok(new
                {
                    Pedidos = pedidos,

                    TotalPedidos = totalPedidos,

                    PaginaNumero = pageNumber,

                    TamañoPagina = pageSize,

                    TotalPaginas = (int)Math.Ceiling(
                        totalPedidos /
                        (double)pageSize
                    )
                });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "No se pudieron recuperar los pedidos para delivery.",

                        error = ex.Message
                    }
                );
            }
        }

        [HttpGet("Cocina/Pendientes")]
        public async Task<IActionResult>
            ObtenerPedidosCocina(
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10)
        {
            try
            {
                pageNumber =
                    pageNumber <= 0 ? 1 : pageNumber;

                pageSize =
                    pageSize <= 0 ? 10 : pageSize;

                var totalPedidos =
                    await _pedidosRepositorio
                        .GetTotalPedidosCocina();

                var pedidos =
                    await _pedidosRepositorio
                        .GetPedidosCocinaAsync(
                            pageNumber,
                            pageSize
                        );

                return Ok(new
                {
                    Pedidos = pedidos,

                    TotalPedidos = totalPedidos,

                    PaginaNumero = pageNumber,

                    TamañoPagina = pageSize,

                    TotalPaginas = (int)Math.Ceiling(
                        totalPedidos /
                        (double)pageSize
                    )
                });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "No se pudieron recuperar los pedidos para delivery.",

                        error = ex.Message
                    }
                );
            }
        }


        [HttpPatch("{pedidoId}/finalizar-cocina")]
        public async Task<IActionResult> FinalizarCocina(
            string pedidoId)
        {
            if (!ObjectId.TryParse(pedidoId, out _))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El identificador no es válido."
                });
            }

            var actualizado =
                await _pedidosRepositorio
                    .CambiarEstadoCocinaAsync(
                        pedidoId,
                        "Listo"
                    );

            if (!actualizado)
            {
                return NotFound(new
                {
                    mensaje =
                        "No se encontró el pedido."
                });
            }

            return Ok(new
            {
                mensaje =
                    "Pedido listo para delivery.",

                pedidoId,
                estado = "Listo"
            });
        }

        [HttpPatch(
            "{pedidoId}/finalizar-delivery"
        )]
        public async Task<IActionResult> FinalizarDelivery(
            string pedidoId)
        {
            if (!ObjectId.TryParse(pedidoId, out _))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El identificador no es válido."
                });
            }

            var actualizado =
                await _pedidosRepositorio
                    .CambiarEstadoDeliveryAsync(
                        pedidoId,
                        "Finalizado"
                    );

            if (!actualizado)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El pedido debe estar listo en cocina y pendiente de delivery."
                });
            }

            return Ok(new
            {
                mensaje =
                    "Pedido entregado correctamente.",

                pedidoId,

                estadoDelivery =
                    "Finalizado"
            });
        }

        [HttpGet("{pedidoId}")]
        public async Task<IActionResult>
            ObtenerPedidoPorId(
                string pedidoId)
        {
            if (!ObjectId.TryParse(pedidoId, out _))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El identificador no es válido."
                });
            }

            var pedido =
                await _pedidosRepositorio
                    .GetPedidosPorIdAsync(pedidoId);

            if (pedido == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "No se encontró el pedido."
                });
            }

            return Ok(pedido);
        }
    }
}