using Delivery.API.Helper;
using Delivery.API.Servicio;
using Delivery.Infraestructura.Configuraciones;
using Delivery.Infraestructura.Repositorio;
using Delivery.Shared.Interfaz;

var builder = WebApplication.CreateBuilder(args);

// Controladores
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Delivery API",
        Version = "v1",
        Description =
            "API para publicar pedidos en Kafka y consultar pedidos en MongoDB."
    });
});

// CORS para Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularApp", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Configuración de Kafka
var kafkaProducerSettings = new KafkaProducerSettings
{
    BootstrapServers =
        builder.Configuration["Kafka:BootstrapServers"]
        ?? "localhost:9002",

    Topic =
        builder.Configuration["Kafka:Topic"]
        ?? "pedidos-delivery"
};

builder.Services.AddSingleton(kafkaProducerSettings);

// Configuración de MongoDB
builder.Services
    .AddOptions<MongoDb>()
    .Bind(builder.Configuration.GetSection("MongoDb"))
    .Validate(
        config =>
            !string.IsNullOrWhiteSpace(config.ConnectionString),
        "Debe configurar MongoDb:ConnectionString."
    )
    .Validate(
        config =>
            !string.IsNullOrWhiteSpace(config.DatabaseName),
        "Debe configurar MongoDb:DatabaseName."
    )
    .Validate(
        config =>
            !string.IsNullOrWhiteSpace(config.PedidosCollection),
        "Debe configurar MongoDb:PedidosCollection."
    )
    .ValidateOnStart();

// Repositorio MongoDB
builder.Services.AddSingleton<
    IPedidosRepositorio,
    PedidosRepositorio
>();

// Productor de Kafka
builder.Services.AddSingleton<KafkaProducerService>();

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Delivery API v1"
        );

        options.DocumentTitle = "Delivery API";
    });
}

app.UseHttpsRedirection();

app.UseCors("AngularApp");

app.UseAuthorization();

app.MapControllers();

app.Run();