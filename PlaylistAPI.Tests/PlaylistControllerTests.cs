using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlaylistAPI.Controllers;
using PlaylistAPI.Data;
using PlaylistAPI.DTOs;
using PlaylistAPI.Models;
using PlaylistAPI.Services;

namespace PlaylistAPI.Tests;

public class PlaylistControllerTests
{
    // Creates a fresh in-memory DB and wires up the real service for each test
    private (PlaylistController controller, PlaylistDbContext context) CreateSut()
    {
        var options = new DbContextOptionsBuilder<PlaylistDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new PlaylistDbContext(options);
        var service = new PlaylistService(context);
        var controller = new PlaylistController(service);

        return (controller, context);
    }

    [Fact]
    public async Task GetPlaylists_ReturnsEmptyList_WhenDatabaseIsEmpty()
    {
        var (controller, _) = CreateSut();

        var result = await controller.GetPlaylists();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedPlaylists = Assert.IsAssignableFrom<IEnumerable<PlaylistDto>>(okResult.Value);
        Assert.Empty(returnedPlaylists);
    }

    [Fact]
    public async Task CreatePlaylist_ReturnsCreated_WhenDataIsValid()
    {
        var (controller, _) = CreateSut();

        var dto = new CreatePlaylistDto
        {
            Name = "5-Team League Warmup",
            Description = "Music for the courts"
        };

        var result = await controller.CreatePlaylist(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedDto = Assert.IsType<PlaylistDto>(createdResult.Value);

        Assert.Equal("5-Team League Warmup", returnedDto.Name);
        Assert.Equal(1, returnedDto.Id);
    }

    [Fact]
    public async Task GetPlaylistById_ReturnsNotFound_WhenIdDoesNotExist()
    {
        var (controller, _) = CreateSut();

        var result = await controller.GetPlaylistById(999);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("That playlist does not exist.", notFoundResult.Value);
    }

    [Fact]
    public async Task DeletePlaylist_ReturnsNotFound_WhenIdDoesNotExist()
    {
        var (controller, _) = CreateSut();

        var result = await controller.DeletePlaylist(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AddSongToPlaylist_ReturnsUpdatedPlaylist_WhenPlaylistExists()
    {
        var (controller, _) = CreateSut();

        // Create a playlist first
        var created = await controller.CreatePlaylist(new CreatePlaylistDto { Name = "Test Playlist" });
        var createdResult = Assert.IsType<CreatedAtActionResult>(created.Result);
        var playlist = Assert.IsType<PlaylistDto>(createdResult.Value);

        // Add a song to it
        var songDto = new CreateSongDto { Title = "Track 1", Artist = "Artist A", DurationInSeconds = 180 };
        var result = await controller.AddSongToPlaylist(playlist.Id, songDto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var updatedPlaylist = Assert.IsType<PlaylistDto>(okResult.Value);

        Assert.Single(updatedPlaylist.Songs);
        Assert.Equal("Track 1", updatedPlaylist.Songs[0].Title);
    }
}
