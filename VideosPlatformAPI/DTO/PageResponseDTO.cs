using System.Collections;

namespace VideosPlatformAPI.DTO;

public record PagedResponseDTO<T>(int CurrentPage, bool HasMorePages, IEnumerable<T> Items);