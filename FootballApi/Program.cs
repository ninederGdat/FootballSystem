using System.Text.Json;
using System.Text.Json.Serialization;
using FootballApi.Configuration;
using FootballApi.Repositories.Competition;
using FootballApi.Repositories.Match;
using FootballApi.Repositories.MatchEvent;
using FootballApi.Repositories.Player;
using FootballApi.Services.Team;
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

builder.Services.AddControllers()
  .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<SeasonOptions>()
    .BindConfiguration(SeasonOptions.SectionName)
    .ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<SeasonOptions>, SeasonOptionsValidator>();


// --- Frontend CORS policy -------------------------------------------------
// Local Vite dev origins are always allowed. Deployed frontend origins come
// from configuration: Cors:AllowedOrigins (array). On Render set e.g.
//   Cors__AllowedOrigins__0 = https://<your-project>.pages.dev
const string FrontendCorsPolicy = "FrontendCorsPolicy";
var configuredOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];
var allowedOrigins = new[] { "http://localhost:5173", "http://127.0.0.1:5173" }
    .Concat(configuredOrigins.Select(o => o.TrimEnd('/')))
    .Distinct()
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    }
    );
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
builder.Services.AddScoped<ITeamService, TeamService>();
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


// --- Hosting (Render) -----------------------------------------------------
// Render injects the port to listen on via the PORT environment variable.
// Locally PORT is not set, so launchSettings.json / defaults still apply.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}
// ---------------------------------------------------------------------------



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

// On Render TLS is terminated at the proxy and the container only speaks HTTP,
// so redirecting to HTTPS here would have no port to redirect to.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Must run before UseAuthorization, and before MapControllers.
app.UseCors(FrontendCorsPolicy);

app.UseAuthorization();

app.MapControllers();
// Lightweight liveness endpoint for Render / uptime pings. Does not touch Supabase.
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
