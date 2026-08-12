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
[Authorize]
public class FlashcardsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public FlashcardsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/flashcards/deck/{deckId}
    [HttpGet("deck/{deckId}")]
    public async Task<ActionResult<IEnumerable<FlashcardResponseDto>>> GetFlashcardsByDeck(int deckId)
    {
        var userId = GetUserId();

        // Check if user has access to this deck
        var deck = await _context.Decks.FindAsync(deckId);
        if (deck == null)
            return NotFound("Deck not found");

        if (deck.OwnerId != userId && !deck.IsPublic)
            return Forbid("You don't have access to this deck");

        var flashcards = await _context.Flashcards
            .Where(f => f.DeckId == deckId)
            .OrderBy(f => f.CreatedAt)
            .Select(f => new FlashcardResponseDto
            {
                Id = f.Id,
                Front = f.Front,
                Back = f.Back,
                DeckId = f.DeckId,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            })
            .ToListAsync();

        return Ok(flashcards);
    }

    // GET: api/flashcards/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<FlashcardResponseDto>> GetFlashcard(int id)
    {
        var userId = GetUserId();

        var flashcard = await _context.Flashcards
            .Include(f => f.Deck)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (flashcard == null)
            return NotFound("Flashcard not found");

        // Check access via deck ownership
        if (flashcard.Deck == null)
            return NotFound("Deck not found");

        if (flashcard.Deck.OwnerId != userId && !flashcard.Deck.IsPublic)
            return Forbid("You don't have access to this flashcard");

        return Ok(new FlashcardResponseDto
        {
            Id = flashcard.Id,
            Front = flashcard.Front,
            Back = flashcard.Back,
            DeckId = flashcard.DeckId,
            CreatedAt = flashcard.CreatedAt,
            UpdatedAt = flashcard.UpdatedAt
        });
    }

    // POST: api/flashcards/deck/{deckId}
    [HttpPost("deck/{deckId}")]
    public async Task<ActionResult<FlashcardResponseDto>> CreateFlashcard(int deckId, CreateFlashcardDto createDto)
    {
        var userId = GetUserId();

        // Verify deck ownership
        var deck = await _context.Decks.FindAsync(deckId);
        if (deck == null)
            return NotFound("Deck not found");

        if (deck.OwnerId != userId)
            return Forbid("You can only add flashcards to your own decks");

        var flashcard = new Flashcard
        {
            Front = createDto.Front,
            Back = createDto.Back,
            DeckId = deckId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Flashcards.Add(flashcard);
        await _context.SaveChangesAsync();

        // Update deck's UpdatedAt timestamp
        deck.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetFlashcard), new { id = flashcard.Id }, new FlashcardResponseDto
        {
            Id = flashcard.Id,
            Front = flashcard.Front,
            Back = flashcard.Back,
            DeckId = flashcard.DeckId,
            CreatedAt = flashcard.CreatedAt,
            UpdatedAt = flashcard.UpdatedAt
        });
    }

    // PUT: api/flashcards/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFlashcard(int id, UpdateFlashcardDto updateDto)
    {
        var userId = GetUserId();

        var flashcard = await _context.Flashcards
            .Include(f => f.Deck)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (flashcard == null)
            return NotFound("Flashcard not found");

        if (flashcard.Deck == null || flashcard.Deck.OwnerId != userId)
            return Forbid("You can only edit flashcards in your own decks");

        flashcard.Front = updateDto.Front;
        flashcard.Back = updateDto.Back;
        flashcard.UpdatedAt = DateTime.UtcNow;

        // Update deck's UpdatedAt timestamp
        if (flashcard.Deck != null)
        {
            flashcard.Deck.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/flashcards/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFlashcard(int id)
    {
        var userId = GetUserId();

        var flashcard = await _context.Flashcards
            .Include(f => f.Deck)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (flashcard == null)
            return NotFound("Flashcard not found");

        if (flashcard.Deck == null || flashcard.Deck.OwnerId != userId)
            return Forbid("You can only delete flashcards from your own decks");

        _context.Flashcards.Remove(flashcard);

        // Update deck's UpdatedAt timestamp
        if (flashcard.Deck != null)
        {
            flashcard.Deck.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }
}
