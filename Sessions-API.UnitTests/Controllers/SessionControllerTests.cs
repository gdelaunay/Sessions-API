using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sessions_API.Controllers;
using Sessions_API.Data;
using Sessions_API.Models;

namespace Sessions_API.UnitTests.Controllers;

public class SessionControllerTests
{
    private static SessionController GetController(AppDbContext context)
    {
        var sessionController = new SessionController(context);
        sessionController.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        return sessionController;
    }
    
    private static SessionController GetControllerWithUser(AppDbContext context, string userId)
    {
        var sessionController = new SessionController(context);
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "mock"));
        sessionController.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        return sessionController;
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
        var sessionController = GetController(context);
        
        var resultGet = await sessionController.Get(1);
        Assert.IsType<UnauthorizedResult>(resultGet.Result);     
        
        var resultGetAll = await sessionController.GetAll();
        Assert.IsType<UnauthorizedResult>(resultGet.Result);
        
        var resultPost = await sessionController.Post(new Session
        {
            Id = 1, 
            Spot = new Spot { Id = 1, Name = "Margaret - Préfailles", Latitude = 47.12f, Longitude = -2.21f, Sessions = [], OwnerId = "user-1"}, 
            Forecast = new Forecast(),
            OwnerId = "user-1"
        });
        Assert.IsType<UnauthorizedResult>(resultPost.Result);
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyUserSessions()
    {
        await using var context = GetInMemoryDb();
        context.Sessions.Add(new Session
        {
            Id = 1, 
            Spot = new Spot { Id = 1, Name = "Margaret - Préfailles", Latitude = 47.12f, Longitude = -2.21f, Sessions = [], OwnerId = "user-1"}, 
            Forecast = new Forecast(),
            OwnerId = "user-1"
        });
        context.Sessions.Add(new Session
        {
            Id = 2, 
            Spot = new Spot { Id = 2, Name = "Grande plage - Hossegor", Latitude = 47.12f, Longitude = -2.21f, Sessions = [], OwnerId = "user-2"}, 
            Forecast = new Forecast(),
            OwnerId = "user-2"
        });
        await context.SaveChangesAsync();

        var controller = GetControllerWithUser(context, "user-1");
        var result = await controller.GetAll();
        
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var sessions = Assert.IsType<List<Session>>(okResult.Value);
        Assert.Single(sessions);
        Assert.Equal("user-1", sessions[0].OwnerId);
    }

    [Fact]
    public async Task Get_ReturnsSession_WhenOwnedByUser()
    {
        await using var context = GetInMemoryDb();
        context.Sessions.Add(new Session
        {
            Id = 1, 
            Spot = new Spot { Id = 1, Name = "Margaret - Préfailles", Latitude = 47.12f, Longitude = -2.21f, Sessions = [], OwnerId = "user-1"}, 
            Forecast = new Forecast(),
            OwnerId = "user-1"
        });
        await context.SaveChangesAsync();

        var controller = GetControllerWithUser(context, "user-1");
        var result = await controller.Get(1);
        
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedSession = Assert.IsType<Session>(okResult.Value);
        Assert.Equal(1, returnedSession.Id);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenNotOwnedByUser()
    {
        await using var context = GetInMemoryDb();
        context.Sessions.Add(new Session
        {
            Id = 1, 
            Spot = new Spot { Id = 1, Name = "Grande plage - Lacanau", Latitude = 47.12f, Longitude = -2.21f, Sessions = [], OwnerId = "user-2"}, 
            Forecast = new Forecast(),
            OwnerId = "user-2"
        });
        await context.SaveChangesAsync();

        var controller = GetControllerWithUser(context, "user-1");
        var result = await controller.Get(1);
        
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Post_CreatesSession_ForUser()
    {
        await using var context = GetInMemoryDb();
        context.Spots.Add(new Spot { Id = 1, Name = "Margaret - Préfailles", Latitude = 47.12f, Longitude = -2.21f, Sessions = [], OwnerId = "user-1"});
        await context.SaveChangesAsync();

        var newSession = new Session
        {
            Id = 1,
            Spot = new Spot { Id = 1, Name = "Margaret - Préfailles", Latitude = 47.12f, Longitude = -2.21f, Sessions = [], OwnerId = "user-1"},
            Forecast = new Forecast(),
            OwnerId = "user-1"
        };
        var controller = GetControllerWithUser(context, "user-1");
        var result = await controller.Post(newSession);
        
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var session = Assert.IsType<Session>(created.Value);
        Assert.Equal("user-1", session.OwnerId);
    }

    [Fact]
    public async Task Put_UpdatesSession_WhenOwnedByUser()
    {
        await using var context = GetInMemoryDb();
        context.Sessions.Add(new Session
        {
            Id = 1, 
            Spot = new Spot { Id = 1, Name = "Margaret - Préfailles", Latitude = 47.12f, Longitude = -2.21f, Sessions = [], OwnerId = "user-1"}, 
            Forecast = new Forecast(),
            Rating = 2,
            OwnerId = "user-1"
        });
        await context.SaveChangesAsync();

        var updatedSession = new Session
        {
            Id = 1,
            Spot = new Spot
            {
                Id = 1, Name = "Margaret - Préfailles", Latitude = 47.12f, Longitude = -2.21f, Sessions = [],
                OwnerId = "user-1"
            },
            Rating = 5,
            Forecast = new Forecast(),
            OwnerId = "user-1"
        };
        var controller = GetControllerWithUser(context, "user-1");

        var result = await controller.Put(1, updatedSession);
        Assert.IsType<NoContentResult>(result.Result);
        
        var sessionsInDb = await context.Sessions.FindAsync(1);
        Assert.NotNull(sessionsInDb);
        Assert.Equal(5, sessionsInDb.Rating);
    }

    [Fact]
    public async Task Put_ReturnsUnauthorized_WhenNotOwnedByUser()
    {
        await using var context = GetInMemoryDb();
        context.Sessions.Add(new Session
        {
            Id = 1, 
            Spot = new Spot { Id = 1, Name = "Grande plage - Lacanau", Latitude = 47.12f, Longitude = -2.21f, Sessions = [], OwnerId = "user-2"}, 
            Forecast = new Forecast(),
            Rating = 2,
            OwnerId = "user-2"
        });
        await context.SaveChangesAsync();

        var updatedSession = new Session
        {
            Id = 1, 
            Spot = new Spot { Id = 1, Name = "Grande plage - Lacanau", Latitude = 47.12f, Longitude = -2.21f, Sessions = [], OwnerId = "user-2"}, 
            Forecast = new Forecast(),
            Rating = 5,
            OwnerId = "user-2"
        };
        var controller = GetControllerWithUser(context, "user-1");

        var result = await controller.Put(1, updatedSession);
        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task Delete_RemovesSession_WhenOwnedByUser()
    {
        await using var context = GetInMemoryDb();
        context.Sessions.Add(new Session
        {
            Id = 1, 
            Spot = new Spot { Id = 1, Name = "Margaret - Préfailles", Latitude = 47.12f, Longitude = -2.21f, Sessions = [], OwnerId = "user-1"}, 
            Forecast = new Forecast(),
            Rating = 2,
            OwnerId = "user-1"
        });
        await context.SaveChangesAsync();

        var controller = GetControllerWithUser(context, "user-1");
        var result = await controller.Delete(1);
        
        Assert.IsType<NoContentResult>(result.Result);
        var sessionInDb = await context.Sessions.FindAsync(1);
        Assert.Null(sessionInDb);
    }

    [Fact]
    public async Task Delete_ReturnsUnauthorized_WhenNotOwnedByUser()
    {
        await using var context = GetInMemoryDb();
        context.Sessions.Add(new Session
        {
            Id = 1, 
            Spot = new Spot { Id = 1, Name = "Grande plage - Lacanau", Latitude = 47.12f, Longitude = -2.21f, Sessions = [], OwnerId = "user-2"}, 
            Forecast = new Forecast(),
            Rating = 2,
            OwnerId = "user-2"
        });
        await context.SaveChangesAsync();

        var controller = GetControllerWithUser(context, "user-1");
        var result = await controller.Delete(1);
        
        Assert.IsType<UnauthorizedResult>(result.Result);
    }
}