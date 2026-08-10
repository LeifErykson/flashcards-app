namespace FlashcardApi.Models;

public class CardProgress
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int CardId { get; set; }
    public Flashcard? Card { get; set; }
    
    public int CorrectCount { get; set; } = 0;            // How many times answered correctly
    public int IncorrectCount { get; set; } = 0;          // How many times answered incorrectly
    public DateTime? LastReviewedAt { get; set; }
    public bool IsMastered { get; set; } = false;         // True when CorrectCount >= required
}
