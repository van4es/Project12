using FixItNR.Api.Data;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Слушаем порт 8080 (Amvera)
builder.WebHost.UseUrls("http://0.0.0.0:8080");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FixIt NR API",
        Version = "v1",
        Description = "Учебный веб-API регистрации заявок с расчётом SLA.",
        Contact = new OpenApiContact { Name = "FixIt NR Team" },
        License = new OpenApiLicense { Name = "MIT (учебный проект)" }
    });

    // Подхватываем XML-комментарии (см. .csproj ниже)
    var xml = Path.Combine(AppContext.BaseDirectory, "Project1.xml");
    if (File.Exists(xml))
        c.IncludeXmlComments(xml, includeControllerXmlComments: true);
});

// In-memory репозиторий
builder.Services.AddSingleton<ITicketRepository, InMemoryTicketRepository>();

var app = builder.Build();

// Swagger и в проде — для проверки в Amvera
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FixIt NR API v1");
    // c.RoutePrefix = "swagger"; // по умолчанию /swagger
});

// Без внутреннего HTTPS в контейнере редирект не нужен
// app.UseHttpsRedirection();

app.MapControllers();
app.Run();
