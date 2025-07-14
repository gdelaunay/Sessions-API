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


// ------------------------------ SERVICES ------------------------------ //

// Ajout des controllers
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

// Ajout du service de prévisions météo, appel à l'API Open-meteo et transformation des données
builder.Services.AddHttpClient<WeatherApiService>();

// Ajout du service BDD
builder.Services.AddDbContext<AppDbContext>(options => options.UseMySQL(Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING")!)); // Développement: MYSQL_CONNECTION_STRING_DEV

// Ajout d'une configuration Cors
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins(allowedOrigins!)
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithExposedHeaders("Location");
        });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Ajout d'un timestamp aux logs
builder.Logging.AddSimpleConsole(options => { options.TimestampFormat = "yyyy/dd/MM HH:mm:ss - "; });


// ------------------------------ BUILD ------------------------------ //

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("gdelaunay.Sessions-API");

// Chargement des variables d'environnement du fichier .env
DotEnvService.Logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("gdelaunay.DotEnvService");
DotEnvService.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));


// ------------------------------ CONFIGURATION ------------------------------ //

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Mappage pour download le json OpenApi
    app.MapGet("/openapi/download", async context =>
    {
        var httpClient = new HttpClient();
        var json = await httpClient.GetStringAsync($"{context.Request.Scheme}://{context.Request.Host}/openapi/v1.json");
        context.Response.Headers.Append("Content-Disposition", "attachment; filename=openapi-v1.json");
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(json);
    });
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

// Effectue la migration de la BDD
if (args.Contains("--migrate"))
{
    using var scope = app.Services.CreateScope();
    AppDbService.MigrateIfNeeded(scope.ServiceProvider, 3, 10);
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowSpecificOrigins");
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();
app.MapGet("/", () => "Bonjour monde!");


// ------------------------------ START ------------------------------ //

await app.StartAsync();

if (app.Environment.IsDevelopment())
{
    logger.LogInformation($"Document OpenAPI : {app.Urls.FirstOrDefault()}/openapi/v1.json");
    logger.LogInformation($"Lien de téléchargement : {app.Urls.FirstOrDefault()}/openapi/download");
}

await app.WaitForShutdownAsync();
