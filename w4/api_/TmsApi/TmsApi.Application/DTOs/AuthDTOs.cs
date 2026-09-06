namespace TmsApi.Application.DTOs;

public class AuthDtOs
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
public record LoginRequest2(string Email, string Password);
public record RefreshRequest(string RefreshToken);
public record UserProfileDto(string Id, string Email, string FullName, string Role);
}