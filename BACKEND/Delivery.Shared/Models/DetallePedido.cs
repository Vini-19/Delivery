using System;
using System.Collections.Generic;
using System.Text;

namespace Delivery.Shared.Models
{
    public class DetallePedido
    {
        public string ProductoNombre { get; set; }
        public int Cantidad { get; set; }
        public string Nota { get; set; }
    }
}
