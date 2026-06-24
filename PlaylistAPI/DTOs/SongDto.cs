namespace PlaylistAPI.DTOs;

public class SongDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Artist { get; set; }
    public int DurationInSeconds { get; set; }
}