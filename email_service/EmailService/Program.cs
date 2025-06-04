using DocumentService.Domain.Documents;
using EmailService.Application.Commands;
using EmailService.Application.Events;
using EmailService.Domain;
using EmailService.Infrastracture.Email.EmailSender;
using EmailService.Infrastracture.Email.TemplateLoader;
using EmailService.Infrastracture.EventStore;
using EmailService.Infrastracture.EventStore.Configuration;
using EmailService.Infrastracture.Messaging.Configuration;
using EmailService.Infrastracture.Notification.Configuration;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
BsonClassMap.RegisterClassMap<UserRegistred>();
var objectSerializer = new ObjectSerializer(ObjectSerializer.AllAllowedTypes);
BsonSerializer.RegisterSerializer(objectSerializer);
builder.Configuration
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.Configure<SMTPOptions>(builder.Configuration.GetSection(SMTPOptions.SMTP));
builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection(KafkaOptions.KAFKA));
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection(MongoDBSettings.MONGO));
var mongoDBSettings = builder.Configuration.GetSection(MongoDBSettings.MONGO).Get<MongoDBSettings>();


builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    return client.GetDatabase(settings.DatabaseName);
});

builder.Services.AddScoped<ITemplateLoader, TemplateLoader>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IEventStoreHandler, EventStoreHandler>();
builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory();
    x.AddRider(rider =>
    {
        rider.AddConsumer<SendWelcomeEmailCommandHandler>();
        rider.AddConsumer<SendPasswordChangeEmailCommandHandler>();
        rider.AddConsumer<SendNewAppointmentEmailCommandHandler>();
        rider.AddConsumer<SendCancelAppointmentEmailCommandHandler>();
        rider.AddConsumer<SendDocumentEmailCommandHandler>();
        rider.AddConsumer<SendZoomMeetingEmailCommandHandler>();
        rider.UsingKafka((context, k) =>
        {
            var host = builder.Configuration.GetSection("Kafka").GetSection("ServerAddress").Value;
            k.Host(host);

            k.TopicEndpoint<DocumentCreated>(builder.Configuration.GetSection(KafkaOptions.KAFKA).GetSection("DocumentCreatedTopic").Value, "r", e =>
            {
                e.EnableAutoOffsetStore = true;
                e.UseRawJsonDeserializer();
                e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Latest;
                e.ConfigureConsumer<SendDocumentEmailCommandHandler>(context);
                e.CreateIfMissing(t =>
                {
                    t.NumPartitions = 1;
                    t.ReplicationFactor = 1;
                });
            });

            k.TopicEndpoint<UserRegistred>(builder.Configuration.GetSection(KafkaOptions.KAFKA).GetSection("UserRegistredTopic").Value, "r", e =>
            {
                e.EnableAutoOffsetStore = true;
                e.UseRawJsonDeserializer();
                e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Latest;
                e.ConfigureConsumer<SendWelcomeEmailCommandHandler>(context);
                e.CreateIfMissing(t =>
                {
                    t.NumPartitions = 1;
                    t.ReplicationFactor = 1;
                });
            });

            k.TopicEndpoint<ChangePassword>(builder.Configuration.GetSection(KafkaOptions.KAFKA).GetSection("ChangePasswordTopic").Value, "r", e =>
            {
                e.EnableAutoOffsetStore = true;
                e.UseRawJsonDeserializer();
                e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Latest;
                e.ConfigureConsumer<SendPasswordChangeEmailCommandHandler>(context);
                e.CreateIfMissing(t =>
                {
                    t.NumPartitions = 1;
                    t.ReplicationFactor = 1;
                });
            });

            k.TopicEndpoint<AppointmentCreated>(builder.Configuration.GetSection(KafkaOptions.KAFKA).GetSection("AppointmentCreatedTopic").Value, "r", e =>
            {
                e.EnableAutoOffsetStore = true;
                e.UseRawJsonDeserializer();
                e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Latest;
                e.ConfigureConsumer<SendNewAppointmentEmailCommandHandler>(context);
                e.CreateIfMissing(t =>
                {
                    t.NumPartitions = 1;
                    t.ReplicationFactor = 1;
                });
            });

            k.TopicEndpoint<AppointmentCancelled>(builder.Configuration.GetSection(KafkaOptions.KAFKA).GetSection("AppointmentCancelledTopic").Value, "r", e =>
            {
                e.EnableAutoOffsetStore = true;
                e.UseRawJsonDeserializer();
                e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Latest;
                e.ConfigureConsumer<SendCancelAppointmentEmailCommandHandler>(context);
                e.CreateIfMissing(t =>
                {
                    t.NumPartitions = 1;
                    t.ReplicationFactor = 1;
                });
            });
            k.TopicEndpoint<ZoomCreated>(builder.Configuration.GetSection(KafkaOptions.KAFKA).GetSection("ZoomCreatedTopic").Value, "r", e =>
            {
                e.EnableAutoOffsetStore = true;
                e.UseRawJsonDeserializer();
                e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Latest;
                e.ConfigureConsumer<SendZoomMeetingEmailCommandHandler>(context);
                e.CreateIfMissing(t =>
                {
                    t.NumPartitions = 1;
                    t.ReplicationFactor = 1;
                });
            });
        });
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => new { status = "healthy" });


app.Run();
