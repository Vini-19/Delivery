using System;
using System.Collections.Generic;
using System.Text;

namespace Delivery.Infraestructura.Configuraciones
{
    public class MongoDb
    {
        public string ConnectionString { get; set; } = string.Empty;

        public string DatabaseName { get; set; } = string.Empty;

        public string PedidosCollection { get; set; } = string.Empty;
    }
}
