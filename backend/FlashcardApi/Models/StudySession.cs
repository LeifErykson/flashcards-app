namespace FlashcardApi.Models;

public class StudySession
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int DeckId { get; set; }
    public Deck? Deck { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int CardsReviewed { get; set; }
    public int CorrectCount { get; set; }
}
