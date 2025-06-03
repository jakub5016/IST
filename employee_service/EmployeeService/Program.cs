using Confluent.Kafka;
using EmployeeService.Application.Commands.Cancel;
using EmployeeService.Application.Commands.Dismiss;
using EmployeeService.Application.Commands.Hire;
using EmployeeService.Application.Queries.GetById;
using EmployeeService.Application.Queries.GetDoctors;
using EmployeeService.Application.Queries.GetEmployees;
using EmployeeService.Domain;
using EmployeeService.Domain.Event;
using EmployeeService.Infrastracture.Database;
using EmployeeService.Infrastracture.Messaging;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .AddEnvironmentVariables();

builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<IValidator<HireEmployeeCommand>,HireEmployeeCommandValidator>();    

builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddDbContext<EmployeeContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DataConnection"))
);

builder.Services.AddMediator(x =>
{
    x.AddConsumer<DismissCommandHandler>();
    x.AddConsumer<HireEmployeeCommandHandler>();
    x.AddConsumer<GetByIdQueryHandler>();
    x.AddConsumer<GetDoctorsQueryHander>();
    x.AddConsumer<GetEmployeesQueryHandler>();
});

builder.Services.AddMassTransit(x => {
    x.UsingInMemory();
    var kafkaOptions = builder.Configuration.GetSection("Kafka").Get<KafkaOptions>();
    x.AddRider(rider =>
    {
        rider.AddProducer<EmployeeHired>(kafkaOptions.EmployeeHiredTopic);
        rider.AddProducer<EmployeeDismissed>(kafkaOptions.EmployeeDismissedTopic);

        rider.AddProducer<CancelEmploymentCommand>(kafkaOptions.EmployeeRegistrationFailedTopic);

        rider.AddConsumer<CancelEmploymentCommandHandler>();
        rider.UsingKafka((context, k) =>
        {
            k.Host(kafkaOptions.ServerAddress);
            k.TopicEndpoint<CancelEmploymentCommand>(kafkaOptions.EmployeeRegistrationFailedTopic, "r", e =>
            {
                e.EnableAutoOffsetStore = true;
                e.UseRawJsonDeserializer();
                e.AutoOffsetReset = AutoOffsetReset.Latest;
                e.ConfigureConsumer<CancelEmploymentCommandHandler>(context);
                e.CreateIfMissing(t =>
                {
                    t.NumPartitions = 1;
                    t.ReplicationFactor = 1;
                });
            });
            k.TopicEndpoint<EmployeeDismissed>(kafkaOptions.EmployeeDismissedTopic, "r", e =>
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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EmployeeContext>();
    dbContext.Database.Migrate();
}
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
