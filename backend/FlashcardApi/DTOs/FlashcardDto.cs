namespace FlashcardApi.DTOs;

public class CreateFlashcardDto
{
    public string Front { get; set; } = string.Empty;
    public string Back { get; set; } = string.Empty;
}

public class UpdateFlashcardDto
{
    public string Front { get; set; } = string.Empty;
    public string Back { get; set; } = string.Empty;
}

public class FlashcardResponseDto
{
    public int Id { get; set; }
    public string Front { get; set; } = string.Empty;
    public string Back { get; set; } = string.Empty;
    public int DeckId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
