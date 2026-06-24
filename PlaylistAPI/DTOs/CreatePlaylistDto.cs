using System.ComponentModel.DataAnnotations;

namespace PlaylistAPI.DTOs;

public class CreatePlaylistDto
{
    [Required(ErrorMessage = "Your playlist needs a name!")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}
