using System.ComponentModel.DataAnnotations;

namespace PlaylistAPI.Models;

public class Playlist 
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Your playlist needs a name!")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    // Optional field — nullable string makes the intent explicit
    [MaxLength(500)]
    public string? Description { get; set; }

    public List<Song> Songs { get; set; } = new List<Song>();
}
