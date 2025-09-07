using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sessions_API.Controllers;
using Sessions_API.Data;
using Sessions_API.Models;

namespace Sessions_API.UnitTests.Controllers;

public class SpotControllerTests
{
    private static SpotController GetController(AppDbContext context)
    {
        var spotController = new SpotController(context);
        spotController.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext()};
        return spotController;
    }
    
    private static SpotController GetControllerWithUser(AppDbContext context, string userId)
    {
        var spotController = new SpotController(context);
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "mock"));
        spotController.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        return spotController;
    }

    private static AppDbContext GetInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new AppDbContext(options);
    }
    
    [Fact]
    public async Task All_ReturnsUnauthorized_WhenUnauthenticated()
    {
        await using var context = GetInMemoryDb();
        var spotController = GetController(context);
        
        var resultGet = await spotController.Get(1);
        Assert.IsType<UnauthorizedResult>(resultGet.Result);     
        
        var resultGetAll = await spotController.GetAll();
        Assert.IsType<UnauthorizedResult>(resultGetAll.Result);
        
        var resultPost = await spotController.Post(new Spot { Id = 1, Name = "Margaret - Préfailles", Latitude = 47.12f, Longitude = -2.21f, Sessions = [], OwnerId = "user-1"});
        Assert.IsType<UnauthorizedResult>(resultPost.Result);

        var resultPut = await spotController.Put(1, new Spot { Id = 1, Name = "Margaret Plage - Préfailles", Latitude = 47.12f, Longitude = -2.21f});
        Assert.IsType<UnauthorizedResult>(resultPut.Result);
        
        var deleteResult = await spotController.Delete(1);
        Assert.IsType<UnauthorizedResult>(deleteResult.Result);
    }    
    
    [Fact]
    public async Task GetAll_ReturnsOnlyUserSpots()
    {
        await using var context = GetInMemoryDb();
        context.Spots.Add(new Spot { Id = 1, Name = "Margaret - Préfailles", Latitude = 47.12f, Longitude = -2.21f, Sessions = [], OwnerId = "user-1"});
        context.Spots.Add(new Spot { Id = 2, Name = "Grande plage - Hossegor", Latitude = 47.12f, Longitude = -2.21f, Sessions = [], OwnerId = "user-2"});
        context.Spots.Add(new Spot { Id = 3, Name = "Grande plage - Oléron", Latitude = 47.12f, Longitude = -2.21f, Sessions = []});
        await context.SaveChangesAsync();

        var spotController = GetControllerWithUser(context, "user-1");
        var result = await spotController.GetAll();
        
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var spots = Assert.IsType<List<Spot>>(okResult.Value);
        Assert.Single(spots);
        Assert.Equal("Margaret - Préfailles", spots[0].Name);
    }
    
    [Fact]
    public async Task Get_ReturnsSpot_WhenOwnedByUser()
    {
        await using var context = GetInMemoryDb();
        context.Spots.Add(new Spot { Id = 1, Name = "Margaret - Préfailles", Latitude = 47.12f, Longitude = -2.21f, Sessions = [], OwnerId = "user-1"});
        context.Spots.Add(new Spot { Id = 2, Name = "Grande plage - Hossegor", Latitude = 47.12f, Longitude = -2.21f, Sessions = [], OwnerId = "user-1"});
        await context.SaveChangesAsync();

        var spotController = GetControllerWithUser(context, "user-1");
        var result = await spotController.Get(1);
        
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var spot = Assert.IsType<Spot>(okResult.Value);
        Assert.Equal("Margaret - Préfailles", spot.Name);
    }    
    
    [Fact]
    public async Task Get_ReturnsNotFound_WhenNotOwnedByUser()
    {
        await using var context = GetInMemoryDb();
        context.Spots.Add(new Spot { Id = 1, Name = "Margaret - Préfailles", Latitude = 47.12f, Longitude = -2.21f, Sessions = [], OwnerId = "user-1"});
        context.Spots.Add(new Spot { Id = 2, Name = "Grande plage - Hossegor", Latitude = 47.12f, Longitude = -2.21f, Sessions = [], OwnerId = "user-2"});
        await context.SaveChangesAsync();

        var spotController = GetControllerWithUser(context, "user-1");
        var result = await spotController.Get(2);
        
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Le spot n'existe pas.", notFoundResult.Value);
    }
    
    [Fact]
    public async Task Post_CreatesSpot_ForUser()
    {
        await using var context = GetInMemoryDb();
        var newSpot = new Spot { Id = 1, Name = "Margaret - Préfailles", Latitude = 47.12f, Longitude = -2.21f, Sessions = [], OwnerId = "user-1"};
        
        var spotController = GetControllerWithUser(context, "user-1");
        var result = await spotController.Post(newSpot);
        
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var spot = Assert.IsType<Spot>(created.Value);
        Assert.Equal("Margaret - Préfailles", spot.Name);
        Assert.Equal("user-1", spot.OwnerId);
    }

    
    [Fact]
    public async Task Put_UpdatesSpot_WhenOwnedByUser()
    {
        await using var context = GetInMemoryDb();
        var spot = new Spot { Id = 1, Name = "Margaret - Quiberon", Latitude = 47.12f, Longitude = -2.21f, Sessions = [], OwnerId = "user-1"};
        context.Spots.Add(spot);
        await context.SaveChangesAsync();

        var spotController = GetControllerWithUser(context, "user-1");
        var updatedSpot = new Spot { Id = 1, Name = "Margaret - Préfailles", Latitude = 47.12f, Longitude = -2.21f};

        var result = await spotController.Put(1, updatedSpot);
        Assert.IsType<NoContentResult>(result.Result);

        var spotInDb = await context.Spots.FindAsync(1);
        Assert.NotNull(spotInDb);
        Assert.Equal("Margaret - Préfailles", spotInDb.Name);
    }

    [Fact]
    public async Task Put_ReturnsUnauthorized_WhenNotOwnedByUser()
    {
        await using var context = GetInMemoryDb();
        var spot = new Spot { Id = 1, Name = "Margaret - Quiberon", Latitude = 47.12f, Longitude = -2.21f, Sessions = [], OwnerId = "user-2"};
        context.Spots.Add(spot);
        await context.SaveChangesAsync();

        var spotController = GetControllerWithUser(context, "user-1");
        var result = await spotController.Put(1, new Spot { Id = 1, Name = "Margaret - Préfailles", Latitude = 47.12f, Longitude = -2.21f});

        Assert.IsType<UnauthorizedResult>(result.Result);
    }
    
    
    [Fact]
    public async Task Delete_RemovesSpot_WhenOwnedByUser()
    {
        await using var context = GetInMemoryDb();
        var spot = new Spot { Id = 1, Name = "Margaret - Quiberon", Latitude = 47.12f, Longitude = -2.21f, Sessions = [], OwnerId = "user-1"};
        context.Spots.Add(spot);
        await context.SaveChangesAsync();

        var spotController = GetControllerWithUser(context, "user-1");
        var result = await spotController.Delete(1);
        
        Assert.IsType<NoContentResult>(result.Result);
        var spotInDb = await context.Spots.FindAsync(1);
        Assert.Null(spotInDb);
    }
    
    [Fact]
    public async Task Delete_ReturnsUnauthorized_WhenNotOwnedByUser()
    {
        await using var context = GetInMemoryDb();
        var spot = new Spot { Id = 1, Name = "Margaret - Quiberon", Latitude = 47.12f, Longitude = -2.21f, Sessions = [], OwnerId = "user-2"};
        context.Spots.Add(spot);
        await context.SaveChangesAsync();

        var spotController = GetControllerWithUser(context, "user-1");
        var result = await spotController.Delete(1);
        
        Assert.IsType<UnauthorizedResult>(result.Result);
    }
    
}
