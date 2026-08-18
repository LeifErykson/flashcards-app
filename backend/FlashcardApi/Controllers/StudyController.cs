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
public class StudyController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StudyController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: api/study/start
    [HttpPost("start")]
    public async Task<ActionResult<StudySessionResponseDto>> StartStudy(StudySessionStartDto startDto)
    {
        var userId = GetUserId();

        // Verify deck exists and user has access
        var deck = await _context.Decks.FindAsync(startDto.DeckId);
        if (deck == null)
            return NotFound("Deck not found");

        if (deck.OwnerId != userId && !deck.IsPublic)
            return Forbid("You don't have access to this deck");

        // Get user's settings for this deck
        var settings = await _context.StudySettings
            .FirstOrDefaultAsync(ss => ss.UserId == userId && ss.DeckId == startDto.DeckId);

        var shuffle = settings?.ShuffleCards ?? true;
        var requiredCorrect = settings?.CorrectAnswersRequired ?? 3;

        // Get all flashcards in the deck
        var flashcards = await _context.Flashcards
            .Where(f => f.DeckId == startDto.DeckId)
            .ToListAsync();

        if (!flashcards.Any())
            return BadRequest("This deck has no flashcards to study");

        // Get progress for each card
        var cardIds = flashcards.Select(f => f.Id).ToList();
        var progress = await _context.CardProgress
            .Where(cp => cp.UserId == userId && cardIds.Contains(cp.CardId))
            .ToDictionaryAsync(cp => cp.CardId, cp => cp);

        // Determine which cards to study (not yet mastered)
        var studyCards = new List<StudyCardDto>();
        var masteredCount = 0;

        foreach (var card in flashcards)
        {
            var isMastered = false;
            if (progress.TryGetValue(card.Id, out var cardProgress))
            {
                isMastered = cardProgress.CorrectCount >= requiredCorrect;
                if (isMastered) masteredCount++;
            }

            studyCards.Add(new StudyCardDto
            {
                CardId = card.Id,
                Front = card.Front,
                Back = card.Back,
                IsMastered = isMastered
            });
        }

        // Filter out mastered cards (only study unmastered)
        var unmasteredCards = studyCards.Where(c => !c.IsMastered).ToList();

        // Apply shuffle if enabled
        if (shuffle)
        {
            var rng = new Random();
            unmasteredCards = unmasteredCards.OrderBy(_ => rng.Next()).ToList();
        }

        // Create a study session record (optional, for tracking history)
        var session = new StudySession
        {
            UserId = userId,
            DeckId = startDto.DeckId,
            StartedAt = DateTime.UtcNow,
            CardsReviewed = 0,
            CorrectCount = 0
        };

        _context.StudySessions.Add(session);
        await _context.SaveChangesAsync();

        return Ok(new StudySessionResponseDto
        {
            SessionId = session.Id,
            Cards = unmasteredCards,
            TotalCards = flashcards.Count,
            MasteredCount = masteredCount
        });
    }

    // POST: api/study/record
    [HttpPost("record")]
    public async Task<IActionResult> RecordResult(StudyResultDto resultDto)
    {
        var userId = GetUserId();

        // Verify card exists
        var card = await _context.Flashcards.FindAsync(resultDto.CardId);
        if (card == null)
            return NotFound("Card not found");

        // Get or create progress record
        var progress = await _context.CardProgress
            .FirstOrDefaultAsync(cp => cp.UserId == userId && cp.CardId == resultDto.CardId);

        if (progress == null)
        {
            progress = new CardProgress
            {
                UserId = userId,
                CardId = resultDto.CardId,
                CorrectCount = 0,
                IncorrectCount = 0,
                IsMastered = false
            };
            _context.CardProgress.Add(progress);
        }

        // Update progress
        if (resultDto.IsCorrect)
        {
            progress.CorrectCount++;
        }
        else
        {
            progress.IncorrectCount++;
        }

        // Check if mastered (based on user's settings for this deck)
        var settings = await _context.StudySettings
            .FirstOrDefaultAsync(ss => ss.UserId == userId && ss.DeckId == card.DeckId);

        var requiredCorrect = settings?.CorrectAnswersRequired ?? 3;
        progress.IsMastered = progress.CorrectCount >= requiredCorrect;
        progress.LastReviewedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Update the most recent study session (optional)
        var latestSession = await _context.StudySessions
            .Where(s => s.UserId == userId && s.DeckId == card.DeckId && s.EndedAt == null)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync();

        if (latestSession != null)
        {
            latestSession.CardsReviewed++;
            if (resultDto.IsCorrect) latestSession.CorrectCount++;
            if (latestSession.CardsReviewed >= 10) // Optional: auto-close after 10 cards
            {
                latestSession.EndedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }

        return Ok(new { message = "Progress recorded" });
    }

    // GET: api/study/progress/{deckId}
    [HttpGet("progress/{deckId}")]
    public async Task<ActionResult<StudyProgressDto>> GetProgress(int deckId)
    {
        var userId = GetUserId();

        var deck = await _context.Decks.FindAsync(deckId);
        if (deck == null)
            return NotFound("Deck not found");

        if (deck.OwnerId != userId && !deck.IsPublic)
            return Forbid("You don't have access to this deck");

        var flashcards = await _context.Flashcards
            .Where(f => f.DeckId == deckId)
            .ToListAsync();

        if (!flashcards.Any())
            return Ok(new StudyProgressDto
            {
                DeckId = deckId,
                TotalCards = 0,
                MasteredCards = 0,
                RemainingCards = 0,
                ProgressPercentage = 0
            });

        var settings = await _context.StudySettings
            .FirstOrDefaultAsync(ss => ss.UserId == userId && ss.DeckId == deckId);

        var requiredCorrect = settings?.CorrectAnswersRequired ?? 3;

        var cardIds = flashcards.Select(f => f.Id).ToList();
        var progress = await _context.CardProgress
            .Where(cp => cp.UserId == userId && cardIds.Contains(cp.CardId))
            .ToDictionaryAsync(cp => cp.CardId, cp => cp);

        var masteredCount = 0;
        foreach (var card in flashcards)
        {
            if (progress.TryGetValue(card.Id, out var cardProgress))
            {
                if (cardProgress.CorrectCount >= requiredCorrect)
                    masteredCount++;
            }
        }

        var totalCards = flashcards.Count;
        var remaining = totalCards - masteredCount;
        var percentage = totalCards > 0 ? (masteredCount * 100.0 / totalCards) : 0;

        return Ok(new StudyProgressDto
        {
            DeckId = deckId,
            TotalCards = totalCards,
            MasteredCards = masteredCount,
            RemainingCards = remaining,
            ProgressPercentage = Math.Round(percentage, 1)
        });
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }

    // POST: api/study/reset/{deckId}
    [HttpPost("reset/{deckId}")]
    public async Task<IActionResult> ResetProgress(int deckId)
    {
        var userId = GetUserId();

        // Verify deck exists and user owns it
        var deck = await _context.Decks.FindAsync(deckId);
        if (deck == null)
            return NotFound("Deck not found");

        if (deck.OwnerId != userId)
            return Forbid("You can only reset progress for your own decks");

        // Get all flashcards in this deck
        var flashcards = await _context.Flashcards
            .Where(f => f.DeckId == deckId)
            .Select(f => f.Id)
            .ToListAsync();

        if (!flashcards.Any())
            return BadRequest("This deck has no flashcards");

        // Find all progress records for this user and these cards
        var progressRecords = await _context.CardProgress
            .Where(cp => cp.UserId == userId && flashcards.Contains(cp.CardId))
            .ToListAsync();

        if (progressRecords.Any())
        {
            // Reset all progress records
            foreach (var progress in progressRecords)
            {
                progress.CorrectCount = 0;
                progress.IncorrectCount = 0;
                progress.IsMastered = false;
                progress.LastReviewedAt = null;
            }
            
            await _context.SaveChangesAsync();
        }

        return Ok(new { message = "Progress reset successfully" });
    }
}