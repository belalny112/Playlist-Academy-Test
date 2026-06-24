using Microsoft.EntityFrameworkCore;
using PlaylistAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// --- 1. ADD SERVICES (The Tools) ---

builder.Services.AddControllers(); 

builder.Services.AddDbContext<PlaylistDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ADDED: The tool that reads your code and builds the Swagger blueprint
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- 2. CONFIGURE PIPELINE (The Traffic Flow) ---

// ADDED: Turn on the Swagger JSON blueprint and the visual Web UI
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers(); 

app.Run();
public partial class Program { }