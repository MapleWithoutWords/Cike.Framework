namespace Cike.Auth;

public interface ICurrentUser
{
    Guid? Id { get; }

    string? UserName { get; }

    string? Name { get; }

    string? SurName { get; }

    string? PhoneNumber { get; }


    string? Email { get; }


    Guid? TenantId { get; }

    string[] Roles { get; }

    bool IsAuthorization { get; }
}
