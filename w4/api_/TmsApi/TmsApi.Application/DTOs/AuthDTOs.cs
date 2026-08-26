namespace Tms.Api.Dtos;

public class AuthDTOs
{
public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Role);

public record LoginRequest(
    string Email,
    string Password);
public record RefreshRequest(string RefreshToken);
public record UserProfileDto(string DisplayName, string Role);
}