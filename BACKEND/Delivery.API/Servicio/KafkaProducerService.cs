using Confluent.Kafka;
using Delivery.API.Helper;
using Delivery.Shared.Models;
using System.Text.Json;

namespace Delivery.API.Servicio
{
    public class KafkaProducerService : IDisposable
    {
        private readonly KafkaProducerSettings _settings;

        private readonly IProducer<string, string> _producer;

        public KafkaProducerService(
            KafkaProducerSettings settings)
        {
            _settings = settings;

            var config = new ProducerConfig
            {
                BootstrapServers =
                    _settings.BootstrapServers,

                Acks = Acks.All,

                EnableIdempotence = true,

                MessageTimeoutMs = 10000
            };

            _producer =
                new ProducerBuilder<string, string>(
                    config
                )
                .Build();
        }

        public async Task PublicarPedidoAsync(
            Pedidos pedido)
        {
            var json = JsonSerializer.Serialize(
                pedido,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy =
                        JsonNamingPolicy.CamelCase
                }
            );

            await _producer.ProduceAsync(
                _settings.Topic,
                new Message<string, string>
                {
                    Key = pedido.Id,
                    Value = json
                }
            );
        }

        public void Dispose()
        {
            _producer.Flush(
                TimeSpan.FromSeconds(5)
            );

            _producer.Dispose();
        }
    }
}