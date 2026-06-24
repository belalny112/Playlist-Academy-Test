using PlaylistAPI.DTOs;

namespace PlaylistAPI.Services;

public interface IPlaylistService
{
    Task<IEnumerable<PlaylistDto>> GetAllPlaylistsAsync();
    Task<PlaylistDto?> GetPlaylistByIdAsync(int id);
    Task<PlaylistDto> CreatePlaylistAsync(CreatePlaylistDto dto);
    Task<bool> UpdatePlaylistAsync(int id, CreatePlaylistDto dto);
    Task<bool> DeletePlaylistAsync(int id);
    Task<PlaylistDto?> AddSongToPlaylistAsync(int playlistId, CreateSongDto dto);
}
