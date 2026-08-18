namespace FlashcardApi.DTOs;

public class StudySettingsDto
{
    public bool ShuffleCards { get; set; } = true;
    public int CorrectAnswersRequired { get; set; } = 3;
}

public class StudySettingsResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int DeckId { get; set; }
    public bool ShuffleCards { get; set; }
    public int CorrectAnswersRequired { get; set; }
}
