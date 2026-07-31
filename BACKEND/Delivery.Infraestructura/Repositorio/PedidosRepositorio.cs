using Delivery.Infraestructura.Configuraciones;
using Delivery.Shared.Interfaz;
using Delivery.Shared.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Delivery.Infraestructura.Repositorio
{
    public class PedidosRepositorio : IPedidosRepositorio
    {
        private readonly IMongoCollection<Pedidos> _pedidosCollection;

        public PedidosRepositorio(IOptions<MongoDb> options)
        {
            var config = options.Value;
            var cliente = new MongoClient(config.ConnectionString);
            var database = cliente.GetDatabase(config.DatabaseName);

            _pedidosCollection = database.GetCollection<Pedidos>(config.ResultadosCollection);
        }

        public static FilterDefinition<Pedidos> FiltroPedidosDelivery()
        {
            return Builders<Pedidos>
                .Filter
                .And(
                Builders<Pedidos>.Filter
                .Eq(
                    x => x.Estado, "Listo"),

                Builders<Pedidos>.Filter
                .Eq(
                    x => x.EstadoDelivery, "Pendiente")
                );
        }



        public async Task<bool> CambiarEstadoCocinaAsync(string pedidoId, string nuevoEstado)
        {
            if(string.IsNullOrWhiteSpace(pedidoId) || string.IsNullOrWhiteSpace(nuevoEstado))
            {
                return false;
            }
            if(!ObjectId.TryParse(pedidoId, out _))
            {
                return false;
            }
            nuevoEstado = nuevoEstado.Trim();

            var filtro = Builders<Pedidos>
                .Filter
                .Eq(x => x.Id, pedidoId);

            var actualizacion = Builders<Pedidos>
                .Update
                .Set(x => x.Estado, nuevoEstado);

            var resultado = await _pedidosCollection.UpdateOneAsync(filtro, actualizacion);

            return resultado.ModifiedCount > 0;
        }

        public async Task<bool> CambiarEstadoDeliveryAsync(string pedidoId, string nuevoEstado)
        {
            if (string.IsNullOrWhiteSpace(pedidoId) || string.IsNullOrWhiteSpace(nuevoEstado))
            {
                return false;
            }
            if (!ObjectId.TryParse(pedidoId, out _))
            {
                return false;
            }
            nuevoEstado = nuevoEstado.Trim();

            if(nuevoEstado.Equals("Finalizado", StringComparison.OrdinalIgnoreCase))
            {
                nuevoEstado = "Finalizado";
            }
            var filtro =
                Builders<Pedidos>
                .Filter
                .And(
                Builders<Pedidos>.Filter
                .Eq(
                    x => x.Id, pedidoId),

                Builders<Pedidos>.Filter
                .Eq(
                    x => x.Estado, "Listo"),

                Builders<Pedidos>.Filter
                .Eq(
                    x => x.EstadoDelivery, "Pendiente")
                );

            var actualizacion = Builders<Pedidos>
                .Update
                .Set(x => x.EstadoDelivery, nuevoEstado);
            if(nuevoEstado == "Finalizado")
            {
                actualizacion = actualizacion.Set(x => x.Finalizado, DateTime.UtcNow);
            }

            var resultado = await _pedidosCollection.UpdateOneAsync(filtro, actualizacion);

            return resultado.ModifiedCount > 0;
        }

        public async Task<ICollection<Pedidos>> GetPedidosAsync(int PageNumber, int PageSize)
        {
            PageNumber = PageNumber <= 0 ? 1 : PageNumber;
            PageSize = PageSize <= 0 ? 100 : PageSize;

            var skip = (PageNumber - 1) * PageSize;
            var filtro = Builders<Pedidos>.Filter.Empty;

            var pedidos = await _pedidosCollection.Find(filtro)
                .SortByDescending(x => x.Creado)
                .Skip(skip)
                .Limit(PageSize)
                .ToListAsync();

            return pedidos;
        }

        public async Task<ICollection<Pedidos>> GetPedidosDeliveryAsync(int PageNumber, int PageSize)
        {
            PageNumber = PageNumber <= 0 ? 1 : PageNumber;
            PageSize = PageSize <= 0 ? 100 : PageSize;

            var skip = (PageNumber - 1) * PageSize;
            var filtro = FiltroPedidosDelivery();

            var pedidos = await _pedidosCollection.Find(filtro)
                .SortByDescending(x => x.Creado)
                .Skip(skip)
                .Limit(PageSize)
                .ToListAsync();

            return pedidos;

        }

        public async Task<Pedidos?> GetPedidosPorIdAsync(string pedidoId)
        {
            if (string.IsNullOrWhiteSpace(pedidoId))
            {
                return null;
            }
            if(!ObjectId.TryParse(pedidoId , out _))
            {
                return null;
            }

            var filtro = Builders<Pedidos>
                .Filter
                .Eq(x => x.Id, pedidoId);

            return await _pedidosCollection.Find(filtro).FirstOrDefaultAsync();
        }

        public async Task<long> GetTotalPedidos()
        {
            var total = Builders<Pedidos>.Filter.Empty;
            return await _pedidosCollection.CountDocumentsAsync(total);
        }

        public async Task<long> GetTotalPedidosDelivery()
        {
            var total = FiltroPedidosDelivery();
            return await _pedidosCollection.CountDocumentsAsync(total);
        }

        public void GuardarPedido(Pedidos pedido)
        {
            if(pedido == null)
            {
                throw new ArgumentNullException(nameof(pedido));
            
            }
            if (string.IsNullOrWhiteSpace(pedido.Id))
            {
                pedido.Id = ObjectId
                    .GenerateNewId()
                    .ToString();
            }
            pedido.Estado = "Pendiente";
            pedido.EstadoDelivery = "Pendiente";
            pedido.Creado = DateTime.UtcNow;

            _pedidosCollection.InsertOne(pedido);

        }
    }
}
