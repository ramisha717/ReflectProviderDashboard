namespace Reflect.Domain.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string EntraObjectId { get; }
    string Email { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}