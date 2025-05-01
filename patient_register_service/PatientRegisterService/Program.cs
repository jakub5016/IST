using FluentValidation;
using MassTransit;
using PatientRegisterService.Commands.Register;
using PatientRegisterService.Config;
using PatientRegisterService.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddMediator(x => x.AddConsumer<RegisterCommandHandler>());
builder.Services.AddMassTransit(x => {
    x.UsingInMemory();
    x.AddRider(rider =>
    {
        var kafkaOptions = builder.Configuration.GetSection("Kafka").Get<KafkaOptions>();
        rider.AddProducer<PatientRegister>(kafkaOptions.PatientRegisterTopic);
        rider.UsingKafka((context, k) =>
        {
            k.Host(kafkaOptions.ServerAddress);


        });
    });
});
builder.Services.Configure<MassTransitHostOptions>(options =>
{
    options.WaitUntilStarted = true;
    options.StartTimeout = TimeSpan.FromMinutes(1);
    options.StopTimeout = TimeSpan.FromMinutes(1);
});

builder.Services.AddScoped<IValidator<RegisterCommand>, RegisterCommandValidator>();

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();
app.Run();
