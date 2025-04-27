using Confluent.Kafka;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PatientService;
using PatientService.Application.Command.CancelRegistration;
using PatientService.Application.Command.ConfirmPatientIdentity;
using PatientService.Application.Command.ConfirmPatientNumber;
using PatientService.Application.Command.Register;
using PatientService.Application.Command.UndoRegistration;
using PatientService.Application.Command.Update;
using PatientService.Application.Queries.GetById;
using PatientService.Application.Saga;
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
// Add services to the container.


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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
    x.AddConsumer<RegisterCommandHandler>();
    x.AddConsumer<CancelRegistrationCommandHandler>();
    x.AddConsumer<UpdateCommandHandler>();
    x.AddConsumer<GetByIdCommandHandler>();
});

builder.Services.AddMassTransit(x => {
    x.UsingInMemory();
   x.AddSagaStateMachine<PatientRegistrationSaga, PatientRegistrationSagaData>().EntityFrameworkRepository(r =>
   {
       r.ExistingDbContext<PatientContext>();
       r.UsePostgres();
   });
    /*x.AddRider(rider =>
    {
        var kafkaOptions = builder.Configuration.GetSection("Kafka").Get<KafkaOptions>();
        rider.AddProducer<Null, CreateUser>(kafkaOptions.CreateUserTopic);
        rider.AddProducer<Null, SendWelcomeEmail>(kafkaOptions.SendWelcomeEmailTopic);
        rider.AddProducer<Null, CancelPatientRegistration>(kafkaOptions.CancelPatientRegistrationTopic);


        rider.UsingKafka((context, k) =>
        {
            k.Host(kafkaOptions.ServerAddress);
            
            k.TopicEndpoint<CancelPatientRegistration>(kafkaOptions.CancelPatientRegistrationTopic, "r", e =>
            {
                e.EnableAutoOffsetStore = true;
                e.UseRawJsonDeserializer();
                e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Latest;
                e.ConfigureConsumer<CancelRegistrationCommandHandler>(context);
            });

            k.TopicEndpoint<PatientRegistrationCancelled>(kafkaOptions.PatientRegistrationCancelledTopic, "r", e =>
            {
                e.EnableAutoOffsetStore = true;
                e.UseRawJsonDeserializer();
                e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Latest;
            });

            k.TopicEndpoint<WelcomeEmailSent>(kafkaOptions.WelcomeEmailSentTopic, "r", e =>
            {
                e.EnableAutoOffsetStore = true;
                e.UseRawJsonDeserializer();
                e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Latest;
            });

            k.TopicEndpoint<UserCreated>(kafkaOptions.UserCreatedTopic, "r", e =>
            {
                e.EnableAutoOffsetStore = true;
                e.UseRawJsonDeserializer();
                e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Latest;
            });
            k.TopicEndpoint<PatientRegistered>(kafkaOptions.PatientRegistredTopic, "r", e =>
            {
                e.EnableAutoOffsetStore = true;
                e.UseRawJsonDeserializer();
                e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Latest;
            });
           
        });
        
    });
   */
});

builder.Services.AddDbContext<PatientContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DataConnection"))
);
var app = builder.Build();

// Configure the HTTP request pipeline.
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
