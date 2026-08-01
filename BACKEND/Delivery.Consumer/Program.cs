

using Delivery.Consumer.Helpers;
using Delivery.Consumer.Servicios;
using Delivery.Infraestructura.Configuraciones;
using Delivery.Infraestructura.Repositorio;
using Delivery.Shared.Interfaz;

var builder = Host.CreateApplicationBuilder(args);

var kafkaSettings = new KafKaSettings
{
    BootstrapServers =
        builder.Configuration["Kafka:BootstrapServers"]
        ?? "localhost:9002",

    Topic =
        builder.Configuration["Kafka:Topic"]
        ?? "pedidos-delivery",

    GroupId =
        builder.Configuration["Kafka:GroupId"]
        ?? "delivery-pedidos-consumer"
};


builder.Services.Configure<MongoDb>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<IPedidosRepositorio, PedidosRepositorio>();
builder.Services.AddSingleton(kafkaSettings);
builder.Services.AddHostedService<KafkaConsumerService>();


var host = builder.Build();
host.Run();
