using Confluent.Kafka;
using Delivery.Consumer.Helpers;
using Delivery.Shared.Interfaz;
using Delivery.Shared.Models;
using MongoDB.Bson;
using System.Text.Json;

namespace Delivery.Consumer.Servicios
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly KafKaSettings _settings;

        private readonly IPedidosRepositorio
            _pedidosRepositorio;

        private readonly ILogger<KafkaConsumerService>
            _logger;

        public KafkaConsumerService(
            KafKaSettings settings,
            IPedidosRepositorio pedidosRepositorio,
            ILogger<KafkaConsumerService> logger)
        {
            _settings = settings;
            _pedidosRepositorio = pedidosRepositorio;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers =
                    _settings.BootstrapServers,

                GroupId =
                    _settings.GroupId,

                AutoOffsetReset =
                    AutoOffsetReset.Earliest,

                EnableAutoCommit = false
            };

            using var consumer =
                new ConsumerBuilder<string, string>(
                    config
                )
                .Build();

            consumer.Subscribe(_settings.Topic);

            _logger.LogInformation(
                "Escuchando el topic {Topic} en {Servers} como grupo {GroupId}.",
                _settings.Topic,
                _settings.BootstrapServers,
                _settings.GroupId
            );

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    ConsumeResult<string, string>? resultado;

                    try
                    {
                        resultado = consumer.Consume(
                            TimeSpan.FromSeconds(1)
                        );
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(
                            ex,
                            "Error consumiendo el mensaje de Kafka."
                        );

                        continue;
                    }

                    if (resultado == null)
                    {
                        continue;
                    }

                    try
                    {
                        var opciones =
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            };

                        var pedido =
                            JsonSerializer.Deserialize<Pedidos>(
                                resultado.Message.Value,
                                opciones
                            );

                        if (pedido == null)
                        {
                            _logger.LogWarning(
                                "Se recibió un pedido vacío."
                            );

                            consumer.Commit(resultado);

                            continue;
                        }

                        if (!ObjectId.TryParse(
                            pedido.Id,
                            out _))
                        {
                            _logger.LogWarning(
                                "Pedido descartado porque el ID {PedidoId} no es un ObjectId válido.",
                                pedido.Id
                            );

                            consumer.Commit(resultado);

                            continue;
                        }

                        if (pedido.Detalles == null ||
                            pedido.Detalles.Count == 0)
                        {
                            _logger.LogWarning(
                                "Pedido {PedidoId} descartado porque no contiene productos.",
                                pedido.Id
                            );

                            consumer.Commit(resultado);

                            continue;
                        }

                        await _pedidosRepositorio
                            .GuardarPedidoAsync(pedido);

                        consumer.Commit(resultado);

                        _logger.LogInformation(
                            "Pedido {PedidoId} guardado para el cliente {Cliente}.",
                            pedido.Id,
                            pedido.Cliente
                        );
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "Mensaje descartado porque el JSON no es válido: {Mensaje}",
                            resultado.Message.Value
                        );

                        consumer.Commit(resultado);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "No se pudo guardar el pedido. Se volverá a intentar."
                        );

                        consumer.Seek(
                            resultado.TopicPartitionOffset
                        );

                        await Task.Delay(
                            TimeSpan.FromSeconds(2),
                            stoppingToken
                        );
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(
                    "Consumer detenido correctamente."
                );
            }
            finally
            {
                consumer.Close();
            }
        }
    }
}