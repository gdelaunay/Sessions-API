using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SurfSessions_API;
using SurfSessions_API.Data;
using SurfSessions_API.Services;

var builder = WebApplication.CreateBuilder(args);

// Culture en-US pour parse les float au format "1.23" au lieu de "1,23"
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("fr-FR");

// Chargement des variables d'environnement du fichier .env
DotEnv.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
Console.WriteLine("VARIABLE_TEST = " + Environment.GetEnvironmentVariable("VARIABLE_TEST"));


// Add services to the container.
// Ajout des controllers
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

// Ajout du service d'appel à l'API de prévision
// (et transformation des données)
builder.Services.AddHttpClient<WeatherApiService>();


// Add DB connection & DB context
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseMySQL(Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING")!));

// Add allowed origins (local & dist)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200",
                    "http://sessions.gdelaunay.fr",
                    "https://sessions.gdelaunay.fr")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithExposedHeaders("Location");
        });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapGet("/", () => "Hello World!");

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowSpecificOrigins");

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

await app.RunAsync();
