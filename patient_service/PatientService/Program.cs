using Confluent.Kafka;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PatientService.Application.Command.ConfirmPatientIdentity;
using PatientService.Application.Command.ConfirmPatientNumber;
using PatientService.Application.Command.Register;
using PatientService.Application.Command.CancelRegistration;
using PatientService.Application.Command.Update;
using PatientService.Application.Queries.GetById;
using PatientService.Domain;
using PatientService.Domain.Shared;
using PatientService.Infrastracture;
using PatientService.Infrastracture.Database;
using PatientService.Infrastracture.Messaging;
using PatientService.Infrastracture.Messaging.IntegrationEvents;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IPacientRepository, PatientRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IValidator<RegisterCommand>, RegisterCommandValidator>();
builder.Services.AddScoped<IValidator<UpdateCommand>, UpdateCommandValidator>();

builder.Services.AddMediator(x =>
{
    x.AddConsumer<ConfirmIdentityCommandHandler>();
    x.AddConsumer<ConfirmNumberCommandHandler>();
    x.AddConsumer<UpdateCommandHandler>();
    x.AddConsumer<GetByIdCommandHandler>();
    x.AddConsumer<RegisterCommandHandler>();

});

builder.Services.AddMassTransit(x => {
    x.UsingInMemory();
    x.AddRider(rider =>
    {
        var kafkaOptions = builder.Configuration.GetSection("Kafka").Get<KafkaOptions>();
        rider.AddProducer<PatientRegistered>(kafkaOptions.PatientRegisteredTopic);
        rider.AddProducer<UserCreationFailed>(kafkaOptions.UserCreationFailedTopic);
        rider.AddConsumer<CancelRegistrationCommandHandler>();
        rider.AddConsumer<RegisterCommandHandler>();

        rider.UsingKafka((context, k) =>
        {
            k.Host(kafkaOptions.ServerAddress);
            k.TopicEndpoint<UserCreationFailed>(kafkaOptions.UserCreationFailedTopic, "r", e =>
            {
                e.EnableAutoOffsetStore = true;
                e.UseRawJsonDeserializer();
                e.AutoOffsetReset = AutoOffsetReset.Latest;
                e.ConfigureConsumer<CancelRegistrationCommandHandler>(context);
                e.CreateIfMissing(t =>
                {
                    t.NumPartitions = 1;
                    t.ReplicationFactor = 1;
                });
            });
            k.TopicEndpoint<RegisterCommand>(kafkaOptions.PatientRegisterTopic, "r", e =>
            {
                e.EnableAutoOffsetStore = true;
                e.UseRawJsonDeserializer();
                e.AutoOffsetReset = AutoOffsetReset.Latest;
                e.ConfigureConsumer<RegisterCommandHandler>(context);
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

builder.Services.AddDbContext<PatientContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DataConnection"))
);
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PatientContext>();
    dbContext.Database.Migrate();
}

app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();
