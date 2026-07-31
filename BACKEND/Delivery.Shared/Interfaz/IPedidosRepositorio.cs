using Delivery.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Delivery.Shared.Interfaz
{
    public interface IPedidosRepositorio
    {
        void GuardarPedido(Pedidos pedido);

        Task<long> GetTotalPedidos();
        Task<long> GetTotalPedidosDelivery();
        Task<Pedidos?> GetPedidosPorIdAsync(string pedidoId);

        Task<ICollection<Pedidos>> GetPedidosAsync(int PageNumber, int PageSize);
        Task<ICollection<Pedidos>> GetPedidosDeliveryAsync(int PageNumber, int PageSize);

        Task<bool> CambiarEstadoCocinaAsync(string pedidoId, string nuevoEstado);
        Task<bool> CambiarEstadoDeliveryAsync(string pedidoId, string nuevoEstado);

    }
}
