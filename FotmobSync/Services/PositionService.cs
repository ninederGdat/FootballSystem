using FootballSystem.Shared.Infrastructure;
using FootballSystem.Shared.Models.Clean;
using FotmobSync.Mappers;
using FotmobSync.Models.Raw;
using Microsoft.Extensions.Logging;

namespace FotmobSync.Services
{
    public class PositionService
    {
        private readonly Supabase.Client _supabase;
        private readonly ILogger<PositionService> _logger;

        public PositionService(SupabaseClientFactory factory, ILogger<PositionService> logger)
        {
            _supabase = factory.CreateServiceRoleClient();
            _logger = logger;
        }

        /// <summary>
        /// Upsert Position và trả về PositionCode
        /// </summary>
        public async Task<string?> UpsertPositionAsync(PositionDescriptionRaw? positionDescription)
        {
            try
            {
                if (positionDescription?.PrimaryPosition == null)
                {
                    _logger.LogWarning("PositionDescription or PrimaryPosition is null");
                    return null;
                }

                var rawKey = positionDescription.PrimaryPosition.Key;
                var rawLabel = positionDescription.PrimaryPosition.Label;

                _logger.LogDebug("Raw position from FotMob: Key='{Key}', Label='{Label}'", rawKey, rawLabel);

                var positionCode = PositionMapper.ToPositionCode(rawKey);
                var positionName = PositionMapper.ToPositionName(rawLabel) ?? rawLabel ?? rawKey;

                if (string.IsNullOrWhiteSpace(positionCode))
                {
                    _logger.LogWarning("Cannot map position key: {Key}", rawKey);
                    return null;
                }

                var position = new PositionClean
                {
                    PositionCode = positionCode,
                    PositionName = positionName,
                    XCoord = 0.5,
                    YCoord = 0.5,
                    IsGoalkeeper = positionCode == "GK"
                };

                await _supabase
                    .From<PositionClean>()
                    .Upsert(position, new() { OnConflict = "position_code" });

                _logger.LogInformation("✅ Position upserted successfully: {Code} - {Name}", positionCode, positionName);

                await UpsertPositionRoleAsync(positionCode, rawLabel ?? positionName, rawKey!);
                return positionCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi upsert position. Raw Key: {Key}",
                    positionDescription?.PrimaryPosition?.Key);
                return null;
            }
        }
        private async Task UpsertPositionRoleAsync(string positionCode, string roleName, string fotmobKey)
        {
            try
            {
                var role = new PositionRoleClean
                {
                    PositionCode = positionCode,
                    RoleName = roleName,
                    RoleShort = fotmobKey,
                    IsPremium = false
                };

                await _supabase
                    .From<PositionRoleClean>()
                    .Upsert(role, new() { OnConflict = "position_code, role_name" });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không upsert được PositionRole cho {Code}", (object)positionCode);
            }
        }
    }
}
