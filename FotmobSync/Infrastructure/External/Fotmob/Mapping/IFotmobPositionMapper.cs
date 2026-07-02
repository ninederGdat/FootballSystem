namespace FotmobSync.Infrastructure.External.Fotmob.Mapping;

public interface IFotmobPositionMapper
{
    bool TryMap(int positionId, out string positionCode);
}