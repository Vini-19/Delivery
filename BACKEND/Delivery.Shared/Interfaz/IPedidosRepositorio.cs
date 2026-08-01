using Delivery.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Delivery.Shared.Interfaz
{
    public interface IPedidosRepositorio
    {
        Task GuardarPedidoAsync(Pedidos pedido);

        Task<long> GetTotalPedidos();
        Task<long> GetTotalPedidosCocina();

        Task<ICollection<Pedidos>> GetPedidosAsync(
            int pageNumber,
            int pageSize
        );

        Task<ICollection<Pedidos>> GetPedidosCocinaAsync(
    int pageNumber,
    int pageSize
);

        Task<Pedidos?> GetPedidosPorIdAsync(
            string pedidoId
        );

        Task<bool> CambiarEstadoCocinaAsync(
            string pedidoId,
            string nuevoEstado
        );

        Task<long> GetTotalPedidosDelivery();

        Task<ICollection<Pedidos>> GetPedidosDeliveryAsync(
            int pageNumber,
            int pageSize
        );

        Task<bool> CambiarEstadoDeliveryAsync(
            string pedidoId,
            string nuevoEstado
        );
    }
}
