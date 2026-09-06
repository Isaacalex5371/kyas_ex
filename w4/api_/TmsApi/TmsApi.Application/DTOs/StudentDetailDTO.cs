namespace TmsApi.Application.DTOs;

public record StudentDetailDTO
{
    public int Id { get; init; }
    public string RegistrationNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal GPA { get; init; }
    public bool IsActive { get; init; }
    public uint Version { get; init; }
    public required IReadOnlyList<LinkDto> Links { get; init; }
}