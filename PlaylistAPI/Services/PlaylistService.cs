using Microsoft.EntityFrameworkCore;
using PlaylistAPI.Data;
using PlaylistAPI.DTOs;
using PlaylistAPI.Models;

namespace PlaylistAPI.Services;

public class PlaylistService : IPlaylistService
{
    private readonly PlaylistDbContext _context;

    public PlaylistService(PlaylistDbContext context)
    {
        _context = context;
    }

    // --- Reusable private mapper (eliminates duplicated mapping code) ---
    private static PlaylistDto MapToDto(Playlist playlist) => new PlaylistDto
    {
        Id = playlist.Id,
        Name = playlist.Name,
        Description = playlist.Description,
        Songs = playlist.Songs.Select(s => new SongDto
        {
            Id = s.Id,
            Title = s.Title,
            Artist = s.Artist,
            DurationInSeconds = s.DurationInSeconds
        }).ToList()
    };

    public async Task<IEnumerable<PlaylistDto>> GetAllPlaylistsAsync()
    {
        var playlists = await _context.Playlists
            .Include(p => p.Songs)
            .ToListAsync();

        return playlists.Select(MapToDto);
    }

    public async Task<PlaylistDto?> GetPlaylistByIdAsync(int id)
    {
        var playlist = await _context.Playlists
            .Include(p => p.Songs)
            .FirstOrDefaultAsync(p => p.Id == id);

        return playlist is null ? null : MapToDto(playlist);
    }

    public async Task<PlaylistDto> CreatePlaylistAsync(CreatePlaylistDto dto)
    {
        var playlist = new Playlist
        {
            Name = dto.Name,
            Description = dto.Description
        };

        _context.Playlists.Add(playlist);
        await _context.SaveChangesAsync();

        return MapToDto(playlist);
    }

    public async Task<bool> UpdatePlaylistAsync(int id, CreatePlaylistDto dto)
    {
        var playlist = await _context.Playlists.FirstOrDefaultAsync(p => p.Id == id);
        if (playlist is null) return false;

        playlist.Name = dto.Name;
        playlist.Description = dto.Description;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeletePlaylistAsync(int id)
    {
        var playlist = await _context.Playlists.FirstOrDefaultAsync(p => p.Id == id);
        if (playlist is null) return false;

        _context.Playlists.Remove(playlist);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<PlaylistDto?> AddSongToPlaylistAsync(int playlistId, CreateSongDto dto)
    {
        var playlist = await _context.Playlists
            .Include(p => p.Songs)
            .FirstOrDefaultAsync(p => p.Id == playlistId);

        if (playlist is null) return null;

        var song = new Song
        {
            Title = dto.Title,
            Artist = dto.Artist,
            DurationInSeconds = dto.DurationInSeconds
        };

        playlist.Songs.Add(song);
        await _context.SaveChangesAsync();

        return MapToDto(playlist);
    }
}
