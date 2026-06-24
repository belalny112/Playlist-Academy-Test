using Microsoft.AspNetCore.Mvc;
using PlaylistAPI.DTOs;
using PlaylistAPI.Services;

namespace PlaylistAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlaylistController : ControllerBase
{
    private readonly IPlaylistService _playlistService;

    public PlaylistController(IPlaylistService playlistService)
    {
        _playlistService = playlistService;
    }

    // GET /api/playlist
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlaylistDto>>> GetPlaylists()
    {
        var playlists = await _playlistService.GetAllPlaylistsAsync();
        return Ok(playlists);
    }

    // GET /api/playlist/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<PlaylistDto>> GetPlaylistById(int id)
    {
        var playlist = await _playlistService.GetPlaylistByIdAsync(id);
        if (playlist is null) return NotFound("That playlist does not exist.");
        return Ok(playlist);
    }

    // POST /api/playlist
    [HttpPost]
    public async Task<ActionResult<PlaylistDto>> CreatePlaylist([FromBody] CreatePlaylistDto dto)
    {
        var created = await _playlistService.CreatePlaylistAsync(dto);
        return CreatedAtAction(nameof(GetPlaylistById), new { id = created.Id }, created);
    }

    // PUT /api/playlist/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePlaylist(int id, [FromBody] CreatePlaylistDto dto)
    {
        var updated = await _playlistService.UpdatePlaylistAsync(id, dto);
        if (!updated) return NotFound();
        return NoContent();
    }

    // DELETE /api/playlist/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlaylist(int id)
    {
        var deleted = await _playlistService.DeletePlaylistAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    // POST /api/playlist/{playlistId}/song
    [HttpPost("{playlistId}/song")]
    public async Task<ActionResult<PlaylistDto>> AddSongToPlaylist(int playlistId, [FromBody] CreateSongDto dto)
    {
        var result = await _playlistService.AddSongToPlaylistAsync(playlistId, dto);
        if (result is null) return NotFound("That playlist does not exist.");
        return Ok(result);
    }
}
