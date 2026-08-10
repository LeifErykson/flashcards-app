namespace FlashcardApi.Models;

public class Flashcard
{
    public int Id { get; set; }
    public string Front { get; set; } = string.Empty;    // Question / prompt
    public string Back { get; set; } = string.Empty;     // Answer / definition
    public int DeckId { get; set; }                      // Which deck it belongs to
    public Deck? Deck { get; set; }                      // Navigation (optional)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
