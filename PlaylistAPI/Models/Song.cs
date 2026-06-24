using System.ComponentModel.DataAnnotations;

namespace PlaylistAPI.Models;

public class Song 
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Every song must have a title.")]
    [MaxLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Artist { get; set; } = string.Empty;

    [Range(1, 3600, ErrorMessage = "Duration must be between 1 second and 1 hour.")]
    public int DurationInSeconds { get; set; }

    // Explicit FK — EF Core infers this by convention but declaring it is cleaner
    public int PlaylistId { get; set; }
}
