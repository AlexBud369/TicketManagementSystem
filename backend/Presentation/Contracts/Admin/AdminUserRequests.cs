namespace Presentation.Contracts.Admin;

public sealed record UpdateUserByAdminRequest(
    string FirstName,
    string LastName,
    string Phone,
    string Address);

public sealed record ChangeUserRoleRequest(string NewRole);
