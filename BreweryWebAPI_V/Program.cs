using BreweryWebAPI_V.Configuration;
using BreweryWebAPI_V.Clients;
using BreweryWebAPI_V.Mappers;
using BreweryWebAPI_V.Services;
using BreweryWebAPI_V.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// bind options
builder.Services.ConfigureBreweryOptions(builder.Configuration);

// logging (default console logger is fine)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// controllers & swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BrewApi", Version = "v1" }));

// api versioning (URL versioning)
builder.Services.AddApiVersioning(opts =>
{
    opts.AssumeDefaultVersionWhenUnspecified = true;
    opts.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    opts.ReportApiVersions = true;
});

// memory cache
builder.Services.AddMemoryCache();

// HttpClient + typed client + small retry handler
builder.Services.AddHttpClients(builder.Configuration);

// DI for layers
builder.Services.AddScoped<IBreweryMapper, BreweryMapper>();
builder.Services.AddScoped<IBreweryService, BreweryService>();
builder.Services.AddScoped<IOpenBreweryClient, OpenBreweryClient>();

var app = builder.Build();

// middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();