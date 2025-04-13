using EmailService.Configuration;
using EmailService.Consumer;
using EmailService.Consumers;
using EmailService.EmailSender;
using EmailService.Events;
using EmailService.TemplateLoader;
using EmailService.Utils;
using MassTransit;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.Configure<SMTPOptions>(builder.Configuration.GetSection(SMTPOptions.SMTP));
builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection(KafkaOptions.KAFKA));

builder.Services.AddMediator((x) =>
{
    x.AddConsumer<SendWelcomeEmailCommandHandler>();
});

builder.Services.AddScoped<ITemplateLoader, TemplateLoader>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IKafkaConsumer<UserRegistredEvent>, KafkaConsumer<UserRegistredEvent>>();
builder.Services.AddHostedService<UserRegistredConsumer>();

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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => new { status = "healthy" });


app.Run();
