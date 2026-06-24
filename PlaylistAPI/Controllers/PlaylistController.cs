using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlaylistAPI.Models;
using PlaylistAPI.Data;
using PlaylistAPI.DTOs; // Important: Gives us access to the DTOs

namespace PlaylistAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlaylistController : ControllerBase
{
    private readonly PlaylistDbContext _context;

    public PlaylistController(PlaylistDbContext context)
    {
        _context = context;
    }

    // 1. GET ALL
    [HttpGet]
    public ActionResult<IEnumerable<PlaylistDto>> GetPlaylists()
    {
        var dbPlaylists = _context.Playlists.Include(p => p.Songs).ToList();

        var playlistDtos = dbPlaylists.Select(p => new PlaylistDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Songs = p.Songs.Select(s => new SongDto
            {
                Id = s.Id,
                Title = s.Title,
                Artist = s.Artist,
                DurationInSeconds = s.DurationInSeconds
            }).ToList()
        }).ToList();

        return Ok(playlistDtos);
    }

    // 2. GET BY ID
    [HttpGet("{id}")]
    public ActionResult<PlaylistDto> GetPlaylistById(int id)
    {
        var playlist = _context.Playlists.Include(p => p.Songs).FirstOrDefault(p => p.Id == id);
        if (playlist == null) return NotFound("That playlist does not exist.");

        var playlistDto = new PlaylistDto
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

        return Ok(playlistDto);
    }

    // 3. CREATE PLAYLIST
    [HttpPost]
    public ActionResult<PlaylistDto> CreatePlaylist([FromBody] CreatePlaylistDto newPlaylistDto)
    {
        var playlistDbModel = new Playlist
        {
            Name = newPlaylistDto.Name,
            Description = newPlaylistDto.Description
        };

        _context.Playlists.Add(playlistDbModel);
        _context.SaveChanges();

        var returnDto = new PlaylistDto
        {
            Id = playlistDbModel.Id,
            Name = playlistDbModel.Name,
            Description = playlistDbModel.Description,
            Songs = new List<SongDto>()
        };

        return CreatedAtAction(nameof(GetPlaylistById), new { id = returnDto.Id }, returnDto);
    }

    // 4. UPDATE PLAYLIST
    [HttpPut("{id}")]
    public IActionResult UpdatePlaylist(int id, [FromBody] CreatePlaylistDto updatedPlaylistDto)
    {
        var existingPlaylist = _context.Playlists.FirstOrDefault(p => p.Id == id);
        if (existingPlaylist == null) return NotFound();

        existingPlaylist.Name = updatedPlaylistDto.Name;
        existingPlaylist.Description = updatedPlaylistDto.Description;

        _context.SaveChanges();
        return NoContent();
    }

    // 5. DELETE PLAYLIST
    [HttpDelete("{id}")]
    public IActionResult DeletePlaylist(int id)
    {
        var playlist = _context.Playlists.FirstOrDefault(p => p.Id == id);
        if (playlist == null) return NotFound();

        _context.Playlists.Remove(playlist);
        _context.SaveChanges();
        return NoContent();
    }

    // 6. ADD SONG TO PLAYLIST
    [HttpPost("{playlistId}/song")]
    public ActionResult<PlaylistDto> AddSongToPlaylist(int playlistId, [FromBody] CreateSongDto newSongDto)
    {
        var playlist = _context.Playlists.Include(p => p.Songs).FirstOrDefault(p => p.Id == playlistId);
        if (playlist == null) return NotFound("That playlist does not exist.");

        var songDbModel = new Song
        {
            Title = newSongDto.Title,
            Artist = newSongDto.Artist,
            DurationInSeconds = newSongDto.DurationInSeconds
        };

        playlist.Songs.Add(songDbModel);
        _context.SaveChanges();

        var returnDto = new PlaylistDto
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

        return Ok(returnDto);
    }
}