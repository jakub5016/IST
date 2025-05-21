using Amazon.S3;
using DocumentService.Application.Commands.UploadDocument;
using DocumentService.Application.Queries.GetDocument;
using DocumentService.Application.Queries.GetDocuments;
using DocumentService.Domain;
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
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddDbContext<DocumentContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DataConnection"))
);

builder.Services.Configure<S3Options>(builder.Configuration.GetSection("Storage"));
builder.Services.AddScoped<IFileStorage,S3Storage>();
builder.Services.AddAWSService<IAmazonS3>();

builder.Services.AddMediator(x => {
    x.AddConsumer<UploadDocumentCommandHandler>();
    x.AddConsumer<GetDocumentQueryHandler>();
    x.AddConsumer<GetDocumentsQueryHandler>();
});

builder.Services.Configure<MassTransitHostOptions>(options =>
{
    options.WaitUntilStarted = true;
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

app.Run();
