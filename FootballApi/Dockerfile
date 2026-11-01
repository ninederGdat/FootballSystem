# Build context must be the repository root (FootballApi references FootballSystem.Shared):
#   docker build -f FootballApi/Dockerfile -t football-api .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only project files first so the restore layer is cached.
COPY FootballSystem.Shared/FootballSystem.Shared.csproj FootballSystem.Shared/
COPY FootballApi/FootballApi.csproj FootballApi/
RUN dotnet restore FootballApi/FootballApi.csproj

COPY FootballSystem.Shared/ FootballSystem.Shared/
COPY FootballApi/ FootballApi/
RUN dotnet publish FootballApi/FootballApi.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=build /app/publish .

# Non-root user provided by the official image. Render sets PORT at runtime;
# Program.cs binds to it (falls back to the image default 8080 otherwise).
USER $APP_UID
ENTRYPOINT ["dotnet", "FootballApi.dll"]