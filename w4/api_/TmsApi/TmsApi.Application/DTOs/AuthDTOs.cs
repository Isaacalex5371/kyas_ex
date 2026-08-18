namespace Tms.Api.Dtos;

public class AuthDTOs
{
public record LoginRequest(string Username, string Password);

public record UserProfileDto(string DisplayName, string Role);
}