using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurfSessions_API.Data;
using SurfSessions_API.Models;

namespace SurfSessions_API.Controllers;

[EnableCors]
[ApiController]
[Route("api/[controller]")]
public class SpotController(AppDbContext context) : Controller
{
    private readonly AppDbContext _context = context;

    [HttpGet]
    public async Task<ActionResult<List<Spot>>> GetAll()
    {
        return Ok(await _context.Spots.ToListAsync());
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Spot>> Get(int id)
    {
        return await _context.Spots.FirstOrDefaultAsync(s => s.Id == id) ?? (ActionResult<Spot>) NotFound();
    }
    
    [HttpPost]
    public async Task<ActionResult<Spot>> Post(Spot spot)
    {
        _context.Spots.Add(spot);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = spot.Id }, spot);
    }
    
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Spot>> Put(int id, Spot updatedSpot)
    {
        var spot = await _context.Spots.FindAsync(id);
        if (spot == null)
        {
            return BadRequest("Le spot n'existe pas.");
        }
        _context.Entry(spot).CurrentValues.SetValues(updatedSpot);
        await _context.SaveChangesAsync();
        return NoContent();
    }
    
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<Spot>> Delete(int id)
    {
        var spot = await _context.Spots.FindAsync(id);
        if (spot == null)
        {
            return NotFound("Le spot n'existe pas.");
        }
        _context.Spots.Remove(spot);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
