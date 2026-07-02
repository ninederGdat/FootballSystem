namespace FotmobSync.Infrastructure.External.Fotmob.Mapping;

public class FotmobPositionMapper : IFotmobPositionMapper
{
    public bool TryMap(int positionId, out string positionCode)
    {
        return FotmobPositionMappings.PositionCodes
            .TryGetValue(positionId, out positionCode!);
    }
}