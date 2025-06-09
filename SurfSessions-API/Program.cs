using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SurfSessions_API;
using SurfSessions_API.Data;

var builder = WebApplication.CreateBuilder(args);

// Chargement des variables d'environnement du fichier .env
DotEnv.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
// Exemple d'utilisation :
// Console.WriteLine( "VARIABLE_TEST = " + Environment.GetEnvironmentVariable("VARIABLE_TEST"));


// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});;

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
                    "http://192.168.1.72:83",
                    "http://192.168.1.72",
                    "http://[2a02:842a:8217:8e01:265e:beff:fe3c:8d95]:83",
                    "http://[2a02:842a:8217:8e01:265e:beff:fe3c:8d95]",
                    "http://gdelaunay.fr")
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

app.Run();
