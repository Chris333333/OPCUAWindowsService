namespace Data.DTOs
{
    public record ErrorLogDTO(bool IsError, string? Message = null);
}
