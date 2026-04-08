using LogisticsAI.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "AI Logistics Platform API",
        Version = "v1",
        Description = "ASP.NET Core backend for the AI Logistics Platform. " +
                      "Module 1: Natural Language → SQL | " +
                      "Module 2: Delay Prediction | " +
                      "Module 3: Route Optimization"
    });
});

// HTTP client for Python ML service
builder.Services.AddHttpClient("PythonApi", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Application services
builder.Services.AddScoped<AiService>();
builder.Services.AddScoped<SqlService>();
builder.Services.AddScoped<RouteService>();

// CORS – allow Blazor frontend (adjust origin in production)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.WithOrigins(
                builder.Configuration["Frontend:Origin"] ?? "http://localhost:5001",
                "http://localhost:5001",
                "https://localhost:7001")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Logistics Platform v1");
    c.RoutePrefix = string.Empty; // Serve Swagger UI at root
});

app.UseCors("AllowBlazor");

app.UseAuthorization();

app.MapControllers();

app.Run();

// Make the Program class accessible for integration tests
public partial class Program { }
