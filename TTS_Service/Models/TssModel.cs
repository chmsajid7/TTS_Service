namespace TTS_Service.Models;

public class TssModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = string.Empty;
    public byte[] Speech { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;
}
