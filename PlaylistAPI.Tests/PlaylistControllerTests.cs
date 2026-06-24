using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlaylistAPI.Controllers;
using PlaylistAPI.Data;
using PlaylistAPI.DTOs;
using PlaylistAPI.Models;

namespace PlaylistAPI.Tests;

public class PlaylistControllerTests
{
    // --- DATABASE SETUP ---
    // Creates a fresh, empty fake database in RAM for every single test
    private PlaylistDbContext GetDatabaseContext()
    {
        var options = new DbContextOptionsBuilder<PlaylistDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        return new PlaylistDbContext(options);
    }

    // --- TEST 1: THE EMPTY ROOM ---
    [Fact] 
    public void GetPlaylists_ReturnsEmptyList_WhenDatabaseIsEmpty()
    {
        // ARRANGE
        var context = GetDatabaseContext();
        var controller = new PlaylistController(context);

        // ACT
        var result = controller.GetPlaylists();

        // ASSERT
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedPlaylists = Assert.IsAssignableFrom<IEnumerable<PlaylistDto>>(okResult.Value);
        Assert.Empty(returnedPlaylists); 
    }

    // --- TEST 2: THE HAPPY PATH ---
    [Fact]
    public void CreatePlaylist_ReturnsCreated_WhenDataIsValid()
    {
        // ARRANGE
        var context = GetDatabaseContext();
        var controller = new PlaylistController(context);
        
        var newPlaylistDto = new CreatePlaylistDto
        {
            Name = "5-Team League Warmup",
            Description = "Music for the courts"
        };

        // ACT
        var result = controller.CreatePlaylist(newPlaylistDto);

        // ASSERT
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedDto = Assert.IsType<PlaylistDto>(createdResult.Value);
        
        Assert.Equal("5-Team League Warmup", returnedDto.Name);
        Assert.Equal(1, returnedDto.Id); 
    }

    // --- TEST 3: THE SAD PATH ---
    [Fact]
    public void GetPlaylistById_ReturnsNotFound_WhenUserSendsStupidId()
    {
        // ARRANGE
        var context = GetDatabaseContext();
        var controller = new PlaylistController(context);

        // ACT
        var result = controller.GetPlaylistById(999);

        // ASSERT
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("That playlist does not exist.", notFoundResult.Value);
    }
}