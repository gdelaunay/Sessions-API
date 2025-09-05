using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sessions_API.Data;
using Sessions_API.Models;

namespace Sessions_API.Controllers;

[Authorize] 
[EnableCors]
[ApiController]
[Route("api/[controller]")]
public class SpotController(AppDbContext context) : Controller
{
    private readonly AppDbContext _context = context;
    
    [HttpGet]
    public async Task<ActionResult<List<Spot>>> GetAll()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();
        return Ok(await _context.Spots.Where(s => s.OwnerId == userId).ToListAsync());
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Spot>> Get(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();
        return await _context.Spots.FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == userId) ?? (ActionResult<Spot>) NotFound();
    }
    
    [HttpPost]
    public async Task<ActionResult<Spot>> Post(Spot spot)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();
        spot.OwnerId = userId;
        
        _context.Spots.Add(spot);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = spot.Id }, spot);
    }
    
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Spot>> Put(int id, Spot updatedSpot)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();
        
        var spot = await _context.Spots.FindAsync(id);
        if (spot == null) return BadRequest("Le spot n'existe pas.");
        if (spot.OwnerId != userId) return Unauthorized();
        
        _context.Entry(spot).CurrentValues.SetValues(updatedSpot);
        await _context.SaveChangesAsync();
        return NoContent();
    }
    
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<Spot>> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();
        
        var spot = await _context.Spots.FindAsync(id);
        if (spot == null) return NotFound("Le spot n'existe pas.");
        if (spot.OwnerId != userId) return Unauthorized();
        
        _context.Spots.Remove(spot);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
