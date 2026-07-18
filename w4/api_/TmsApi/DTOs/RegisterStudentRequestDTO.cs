using System.ComponentModel.DataAnnotations;

namespace TmsApi.DTOs;

public record RegisterStudentRequest
{
    [Required]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public required string Name { get; init; }

    [Required]
    [Range(0.0, 4.0, ErrorMessage = "GPA must be between 0.0 and 4.0")]
    public required decimal GPA { get; init; }
};

public record UpdateStudentRequest(string? Name, decimal? GPA, bool? IsActive, uint Version);