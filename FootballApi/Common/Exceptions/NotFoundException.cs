// Common/Exceptions/NotFoundException.cs
namespace FootballApi.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}