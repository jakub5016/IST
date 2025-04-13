using MassTransit;
using Microsoft.EntityFrameworkCore;
using PatientService.Application.Command.ConfirmPatientIdentity;
using PatientService.Application.Command.ConfirmPatientNumber;
using PatientService.Application.Command.Register;
using PatientService.Application.Command.UndoRegistration;
using PatientService.Application.Command.Update;
using PatientService.Application.Queries.GetById;
using PatientService.Domain;
using PatientService.Infrastracture;
using PatientService.Infrastracture.Database;
using System.Reflection;

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
builder.Services.AddMediator(x =>
{
    x.AddConsumer<ConfirmIdentityCommandHandler>();
    x.AddConsumer<ConfirmNumberCommandHandler>();
    x.AddConsumer<RegisterCommandHandler>();
    x.AddConsumer<UndoRegistrationCommandHandler>();
    x.AddConsumer<UpdateCommandHandler>();
    x.AddConsumer<GetByIdCommandHandler>();
});

builder.Services.AddMassTransit(x => {
    x.UsingInMemory();

    x.AddRider(rider =>
    {
        rider.UsingKafka((context, k) =>
        {
            var host = builder.Configuration.GetSection("Kafka").GetSection("ServerAddress").Value;
            k.Host(host);

           
        });
    });
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
