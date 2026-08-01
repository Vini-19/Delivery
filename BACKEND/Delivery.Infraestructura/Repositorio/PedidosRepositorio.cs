using Delivery.Infraestructura.Configuraciones;
using Delivery.Shared.Interfaz;
using Delivery.Shared.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Delivery.Infraestructura.Repositorio
{
    public class PedidosRepositorio : IPedidosRepositorio
    {
        private readonly IMongoCollection<Pedidos>
            _pedidosCollection;

        public PedidosRepositorio(
            IOptions<MongoDb> options)
        {
            var config = options.Value;

            var cliente = new MongoClient(
                config.ConnectionString
            );

            var database = cliente.GetDatabase(
                config.DatabaseName
            );

            _pedidosCollection =
                database.GetCollection<Pedidos>(
                    config.PedidosCollection
                );
        }

        private static FilterDefinition<Pedidos>
            FiltroPedidosDelivery()
        {
            return Builders<Pedidos>.Filter.And(
                Builders<Pedidos>.Filter.Eq(
                    x => x.Estado,
                    "Listo"
                ),

                Builders<Pedidos>.Filter.Eq(
                    x => x.EstadoDelivery,
                    "Pendiente"
                )
            );
        }


        private static FilterDefinition<Pedidos>
            FiltroPedidosPedidoCocina()
        {
            return Builders<Pedidos>.Filter.And(
                Builders<Pedidos>.Filter.Eq(
                    x => x.Estado,
                    "Pendiente"
                ),

                Builders<Pedidos>.Filter.Eq(
                    x => x.EstadoDelivery,
                    "Pendiente"
                )
            );
        }

        public async Task GuardarPedidoAsync(
            Pedidos pedido)
        {
            if (pedido == null)
            {
                throw new ArgumentNullException(
                    nameof(pedido)
                );
            }

            if (!ObjectId.TryParse(pedido.Id, out _))
            {
                throw new InvalidOperationException(
                    $"El identificador '{pedido.Id}' no es un ObjectId válido."
                );
            }

            var filtro = Builders<Pedidos>.Filter.Eq(
                x => x.Id,
                pedido.Id
            );

            await _pedidosCollection.ReplaceOneAsync(
                filtro,
                pedido,
                new ReplaceOptions
                {
                    IsUpsert = true
                }
            );
        }

        public async Task<bool> CambiarEstadoCocinaAsync(
            string pedidoId,
            string nuevoEstado)
        {
            if (!ObjectId.TryParse(pedidoId, out _))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(nuevoEstado))
            {
                return false;
            }

            nuevoEstado = nuevoEstado.Trim();

            var filtro = Builders<Pedidos>.Filter.Eq(
                x => x.Id,
                pedidoId
            );

            var actualizacion =
                Builders<Pedidos>.Update.Set(
                    x => x.Estado,
                    nuevoEstado
                );

            var resultado =
                await _pedidosCollection.UpdateOneAsync(
                    filtro,
                    actualizacion
                );

            return resultado.MatchedCount > 0;
        }

        public async Task<bool>
            CambiarEstadoDeliveryAsync(
                string pedidoId,
                string nuevoEstado)
        {
            if (!ObjectId.TryParse(pedidoId, out _))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(nuevoEstado))
            {
                return false;
            }

            nuevoEstado = nuevoEstado.Trim();

            if (nuevoEstado.Equals(
                "Finalizado",
                StringComparison.OrdinalIgnoreCase))
            {
                nuevoEstado = "Finalizado";
            }

            var filtro = Builders<Pedidos>.Filter.And(
                Builders<Pedidos>.Filter.Eq(
                    x => x.Id,
                    pedidoId
                ),

                Builders<Pedidos>.Filter.Eq(
                    x => x.Estado,
                    "Listo"
                ),

                Builders<Pedidos>.Filter.Eq(
                    x => x.EstadoDelivery,
                    "Pendiente"
                )
            );

            var actualizacion =
                Builders<Pedidos>.Update
                    .Set(
                        x => x.EstadoDelivery,
                        nuevoEstado
                    );

            if (nuevoEstado == "Finalizado")
            {
                actualizacion = actualizacion.Set(
                    x => x.Finalizado,
                    DateTime.UtcNow
                );
            }

            var resultado =
                await _pedidosCollection.UpdateOneAsync(
                    filtro,
                    actualizacion
                );

            return resultado.MatchedCount > 0;
        }

        public async Task<ICollection<Pedidos>>
            GetPedidosAsync(
                int pageNumber,
                int pageSize)
        {
            pageNumber =
                pageNumber <= 0 ? 1 : pageNumber;

            pageSize =
                pageSize <= 0
                    ? 10
                    : Math.Min(pageSize, 100);

            var skip =
                (pageNumber - 1) * pageSize;

            return await _pedidosCollection
                .Find(Builders<Pedidos>.Filter.Empty)
                .SortByDescending(x => x.Creado)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<ICollection<Pedidos>>
            GetPedidosDeliveryAsync(
                int pageNumber,
                int pageSize)
        {
            pageNumber =
                pageNumber <= 0 ? 1 : pageNumber;

            pageSize =
                pageSize <= 0
                    ? 10
                    : Math.Min(pageSize, 100);

            var skip =
                (pageNumber - 1) * pageSize;

            return await _pedidosCollection
                .Find(FiltroPedidosDelivery())
                .SortBy(x => x.Creado)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<Pedidos?> GetPedidosPorIdAsync(
            string pedidoId)
        {
            if (!ObjectId.TryParse(pedidoId, out _))
            {
                return null;
            }

            var filtro = Builders<Pedidos>.Filter.Eq(
                x => x.Id,
                pedidoId
            );

            return await _pedidosCollection
                .Find(filtro)
                .FirstOrDefaultAsync();
        }

        public async Task<long> GetTotalPedidos()
        {
            return await _pedidosCollection
                .CountDocumentsAsync(
                    Builders<Pedidos>.Filter.Empty
                );
        }

        public async Task<long>
            GetTotalPedidosDelivery()
        {
            return await _pedidosCollection
                .CountDocumentsAsync(
                    FiltroPedidosDelivery()
                );
        }

        public async Task<ICollection<Pedidos>> GetPedidosCocinaAsync(int pageNumber, int pageSize)
        {
            pageNumber =
               pageNumber <= 0 ? 1 : pageNumber;

            pageSize =
                pageSize <= 0
                    ? 10
                    : Math.Min(pageSize, 100);

            var skip =
                (pageNumber - 1) * pageSize;

            return await _pedidosCollection
                .Find(FiltroPedidosPedidoCocina())
                .SortBy(x => x.Creado)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<long> GetTotalPedidosCocina()
        {
            return await _pedidosCollection.CountDocumentsAsync(FiltroPedidosPedidoCocina());
        }
    }
}