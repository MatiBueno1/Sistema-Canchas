using FutbolComplejo.API.Middleware;
using FutbolComplejo.Application.Interfaces;
using FutbolComplejo.Application.Services;
using FutbolComplejo.Infrastructure.Data;
using FutbolComplejo.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ── Base de datos ─────────────────────────────────────────────────────────────
var dbPath = builder.Environment.IsProduction()
    ? "/app/data/lamasia.db"
    : "lamasia.db";

Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

builder.Services.AddDbContext<FutbolComplejoDbContext>(opt =>
    opt.UseSqlite($"Data Source={dbPath}"));

// ── Repositorios ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<ICanchaRepository,      CanchaRepository>();
builder.Services.AddScoped<IUsuarioRepository,     UsuarioRepository>();
builder.Services.AddScoped<ITurnoRepository,       TurnoRepository>();
builder.Services.AddScoped<IListaEsperaRepository, ListaEsperaRepository>();

// ── Servicios ─────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IReservaService,     ReservaService>();
builder.Services.AddScoped<IListaEsperaService, ListaEsperaService>();
builder.Services.AddScoped<IWhatsAppService,    WhatsAppMockService>();

// ── Controllers ───────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
        opt.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// ── Swagger ───────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "⚽ La Masia API",
        Version     = "v1",
        Description = "API de reservas — La Masia Fútbol, Rosario."
    });
    c.EnableAnnotations();
});

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// ── Middleware ────────────────────────────────────────────────────────────────
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "La Masia API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "⚽ La Masia API";
});

// Servir index.html y admin.html como archivos estáticos desde wwwroot/
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();
app.MapControllers();

// /admin → sirve admin.html sin mostrar la extensión en la URL
app.MapGet("/admin", () => Results.File("wwwroot/admin.html", "text/html"));

// Cualquier ruta no-API cae a index.html
app.MapFallbackToFile("index.html");

// ── Crear DB al iniciar ───────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FutbolComplejoDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
