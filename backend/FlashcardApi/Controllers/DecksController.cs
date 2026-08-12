using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FlashcardApi.Data;
using FlashcardApi.Models;
using FlashcardApi.DTOs;

namespace FlashcardApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]  // All endpoints require authentication
public class DecksController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DecksController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/decks
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeckResponseDto>>> GetMyDecks()
    {
        var userId = GetUserId();

        var decks = await _context.Decks
            .Where(d => d.OwnerId == userId)
            .Include(d => d.Owner)
            .Include(d => d.Flashcards)
            .OrderByDescending(d => d.UpdatedAt)
            .Select(d => new DeckResponseDto
            {
                Id = d.Id,
                Title = d.Title,
                Description = d.Description,
                IsPublic = d.IsPublic,
                OwnerId = d.OwnerId,
                OwnerUsername = d.Owner != null ? d.Owner.Username : "Unknown",
                CardCount = d.Flashcards.Count,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .ToListAsync();

        return Ok(decks);
    }

    // GET: api/decks/public
    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<DeckResponseDto>>> GetPublicDecks()
    {
        var decks = await _context.Decks
            .Where(d => d.IsPublic)
            .Include(d => d.Owner)
            .Include(d => d.Flashcards)
            .OrderByDescending(d => d.UpdatedAt)
            .Select(d => new DeckResponseDto
            {
                Id = d.Id,
                Title = d.Title,
                Description = d.Description,
                IsPublic = d.IsPublic,
                OwnerId = d.OwnerId,
                OwnerUsername = d.Owner != null ? d.Owner.Username : "Unknown",
                CardCount = d.Flashcards.Count,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .ToListAsync();

        return Ok(decks);
    }

    // GET: api/decks/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<DeckResponseDto>> GetDeck(int id)
    {
        var userId = GetUserId();

        var deck = await _context.Decks
            .Include(d => d.Owner)
            .Include(d => d.Flashcards)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (deck == null)
            return NotFound("Deck not found");

        // Check if user has access (owner OR public deck)
        if (deck.OwnerId != userId && !deck.IsPublic)
            return Forbid("You don't have access to this deck");

        return Ok(new DeckResponseDto
        {
            Id = deck.Id,
            Title = deck.Title,
            Description = deck.Description,
            IsPublic = deck.IsPublic,
            OwnerId = deck.OwnerId,
            OwnerUsername = deck.Owner?.Username ?? "Unknown",
            CardCount = deck.Flashcards.Count,
            CreatedAt = deck.CreatedAt,
            UpdatedAt = deck.UpdatedAt
        });
    }

    // POST: api/decks
    [HttpPost]
    public async Task<ActionResult<DeckResponseDto>> CreateDeck(CreateDeckDto createDto)
    {
        var userId = GetUserId();

        var deck = new Deck
        {
            Title = createDto.Title,
            Description = createDto.Description,
            IsPublic = createDto.IsPublic,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Decks.Add(deck);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDeck), new { id = deck.Id }, new DeckResponseDto
        {
            Id = deck.Id,
            Title = deck.Title,
            Description = deck.Description,
            IsPublic = deck.IsPublic,
            OwnerId = deck.OwnerId,
            CardCount = 0,
            CreatedAt = deck.CreatedAt,
            UpdatedAt = deck.UpdatedAt
        });
    }

    // PUT: api/decks/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDeck(int id, UpdateDeckDto updateDto)
    {
        var userId = GetUserId();

        var deck = await _context.Decks.FindAsync(id);
        if (deck == null)
            return NotFound("Deck not found");

        // Only owner can edit
        if (deck.OwnerId != userId)
            return Forbid("You can only edit your own decks");

        deck.Title = updateDto.Title;
        deck.Description = updateDto.Description;
        deck.IsPublic = updateDto.IsPublic;
        deck.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/decks/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDeck(int id)
    {
        var userId = GetUserId();

        var deck = await _context.Decks.FindAsync(id);
        if (deck == null)
            return NotFound("Deck not found");

        // Only owner can delete
        if (deck.OwnerId != userId)
            return Forbid("You can only delete your own decks");

        _context.Decks.Remove(deck);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }
}
