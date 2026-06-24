namespace PlaylistAPI.DTOs;

public class PlaylistDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    // Notice it uses SongDto, not the raw Song database model!
    public List<SongDto> Songs { get; set; } = new List<SongDto>(); 
}