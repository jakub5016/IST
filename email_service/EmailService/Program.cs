using EmailService.Commands;
using EmailService.Configuration;
using EmailService.Consumers;
using EmailService.EmailSender;
using EmailService.Events;
using EmailService.TemplateLoader;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.Configure<SMTPOptions>(builder.Configuration.GetSection(SMTPOptions.SMTP));
builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection(KafkaOptions.KAFKA));

builder.Services.AddScoped<ITemplateLoader, TemplateLoader>();
builder.Services.AddScoped<IEmailSender, EmailSender>();

builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory();
    x.AddRider(rider =>
    {
        rider.AddConsumer<SendWelcomeEmailCommandHandler>();
        rider.AddConsumer<SendPasswordChangeEmailCommandHandler>();
        rider.AddConsumer<SendNewAppointmentEmailCommandHandler>();
        rider.AddConsumer<SendCancelAppointmentEmailCommandHandler>();
        rider.UsingKafka((context, k) =>
        {
            var host = builder.Configuration.GetSection("Kafka").GetSection("ServerAddress").Value;
            k.Host(host);

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
