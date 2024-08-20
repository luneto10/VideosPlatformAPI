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
    
    private VideoResponseDTO ConvertToVideoResponseDto(Video video)
    {
        return new VideoResponseDTO
        (
            id: video.Id,
            Title: video.Title,
            Description : video.Description,
            Url : video.Url,
            Category : new CategoryDTO
            (
                Title : video.Category.Title,
                Color : video.Category.Color
            )
        );
    }
    
    // GET
    [HttpGet(Name = "GetAllVideos")]
    public async Task<ActionResult<IEnumerable<Video>>> Get()
    {
        try
        {
            var videos = await _context.Videos
                .Include(v => v.Category)  // Include the Category for each Video
                .ToListAsync();

            return Ok(videos.Select(ConvertToVideoResponseDto).ToList());
        }
        catch (Exception e)
        {
            return Problem("An error Occur");
        }
    }
    [HttpGet("{id:int}", Name = "GetVideoById")]
    public async Task<ActionResult<VideoResponseDTO>> Get(int id)
    {
        try
        {
            var video = await _context.Videos.FindAsync(id);
        
            if (video == null)
            {
                return NotFound($"Video with Id = {id} not found.");
            }

            return Ok(ConvertToVideoResponseDto(video));
        }
        catch (Exception e)
        {
            return Problem("An error occurred while retrieving the video.");
        }
    }

    [HttpPost(Name = "CreateVideo")]
    public async Task<ActionResult<VideoResponseDTO>> Post([FromBody] VideoDTO videoDto)
    {
        try
        {
            if (videoDto is null)
            {
                return BadRequest("Video data is null");
            }
        
            Video videoToAdd = new Video
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
        
            // Validate the Video entity only
            if (!TryValidateModel(videoToAdd))
            {
                return BadRequest(ModelState);
            }
        
            _context.Videos.Add(videoToAdd);
            await _context.SaveChangesAsync();

            return CreatedAtRoute("GetVideoById", new { id = videoToAdd.Id }, videoToAdd);
        }
        catch (Exception e)
        {
            return Problem("An error occurred while saving the video.");
        }
    }

    
    [HttpPut("{id:int}", Name = "UpdateVideo")]
    public async Task<IActionResult> Update(int id, [FromBody] VideoDTO videoDto)
    {
        try
        {
            if (videoDto == null)
            {
                return BadRequest("Video data is null");
            }
            
            var videoToUpdate = await _context.Videos.FindAsync(id);
            
            if (videoToUpdate == null)
            {
                return NotFound($"Video with Id = {id} not found.");
            }
            
            if (!string.IsNullOrEmpty(videoDto.Title))
            {
                videoToUpdate.Title = videoDto.Title;
            }

            if (!string.IsNullOrEmpty(videoDto.Description))
            {
                videoToUpdate.Description = videoDto.Description;
            }

            if (!string.IsNullOrEmpty(videoDto.Url))
            {
                videoToUpdate.Url = videoDto.Url;
            }

            var category =
                _context.Categories.FirstOrDefault(c => c.Title.ToUpper().Equals(videoDto.CategoryName.ToUpper()));
            
            if (category is not null)
            {
                videoToUpdate.Category = category;
            }
            
            if (!TryValidateModel(videoToUpdate))
            {
                return BadRequest(ModelState);
            }
            
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception e)
        {
            return Problem("An error occurred while updating the video.");
        }
    }
    
    [HttpDelete("{id:int}", Name = "DeleteVideo")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var videoToDelete = await _context.Videos.FindAsync(id);

            if (videoToDelete == null)
            {
                return NotFound($"Video with Id = {id} not found.");
            }

            _context.Videos.Remove(videoToDelete);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            return Problem("An error occurred while deleting the video.");
        }
    }
}