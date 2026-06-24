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
    private readonly WebApplicationFactory<Program> _factory;

    public PlaylistIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    // Each test gets its own isolated SQLite file — prevents parallel test interference
    private HttpClient CreateIsolatedClient()
    {
        var dbName = $"integration_{Guid.NewGuid()}.db";

        var customFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<PlaylistDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<PlaylistDbContext>(options =>
                {
                    options.UseSqlite($"Data Source={dbName}");
                });

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PlaylistDbContext>();

                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            });
        });

        return customFactory.CreateClient();
    }

    [Fact]
    public async Task CreatePlaylist_ReturnsBadRequest_WhenNameIsMissing()
    {
        var client = CreateIsolatedClient();
        var badDto = new CreatePlaylistDto { Name = "", Description = "No name!" };
        var response = await client.PostAsJsonAsync("/api/playlist", badDto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddSong_ReturnsBadRequest_WhenDurationIsInvalid()
    {
        var client = CreateIsolatedClient();

        var validPlaylist = new CreatePlaylistDto { Name = "Valid List" };
        var playlistResponse = await client.PostAsJsonAsync("/api/playlist", validPlaylist);
        var createdPlaylist = await playlistResponse.Content.ReadFromJsonAsync<PlaylistDto>();

        var badSong = new CreateSongDto
        {
            Title = "Glitched Track",
            Artist = "Unknown",
            DurationInSeconds = -50 // Violates [Range(1, 3600)]
        };

        var response = await client.PostAsJsonAsync($"/api/playlist/{createdPlaylist!.Id}/song", badSong);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EndToEnd_CreateAndGetPlaylist()
    {
        var client = CreateIsolatedClient();

        var newPlaylist = new CreatePlaylistDto { Name = "Clair Obscur OST", Description = "Expedition 33 Tracks" };
        var createResponse = await client.PostAsJsonAsync("/api/playlist", newPlaylist);
        createResponse.EnsureSuccessStatusCode();

        var createdDto = await createResponse.Content.ReadFromJsonAsync<PlaylistDto>();

        var getResponse = await client.GetAsync($"/api/playlist/{createdDto!.Id}");
        getResponse.EnsureSuccessStatusCode();

        var fetchedDto = await getResponse.Content.ReadFromJsonAsync<PlaylistDto>();
        Assert.Equal("Clair Obscur OST", fetchedDto!.Name);
    }

    [Fact]
    public async Task EndToEnd_CreateUpdateAndVerifyPlaylist()
    {
        var client = CreateIsolatedClient();

        var createResponse = await client.PostAsJsonAsync("/api/playlist",
            new CreatePlaylistDto { Name = "Old Name", Description = "Temp" });
        createResponse.EnsureSuccessStatusCode();

        var createdDto = await createResponse.Content.ReadFromJsonAsync<PlaylistDto>();

        var updateResponse = await client.PutAsJsonAsync($"/api/playlist/{createdDto!.Id}",
            new CreatePlaylistDto { Name = "Tournament Anthems", Description = "5-Team League" });
        updateResponse.EnsureSuccessStatusCode();

        var getResponse = await client.GetAsync($"/api/playlist/{createdDto.Id}");
        var fetchedDto = await getResponse.Content.ReadFromJsonAsync<PlaylistDto>();

        Assert.Equal("Tournament Anthems", fetchedDto!.Name);
    }

    [Fact]
    public async Task EndToEnd_CreateAndDeletePlaylist()
    {
        var client = CreateIsolatedClient();

        var createResponse = await client.PostAsJsonAsync("/api/playlist",
            new CreatePlaylistDto { Name = "Delete Me", Description = "Temp" });
        createResponse.EnsureSuccessStatusCode();

        var createdDto = await createResponse.Content.ReadFromJsonAsync<PlaylistDto>();

        var deleteResponse = await client.DeleteAsync($"/api/playlist/{createdDto!.Id}");
        deleteResponse.EnsureSuccessStatusCode();

        var getResponse = await client.GetAsync($"/api/playlist/{createdDto.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task EndToEnd_AddSongAndVerifyInPlaylist()
    {
        var client = CreateIsolatedClient();

        var createResponse = await client.PostAsJsonAsync("/api/playlist",
            new CreatePlaylistDto { Name = "My Playlist" });
        createResponse.EnsureSuccessStatusCode();
        var playlist = await createResponse.Content.ReadFromJsonAsync<PlaylistDto>();

        var songResponse = await client.PostAsJsonAsync($"/api/playlist/{playlist!.Id}/song",
            new CreateSongDto { Title = "Bohemian Rhapsody", Artist = "Queen", DurationInSeconds = 354 });
        songResponse.EnsureSuccessStatusCode();

        var getResponse = await client.GetAsync($"/api/playlist/{playlist.Id}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<PlaylistDto>();

        Assert.Single(fetched!.Songs);
        Assert.Equal("Bohemian Rhapsody", fetched.Songs[0].Title);
    }
}
