using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurfSessions_API.Data;
using SurfSessions_API.Models;

namespace SurfSessions_API.Controllers;

[EnableCors]
[ApiController]
[Route("api/[controller]")]
public class SessionController(AppDbContext context) : Controller
{
    private readonly AppDbContext _context = context;

    [HttpGet]
    public async Task<ActionResult<List<Session>>> GetAll()
    {
        return Ok(await _context.Sessions.Include(s => s.Spot).ToListAsync());
    }
    
    [HttpGet("spotid/{spotId:int}")]
    public async Task<ActionResult<Session>> GetAllBySpot(int spotId)
    {
        return await _context.Sessions.FirstOrDefaultAsync(s => s.Spot.Id == spotId) ?? (ActionResult<Session>) NotFound();
    }
        
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Session>> Get(int id)
    {
        return await _context.Sessions.Include(s => s.Spot).FirstOrDefaultAsync(s => s.Id == id) ?? (ActionResult<Session>) NotFound();
    }
    
    [HttpPost]
    public async Task<ActionResult<Session>> Post(Session session)
    {
        var spot = await _context.Spots.FindAsync(session.Spot.Id);    
        if (spot == null)
        {
            return BadRequest("Le spot n'existe pas.");
        }
        session.Spot = spot;
        var forecast = await _context.Forecasts.FindAsync(session.Forecast.Id);
        if (forecast != null)
        {
            session.Forecast = forecast;
        }
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = session.Id }, session);
    }
    
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Session>> Put(int id, Session updatedSession)
    {
        var session = await _context.Sessions.FindAsync(id);
        if (session == null)
        {
            return BadRequest("La session n'existe pas.");
        }
        _context.Entry(session).CurrentValues.SetValues(updatedSession);
        await _context.SaveChangesAsync();
        return NoContent();
    }
    
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<Session>> Delete(int id)
    {
        var session = await _context.Sessions.FindAsync(id);
        if (session == null)
        {
            return NotFound("La session n'existe pas.");
        }
        _context.Sessions.Remove(session);
        await _context.SaveChangesAsync();
        return NoContent();
    }
        
}
