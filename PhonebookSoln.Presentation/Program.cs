using RabbitMQ.Client;
using PhonebookSoln.Application.Interfaces;
using PhonebookSoln.Application.Services;
using PhonebookSoln.Core.Interfaces;
using PhonebookSoln.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using PhonebookSoln.Infrastructure.Data;
using PhonebookSoln.Infrastructure.Repositories;
using PhonebookSoln.Infrastructure.Consumer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("PhonebookSoln.Presentation")
            ));

builder.Services.AddControllers();

builder.Services.AddScoped<IPhonebookRepository, PhonebookRepository>();
builder.Services.AddScoped<IContactInfoService, ContactInfoService>();
builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IRabbitMqService, RabbitMqService>();
builder.Services.AddScoped<IOutboxService, OutboxService>();

builder.Services.AddSingleton<IConnectionFactory, ConnectionFactory>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    return new ConnectionFactory()
    {
        Uri = new Uri(builder.Configuration["RabbitMQ:ConnectionString"]),
        AutomaticRecoveryEnabled = true,
        TopologyRecoveryEnabled = true
    };
});

builder.Services.AddHostedService<RabbitMqConsumer>();

builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();