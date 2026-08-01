using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Delivery.Shared.Models
{
    public class Pedidos
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Cliente { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public string EstadoDelivery { get; set; } = "Pendiente";
        public string Lugar_envio { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ISV { get; set; }
        public decimal Total { get; set; }
        public DateTime Creado { get; set; } = DateTime.UtcNow;
        public DateTime? Finalizado { get; set; }
        public List<DetallePedido> Detalles { get; set; } = new();
    }
}
