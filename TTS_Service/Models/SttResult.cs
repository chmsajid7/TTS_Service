namespace TTS_Service.Models;

public class SttResult
{
    public string RecognitionStatus { get; set; }
    public int Offset { get; set; }
    public int Duration { get; set; }
    public string DisplayText { get; set; }
}
