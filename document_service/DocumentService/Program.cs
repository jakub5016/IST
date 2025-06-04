using Amazon.S3;
using Confluent.Kafka;
using DocumentService.Application.Appointments.Commands.Finish;
using DocumentService.Application.Documents.Commands.UploadDocument;
using DocumentService.Application.Documents.Queries.GetDocument;
using DocumentService.Application.Documents.Queries.GetDocuments;
using DocumentService.Domain.Appointments;
using DocumentService.Domain.Documents;
using DocumentService.Infrastracture.Database;
using DocumentService.Infrastracture.Messaging;
using DocumentService.Infrastracture.Storage;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Configuration
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddDbContext<DocumentContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DataConnection"))
);

builder.Services.Configure<S3Options>(builder.Configuration.GetSection("Storage"));
builder.Services.AddScoped<IFileStorage, S3Storage>();
builder.Services.AddAWSService<IAmazonS3>();

builder.Services.AddMediator(x =>
{
    x.AddConsumer<UploadDocumentCommandHandler>();
    x.AddConsumer<GetDocumentQueryHandler>();
    x.AddConsumer<GetDocumentsQueryHandler>();
});

builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory();
    x.AddRider(rider =>
    {
        var kafkaOptions = builder.Configuration.GetSection("Kafka").Get<KafkaOptions>();
        rider.AddConsumer<FinishAppointmentCommandHandler>();
        rider.AddProducer<FinishAppointmentCommand>(kafkaOptions.FinishAppointmentTopic);
        rider.AddProducer<DocumentCreated>(kafkaOptions.DocumentCreatedTopic);
        rider.UsingKafka((context, k) =>
        {
            k.Host(kafkaOptions.ServerAddress);
            k.TopicEndpoint<FinishAppointmentCommand>(kafkaOptions.FinishAppointmentTopic, "r", e =>
            {
                e.EnableAutoOffsetStore = true;
                e.UseRawJsonDeserializer();
                e.AutoOffsetReset = AutoOffsetReset.Latest;
                e.ConfigureConsumer<FinishAppointmentCommandHandler>(context);
                e.CreateIfMissing(t =>
                {
                    t.NumPartitions = 1;
                    t.ReplicationFactor = 1;
                });
            });
            k.TopicEndpoint<DocumentCreated>(kafkaOptions.DocumentCreatedTopic, "r", e =>
            {
                e.EnableAutoOffsetStore = true;
                e.UseRawJsonDeserializer();
                e.AutoOffsetReset = AutoOffsetReset.Latest;
                e.CreateIfMissing(t =>
                {
                    t.NumPartitions = 1;
                    t.ReplicationFactor = 1;
                });
            });
        });
    });
});

builder.Services.Configure<MassTransitHostOptions>(options =>
{
    options.WaitUntilStarted = false;
    options.StartTimeout = TimeSpan.FromSeconds(30);
    options.StopTimeout = TimeSpan.FromMinutes(1);
});

var app = builder.Build();


app.MapOpenApi();


using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DocumentContext>();
    dbContext.Database.Migrate();
}

app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapGet("/health", () => new { status = "healthy" });
app.Run();
