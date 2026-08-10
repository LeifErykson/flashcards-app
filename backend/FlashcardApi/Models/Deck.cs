namespace FlashcardApi.Models;

public class Deck
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = false;          // false = private by default
    public int OwnerId { get; set; }                     // Who created it
    public User? Owner { get; set; }                     // Navigation (optional)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<Flashcard> Flashcards { get; set; } = new List<Flashcard>();
}
