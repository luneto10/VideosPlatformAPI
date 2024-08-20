using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideosPlatformAPI.Data;
using VideosPlatformAPI.DTO;
using VideosPlatformAPI.Models;

namespace VideosPlatformAPI.Controllers;

[ApiController]
[Route("videos")]
public class VideoController : ControllerBase
{
    private readonly AppDbContext _context;

    public VideoController(AppDbContext context)
    {
        _context = context;
    }

    // Helper method to convert a Video entity to a VideoResponseDTO
    private VideoResponseDTO ConvertToVideoResponseDto(Video video)
    {
        return new VideoResponseDTO
        (
            id: video.Id,
            Title: video.Title,
            Description: video.Description,
            Url: video.Url,
            Category: new CategoryResponseDTO
            (
                Id : video.Category.Id,
                Title: video.Category.Title,
                Color: video.Category.Color
            )
        );
    }

    // GET: /videos
    [HttpGet(Name = "GetAllVideos")]
    public async Task<ActionResult<IEnumerable<VideoResponseDTO>>> Get()
    {
        try
        {
            var videos = await _context.Videos
                .Include(v => v.Category)  // Include the Category for each Video
                .ToListAsync();

            var videoDtos = videos.Select(ConvertToVideoResponseDto).ToList();
            return Ok(videoDtos);
        }
        catch (Exception)
        {
            return Problem("An error occurred while retrieving the videos.");
        }
    }

    // GET: /videos/{id}
    [HttpGet("{id:int}", Name = "GetVideoById")]
    public async Task<ActionResult<VideoResponseDTO>> Get(int id)
    {
        try
        {
            var video = await _context.Videos
                .Include(v => v.Category) // Include Category to ensure it's loaded
                .FirstOrDefaultAsync(v => v.Id == id);

            if (video == null)
            {
                return NotFound($"Video with Id = {id} not found.");
            }

            return Ok(ConvertToVideoResponseDto(video));
        }
        catch (Exception)
        {
            return Problem("An error occurred while retrieving the video.");
        }
    }

    // POST: /videos
    [HttpPost(Name = "CreateVideo")]
    public async Task<ActionResult<VideoResponseDTO>> Post([FromBody] VideoDTO videoDto)
    {
        try
        {
            if (videoDto is null)
            {
                return BadRequest("Video data is null");
            }

            var videoToAdd = new Video
            {
                Title = videoDto.Title,
                Description = videoDto.Description,
                Url = videoDto.Url
            };

            if (!string.IsNullOrEmpty(videoDto.CategoryName))
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Title.ToUpper().Equals(videoDto.CategoryName.ToUpper()));

                if (category == null)
                {
                    return BadRequest("Category does not exist");
                }

                videoToAdd.Category = category;
            }
            else
            {
                var defaultCategory = await _context.Categories.FindAsync(1);
                if (defaultCategory == null)
                {
                    return BadRequest("Default category not found");
                }

                videoToAdd.Category = defaultCategory;
            }

            if (!TryValidateModel(videoToAdd))
            {
                return BadRequest(ModelState);
            }

            _context.Videos.Add(videoToAdd);
            await _context.SaveChangesAsync();

            var videoResponseDto = ConvertToVideoResponseDto(videoToAdd);

            return CreatedAtRoute("GetVideoById", new { id = videoResponseDto.id }, videoResponseDto);
        }
        catch (Exception)
        {
            return Problem("An error occurred while saving the video.");
        }
    }

    // PUT: /videos/{id}
    [HttpPut("{id:int}", Name = "UpdateVideo")]
    public async Task<ActionResult<VideoResponseDTO>> Update(int id, [FromBody] VideoDTO videoDto)
    {
        try
        {
            if (videoDto == null)
            {
                return BadRequest("Video data is null");
            }

            var videoToUpdate = await _context.Videos
                .Include(v => v.Category) // Ensure category is included for updating
                .FirstOrDefaultAsync(v => v.Id == id);

            if (videoToUpdate == null)
            {
                return NotFound($"Video with Id = {id} not found.");
            }

            videoToUpdate.Title = !string.IsNullOrEmpty(videoDto.Title) ? videoDto.Title : videoToUpdate.Title;
            videoToUpdate.Description = !string.IsNullOrEmpty(videoDto.Description) ? videoDto.Description : videoToUpdate.Description;
            videoToUpdate.Url = !string.IsNullOrEmpty(videoDto.Url) ? videoDto.Url : videoToUpdate.Url;

            if (!string.IsNullOrEmpty(videoDto.CategoryName))
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Title.ToUpper().Equals(videoDto.CategoryName.ToUpper()));

                if (category != null)
                {
                    videoToUpdate.Category = category;
                }
            }

            if (!TryValidateModel(videoToUpdate))
            {
                return BadRequest(ModelState);
            }

            await _context.SaveChangesAsync();

            var updatedVideo = await _context.Videos
                .Include(v => v.Category)
                .FirstOrDefaultAsync(v => v.Id == id);

            return Ok(ConvertToVideoResponseDto(updatedVideo));
        }
        catch (Exception)
        {
            return Problem("An error occurred while updating the video.");
        }
    }

    // DELETE: /videos/{id}
    [HttpDelete("{id:int}", Name = "DeleteVideo")]
    public async Task<ActionResult<VideoResponseDTO>> Delete(int id)
    {
        try
        {
            var videoToDelete = await _context.Videos
                .Include(v => v.Category) // Ensure category is included for return
                .FirstOrDefaultAsync(v => v.Id == id);

            if (videoToDelete == null)
            {
                return NotFound($"Video with Id = {id} not found.");
            }

            var deletedVideoDto = ConvertToVideoResponseDto(videoToDelete);

            _context.Videos.Remove(videoToDelete);
            await _context.SaveChangesAsync();

            return Ok(deletedVideoDto);
        }
        catch (Exception)
        {
            return Problem("An error occurred while deleting the video.");
        }
    }
}
