using System.Globalization;
using System.Text;
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
DotEnv.Load(Path.Combine(Directory.GetCurrentDirectory(), "/App/.env"));
Console.WriteLine(Environment.GetEnvironmentVariable("VARIABLE_TEST"));


// ------------------------------ SERVICES ------------------------------ //

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
builder.Services.AddDbContext<AppDbContext>(options => options.UseMySQL(Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING")));

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

// Add timestamp to logs
builder.Logging.AddSimpleConsole(options => { options.TimestampFormat = "HH:mm:ss - "; });

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("gdelaunay.Sessions-API");


// ------------------------------ CONFIGURATION ------------------------------ //

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

// Configure un middleware logger HTTP
app.Use(async (context, next) =>
{
    var body = "";
    if (new[] { "POST", "PUT", "PATCH" }.Contains(context.Request.Method))
    {
        context.Request.EnableBuffering();
        body = await new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true).ReadToEndAsync();
        context.Request.Body.Position = 0;
    }

    var loggerHttp = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("gdelaunay.HttpRequestLogger");
    loggerHttp.LogInformation(
        $"Method : {context.Request.Method} \n" +
        $"      Path : {context.Request.Path + context.Request.QueryString} \n" +
        (string.IsNullOrEmpty(body) ? "" : $"      Body : \n{body} \n") +
        $"      Host : {context.Request.Headers["Host"]} \n" +
        $"      Adress : {
            context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? context.Connection.RemoteIpAddress?.ToString()
        }"
    );
    
    await next();
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowSpecificOrigins");
app.UseAuthorization();


// ------------------------------ MAPPING  ------------------------------ //
app.MapStaticAssets();

app.MapGet("/", () => "Bonjour monde!");

app.MapControllers();

// Mappage pour download le json OpenApi
app.MapGet("/openapi/download", async context =>
{
    var httpClient = new HttpClient();
    var json = await httpClient.GetStringAsync($"{context.Request.Scheme}://{context.Request.Host}/openapi/v1.json");
    context.Response.Headers.Append("Content-Disposition", "attachment; filename=openapi-v1.json");
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync(json);
});

await app.StartAsync();

logger.LogInformation($"Document OpenAPI : {app.Urls.FirstOrDefault()}/openapi/v1.json");
logger.LogInformation($"Lien de téléchargement : {app.Urls.FirstOrDefault()}/openapi/download");

await app.WaitForShutdownAsync();
