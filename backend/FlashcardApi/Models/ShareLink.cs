namespace FlashcardApi.Models;

public class ShareLink
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;    // Unique identifier for the link
    public int DeckId { get; set; }
    public Deck? Deck { get; set; }
    public int CreatedBy { get; set; }                   // Which user created the share link
    public bool AllowEdit { get; set; } = false;         // Can the viewer edit?
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }             // Optional expiry
}
