using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using PlaylistAPI.DTOs;
using PlaylistAPI.Data; 

namespace PlaylistAPI.Tests;

public class PlaylistIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PlaylistIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // 1. Find and remove the original database configuration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PlaylistDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                // 2. Keep using SQLite, but point it to a completely different, temporary test file
                services.AddDbContext<PlaylistDbContext>(options =>
                {
                    options.UseSqlite("Data Source=integration_test.db");
                });

                // 3. Force the server to physically build the new test database file
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PlaylistDbContext>();
                
                // Wipe it clean and rebuild it so tests always start with a fresh slate
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            });
        });

        _client = customFactory.CreateClient();
    }
    
    

    [Fact]
    public async Task CreatePlaylist_ReturnsBadRequest_WhenNameIsMissing()
    {
        var badPlaylistDto = new CreatePlaylistDto { Name = "", Description = "No name!" };
        var response = await _client.PostAsJsonAsync("/api/playlist", badPlaylistDto);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddSong_ReturnsBadRequest_WhenDurationIsImpossible()
    {
        // 1. Create a valid playlist first so we have somewhere to put the song
        var validPlaylist = new CreatePlaylistDto { Name = "Valid List" };
        var playlistResponse = await _client.PostAsJsonAsync("/api/playlist", validPlaylist);
        var createdPlaylist = await playlistResponse.Content.ReadFromJsonAsync<PlaylistDto>();

        // 2. Try to add a song with a negative duration to that new playlist
        var badSongDto = new CreateSongDto 
        { 
            Title = "Glitched Track", 
            Artist = "Unknown", 
            DurationInSeconds = -50 // Violates the [Range(1, 3600)] rule!
        };

        var response = await _client.PostAsJsonAsync($"/api/playlist/{createdPlaylist.Id}/song", badSongDto);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // --- 2. THE HAPPY PATHS (End-to-End User Journeys) ---

    [Fact]
    public async Task EndToEnd_CreateAndGetPlaylist()
    {
        // 1. User creates a playlist
        var newPlaylist = new CreatePlaylistDto { Name = "Clair Obscur OST", Description = "Expedition 33 Tracks" };
        var createResponse = await _client.PostAsJsonAsync("/api/playlist", newPlaylist);
        createResponse.EnsureSuccessStatusCode();
        var createdDto = await createResponse.Content.ReadFromJsonAsync<PlaylistDto>();

        // 2. User immediately fetches that specific playlist
        var getResponse = await _client.GetAsync($"/api/playlist/{createdDto.Id}");
        getResponse.EnsureSuccessStatusCode();
        var fetchedDto = await getResponse.Content.ReadFromJsonAsync<PlaylistDto>();

        // 3. Verify the database remembered it correctly
        Assert.Equal("Clair Obscur OST", fetchedDto.Name);
    }

    [Fact]
    public async Task EndToEnd_CreateUpdateAndVerifyPlaylist()
    {
        // 1. Create it (Added Description to satisfy the Non-Null database rule!)
        var originalPlaylist = new CreatePlaylistDto { Name = "Old Name", Description = "Temp Description" };
        var createResponse = await _client.PostAsJsonAsync("/api/playlist", originalPlaylist);
        
        // This line guarantees we stop immediately if the creation fails
        createResponse.EnsureSuccessStatusCode(); 
        
        var createdDto = await createResponse.Content.ReadFromJsonAsync<PlaylistDto>();

        // 2. Update it (PUT Request)
        var updatedPlaylist = new CreatePlaylistDto { Name = "Tournament Anthems", Description = "5-Team League" };
        var updateResponse = await _client.PutAsJsonAsync($"/api/playlist/{createdDto.Id}", updatedPlaylist);
        updateResponse.EnsureSuccessStatusCode();

        // 3. Fetch it to prove the database actually overwrote the old name
        var getResponse = await _client.GetAsync($"/api/playlist/{createdDto.Id}");
        var fetchedDto = await getResponse.Content.ReadFromJsonAsync<PlaylistDto>();
        
        Assert.Equal("Tournament Anthems", fetchedDto.Name);
    }

    [Fact]
    public async Task EndToEnd_CreateAndDeletePlaylist()
    {
        // 1. Create it (Added Description here too)
        var tempPlaylist = new CreatePlaylistDto { Name = "Delete Me", Description = "Temp Description" };
        var createResponse = await _client.PostAsJsonAsync("/api/playlist", tempPlaylist);
        createResponse.EnsureSuccessStatusCode();
        
        var createdDto = await createResponse.Content.ReadFromJsonAsync<PlaylistDto>();

        // 2. Delete it (DELETE Request)
        var deleteResponse = await _client.DeleteAsync($"/api/playlist/{createdDto.Id}");
        deleteResponse.EnsureSuccessStatusCode();

        // 3. Try to fetch it again (Should return 404 Not Found)
        var getResponse = await _client.GetAsync($"/api/playlist/{createdDto.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

}