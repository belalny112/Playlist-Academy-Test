using System.ComponentModel.DataAnnotations;

namespace PlaylistAPI.Models;

public class Song 
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Every song must have a title.")]
    [MaxLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
    public string Title { get; set; }

    [Required]
    public string Artist { get; set; }

    [Range(1, 3600, ErrorMessage = "Duration must be between 1 second and 1 hour.")]
    public int DurationInSeconds { get; set; }
}