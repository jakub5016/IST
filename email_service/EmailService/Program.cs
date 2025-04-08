using EmailService.Configuration;
using EmailService.Consumers;
using EmailService.EmailSender;
using EmailService.Events;
using EmailService.TemplateLoader;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.Configure<SMTPOptions>(builder.Configuration.GetSection(SMTPOptions.SMTP));
builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection(KafkaOptions.KAFKA));

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<ITemplateLoader, TemplateLoader>();

builder.Services.AddMassTransit(x => {
    x.UsingInMemory();

    x.AddRider(rider =>
    {
        rider.AddConsumer<SendWelcomeEmailCommandHandler>();
        rider.UsingKafka((context, k) =>
        {
            var host = builder.Configuration.GetSection("Kafka").GetSection("ServerAddress").Value;
            k.Host(host);
           
            k.TopicEndpoint<UserRegistredEvent>("user_registred", "r", e =>
            {
                e.EnableAutoOffsetStore = true;
                e.UseRawJsonDeserializer();
                e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Latest;
                e.ConfigureConsumer<SendWelcomeEmailCommandHandler>(context);

            });
        });
    });
});

var app = builder.Build();

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
