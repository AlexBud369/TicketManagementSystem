namespace Presentation.Contracts.Users;

public sealed record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string Phone,
    string Address);
