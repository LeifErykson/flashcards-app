namespace FlashcardApi.DTOs;

public class StudySessionStartDto
{
    public int DeckId { get; set; }
}

public class StudyCardDto
{
    public int CardId { get; set; }
    public string Front { get; set; } = string.Empty;
    public string Back { get; set; } = string.Empty;
    public bool IsMastered { get; set; }  // Already mastered by this user
}

public class StudyResultDto
{
    public int CardId { get; set; }
    public bool IsCorrect { get; set; }
}

public class StudySessionResponseDto
{
    public int SessionId { get; set; }
    public List<StudyCardDto> Cards { get; set; } = new List<StudyCardDto>();
    public int TotalCards { get; set; }
    public int MasteredCount { get; set; }
}

public class StudyProgressDto
{
    public int DeckId { get; set; }
    public int TotalCards { get; set; }
    public int MasteredCards { get; set; }
    public int RemainingCards { get; set; }
    public double ProgressPercentage { get; set; }
}
