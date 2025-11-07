using System.Reflection;
using System.IO;
using FixItNR.Api.Data;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Слушаем порт 8080 (нужно для Amvera/контейнера)
builder.WebHost.UseUrls("http://0.0.0.0:8080");

// ---------- Services ----------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FixIt NR API",
        Version = "v1",
        Description = "Учебный веб-API регистрации заявок с расчётом SLA.",
        Contact = new OpenApiContact { Name = "FixIt NR Team" }
    });

    // Подключаем XML-комментарии (если включена генерация в .csproj/свойствах)
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});

// In-memory репозиторий для ПР
builder.Services.AddSingleton<ITicketRepository, InMemoryTicketRepository>();

var app = builder.Build();

// ---------- Middleware ----------
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FixIt NR API v1");
    // c.RoutePrefix = string.Empty; // если хочешь открыть Swagger на корне "/"
});

// В контейнере HTTPS-редирект не нужен
// app.UseHttpsRedirection();

app.MapControllers();

app.Run();
