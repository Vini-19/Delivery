using Delivery.Shared.Models;

namespace Delivery.API.DTO
{
    public class PedidoCrearDTO
    {
        public string Cliente { get; set; } = string.Empty;

        public string Lugar_envio { get; set; } = string.Empty;

        public int SubTotal { get; set; }

        public int ISV { get; set; }

        public int Total { get; set; }

        public List<DetallePedido> Detalles { get; set; } = new();
    }
}
