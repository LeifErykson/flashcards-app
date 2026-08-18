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
public class StudySettingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StudySettingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/studysettings/deck/{deckId}
    [HttpGet("deck/{deckId}")]
    public async Task<ActionResult<StudySettingsResponseDto>> GetSettings(int deckId)
    {
        var userId = GetUserId();

        // Check if user has access to this deck
        var deck = await _context.Decks.FindAsync(deckId);
        if (deck == null)
            return NotFound("Deck not found");

        if (deck.OwnerId != userId && !deck.IsPublic)
            return Forbid("You don't have access to this deck");

        // Get or create default settings
        var settings = await _context.StudySettings
            .FirstOrDefaultAsync(ss => ss.UserId == userId && ss.DeckId == deckId);

        if (settings == null)
        {
            // Return defaults if no settings exist
            return Ok(new StudySettingsResponseDto
            {
                Id = 0,
                UserId = userId,
                DeckId = deckId,
                ShuffleCards = true,
                CorrectAnswersRequired = 3
            });
        }

        return Ok(new StudySettingsResponseDto
        {
            Id = settings.Id,
            UserId = settings.UserId,
            DeckId = settings.DeckId,
            ShuffleCards = settings.ShuffleCards,
            CorrectAnswersRequired = settings.CorrectAnswersRequired
        });
    }

    // PUT: api/studysettings/deck/{deckId}
    [HttpPut("deck/{deckId}")]
    public async Task<IActionResult> UpdateSettings(int deckId, StudySettingsDto settingsDto)
    {
        var userId = GetUserId();

        // Check if deck exists
        var deck = await _context.Decks.FindAsync(deckId);
        if (deck == null)
            return NotFound("Deck not found");

        // Only deck owner can change settings (or any user with access?)
        // For now, allow any user who can access the deck to set their preferences
        if (deck.OwnerId != userId && !deck.IsPublic)
            return Forbid("You don't have access to this deck");

        // Find existing settings or create new
        var settings = await _context.StudySettings
            .FirstOrDefaultAsync(ss => ss.UserId == userId && ss.DeckId == deckId);

        if (settings == null)
        {
            settings = new StudySettings
            {
                UserId = userId,
                DeckId = deckId,
                ShuffleCards = settingsDto.ShuffleCards,
                CorrectAnswersRequired = settingsDto.CorrectAnswersRequired
            };
            _context.StudySettings.Add(settings);
        }
        else
        {
            settings.ShuffleCards = settingsDto.ShuffleCards;
            settings.CorrectAnswersRequired = settingsDto.CorrectAnswersRequired;
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
