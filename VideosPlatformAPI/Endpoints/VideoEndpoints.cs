using Carter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideosPlatformAPI.Data;
using VideosPlatformAPI.DTO;
using VideosPlatformAPI.Models;
using VideosPlatformAPI.Utility;

namespace VideosPlatformAPI.Endpoints;

public class VideoEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("videos");

        group.MapGet("", GetAllOrSearchVideos).WithName(nameof(GetAllOrSearchVideos));
        group.MapGet("{id:int}", GetVideoById).WithName(nameof(GetVideoById));
        group.MapPost("", CreateVideo).WithName(nameof(CreateVideo));
        group.MapPut("{id:int}", UpdateVideo).WithName(nameof(UpdateVideo));
        group.MapDelete("{id:int}", DeleteVideo).WithName(nameof(DeleteVideo));
    }

    private static async Task<IResult> GetAllOrSearchVideos(
        [FromQuery] string? search, 
        AppDbContext context, 
        VideoMappingService videoMappingService,
        [FromQuery] int page = 1)
    {
        try
        {
            const int pageSize = 5;
            var skip = (page - 1) * pageSize;

            IQueryable<Video> query = context.Videos.Include(v => v.Category);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(v => v.Title.ToUpper().Contains(search.ToUpper()));
            }

            var totalCount = await query.CountAsync();

            var videos = await query
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            if (!videos.Any())
            {
                return Results.NotFound("No videos found.");
            }

            var hasMorePages = (skip + videos.Count) < totalCount;

            var videoDtos = videos.Select(videoMappingService.ConvertToVideoResponseDto).ToList();

            var response = new PagedResponseDTO<VideoResponseDTO>
            (
                CurrentPage: page,
                HasMorePages: hasMorePages,
                Items: videoDtos
            );

            return Results.Ok(response);
        }
        catch (Exception)
        {
            return Results.Problem("An error occurred while retrieving the videos.");
        }
    }

    private static async Task<IResult> GetVideoById(
        int id, 
        AppDbContext context, 
        VideoMappingService videoMappingService)
    {
        try
        {
            var video = await context.Videos
                .Include(v => v.Category)
                .FirstOrDefaultAsync(v => v.Id == id);

            return video == null 
                ? Results.NotFound($"Video with Id = {id} not found.") 
                : Results.Ok(videoMappingService.ConvertToVideoResponseDto(video));
        }
        catch (Exception)
        {
            return Results.Problem("An error occurred while retrieving the video.");
        }
    }

    private static async Task<IResult> CreateVideo(
        VideoDTO videoDto, 
        AppDbContext context, 
        VideoMappingService videoMappingService)
    {
        try
        {
            if (videoDto is null)
            {
                return Results.BadRequest("Video data is null");
            }

            var videoToAdd = new Video
            {
                Title = videoDto.Title,
                Description = videoDto.Description,
                Url = videoDto.Url
            };

            if (!string.IsNullOrEmpty(videoDto.CategoryName))
            {
                var category = await context.Categories
                    .FirstOrDefaultAsync(c => c.Title.ToUpper().Equals(videoDto.CategoryName.ToUpper()));

                if (category == null)
                {
                    return Results.BadRequest("Category does not exist");
                }

                videoToAdd.Category = category;
            }
            else
            {
                var defaultCategory = await context.Categories.FindAsync(1);
                if (defaultCategory == null)
                {
                    return Results.BadRequest("Default category not found");
                }

                videoToAdd.Category = defaultCategory;
            }

            context.Videos.Add(videoToAdd);
            await context.SaveChangesAsync();

            var videoResponseDto = videoMappingService.ConvertToVideoResponseDto(videoToAdd);

            return Results.Created($"/videos/{videoResponseDto.id}", videoResponseDto);
        }
        catch (Exception)
        {
            return Results.Problem("An error occurred while saving the video.");
        }
    }

    private static async Task<IResult> UpdateVideo(
        int id, 
        VideoDTO videoDto, 
        AppDbContext context, 
        VideoMappingService videoMappingService)
    {
        try
        {
            var videoToUpdate = await context.Videos
                .Include(v => v.Category)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (videoToUpdate == null)
            {
                return Results.NotFound($"Video with Id = {id} not found.");
            }

            videoToUpdate.Title = !string.IsNullOrEmpty(videoDto.Title) ? videoDto.Title : videoToUpdate.Title;
            videoToUpdate.Description = !string.IsNullOrEmpty(videoDto.Description) ? videoDto.Description : videoToUpdate.Description;
            videoToUpdate.Url = !string.IsNullOrEmpty(videoDto.Url) ? videoDto.Url : videoToUpdate.Url;

            if (!string.IsNullOrEmpty(videoDto.CategoryName))
            {
                var category = await context.Categories
                    .FirstOrDefaultAsync(c => c.Title.ToUpper().Equals(videoDto.CategoryName.ToUpper()));

                if (category != null)
                {
                    videoToUpdate.Category = category;
                }
            }

            await context.SaveChangesAsync();

            var updatedVideo = await context.Videos
                .Include(v => v.Category)
                .FirstOrDefaultAsync(v => v.Id == id);

            return Results.Ok(videoMappingService.ConvertToVideoResponseDto(updatedVideo));
        }
        catch (Exception)
        {
            return Results.Problem("An error occurred while updating the video.");
        }
    }

    private static async Task<IResult> DeleteVideo(
        int id, 
        AppDbContext context, 
        VideoMappingService videoMappingService)
    {
        try
        {
            var videoToDelete = await context.Videos
                .Include(v => v.Category)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (videoToDelete == null)
            {
                return Results.NotFound($"Video with Id = {id} not found.");
            }

            var deletedVideoDto = videoMappingService.ConvertToVideoResponseDto(videoToDelete);

            context.Videos.Remove(videoToDelete);
            await context.SaveChangesAsync();

            return Results.Ok(deletedVideoDto);
        }
        catch (Exception)
        {
            return Results.Problem("An error occurred while deleting the video.");
        }
    }
}
