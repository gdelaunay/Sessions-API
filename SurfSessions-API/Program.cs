using SurfSessions_API;

var builder = WebApplication.CreateBuilder(args);

// Chargement des variables d'environnement du fichier .env
DotEnv.Load(Path.Combine(Directory.GetCurrentDirectory(), "SurfSessions-API.env"));
// Exemple d'utilisation : Console.WriteLine( "VARIABLE_TEST = " + Environment.GetEnvironmentVariable("VARIABLE_TEST"));


// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => "Hello World!");

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
