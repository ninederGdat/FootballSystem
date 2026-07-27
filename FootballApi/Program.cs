using FootballApi.Repositories.Match;
using FootballApi.Repositories.Player;
using FootballApi.Repositories.Position;
using FootballApi.Repositories.PositionRole;
using FootballApi.Repositories.Team;
using FootballApi.Services;
using FootballApi.Services.Player;
using FootballSystem.Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Đăng ký Supabase client
builder.Services.AddSingleton<SupabaseClientFactory>();

builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<ILineupRepository, LineupRepository>();
builder.Services.AddScoped<ILineupService, LineupService>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IPositionRepository, PositionRepository>();
builder.Services.AddScoped<IPositionRoleRepository, PositionRoleRepository>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();


app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
