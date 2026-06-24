namespace PlaylistAPI.DTOs;

public class PlaylistDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<SongDto> Songs { get; set; } = new List<SongDto>();
}
