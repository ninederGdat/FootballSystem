using FootballApi.Configuration;
using FootballApi.Repositories.Competition;
using FootballApi.Repositories.Match;
using FootballApi.Repositories.MatchEvent;
using FootballApi.Repositories.Player;
using FootballApi.Repositories.Position;
using FootballApi.Repositories.PositionRole;
using FootballApi.Repositories.Team;
using FootballApi.Repositories.Transfer;
using FootballApi.Services;
using FootballApi.Services.Competition;
using FootballApi.Services.MatchEvent;
using FootballApi.Services.Player;
using FootballApi.Services.Season;
using FootballApi.Services.Transfer;
using FootballSystem.Shared.Infrastructure;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<SeasonOptions>()
    .BindConfiguration(SeasonOptions.SectionName)
    .ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<SeasonOptions>, SeasonOptionsValidator>();


// --- Frontend CORS policy -------------------------------------------------
// Vite dev server default is http://localhost:5173. Add any other origins
// (e.g. a deployed frontend URL) to this list as needed.
const string FrontendCorsPolicy = "FrontendCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
// ---------------------------------------------------------------------------

// Đăng ký Supabase client
builder.Services.AddSingleton<SupabaseClientFactory>();

builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<ICompetitionRepository, CompetitionRepository>();
builder.Services.AddScoped<ICompetitionService, CompetitionService>();
builder.Services.AddScoped<ISeasonService, SeasonService>();
builder.Services.AddScoped<ILineupRepository, LineupRepository>();
builder.Services.AddScoped<ILineupService, LineupService>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IPositionRepository, PositionRepository>();
builder.Services.AddScoped<IPositionRoleRepository, PositionRoleRepository>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IMatchEventRepository, MatchEventRepository>();
builder.Services.AddScoped<IMatchEventService, MatchEventService>();
builder.Services.AddScoped<ITransferRepository, TransferRepository>();
builder.Services.AddScoped<ITransferService, TransferService>();
builder.Services.AddScoped<ISeasonService, SeasonService>();
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

// Must run before UseAuthorization, and before MapControllers.
app.UseCors(FrontendCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();
