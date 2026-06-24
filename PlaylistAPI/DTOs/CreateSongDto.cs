using System.ComponentModel.DataAnnotations;

namespace PlaylistAPI.DTOs;

public class CreateSongDto
{
    [Required(ErrorMessage = "Every song must have a title.")]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Artist { get; set; } = string.Empty;

    [Range(1, 3600)]
    public int DurationInSeconds { get; set; }
}
