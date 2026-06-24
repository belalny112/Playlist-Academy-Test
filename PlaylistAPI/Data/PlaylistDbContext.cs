using Microsoft.EntityFrameworkCore;
using PlaylistAPI.Models;

namespace PlaylistAPI.Data;

public class PlaylistDbContext : DbContext
{
    public PlaylistDbContext(DbContextOptions<PlaylistDbContext> options) : base(options)
    {
    }

    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<Song> Songs { get; set; }
}