namespace VideosPlatformAPI.DTO;

public record VideoResponseDTO (int id, string Title, string Description, string Url, CategoryDTO Category);