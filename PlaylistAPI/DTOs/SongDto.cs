namespace PlaylistAPI.DTOs;

public class SongDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public int DurationInSeconds { get; set; }
}
