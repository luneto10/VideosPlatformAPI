using VideosPlatformAPI.DTO;
using VideosPlatformAPI.Models;

namespace VideosPlatformAPI.Utility;

public class VideoMappingService
{
    public VideoResponseDTO ConvertToVideoResponseDto(Video video)
    {
        return new VideoResponseDTO
        (
            id: video.Id,
            Title: video.Title,
            Description: video.Description,
            Url: video.Url,
            Category: new CategoryResponseDTO
            (
                Id: video.Category.Id,
                Title: video.Category.Title,
                Color: video.Category.Color
            )
        );
    }
}