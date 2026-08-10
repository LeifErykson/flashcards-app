namespace FlashcardApi.Models;

public class StudySettings
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int DeckId { get; set; }
    public Deck? Deck { get; set; }
    
    public bool ShuffleCards { get; set; } = true;        // Shuffle by default
    public int CorrectAnswersRequired { get; set; } = 3;  // How many correct before mastered
}
