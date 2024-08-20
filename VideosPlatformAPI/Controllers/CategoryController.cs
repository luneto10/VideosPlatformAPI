using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideosPlatformAPI.Data;
using VideosPlatformAPI.DTO;
using VideosPlatformAPI.Models;

namespace VideosPlatformAPI.Controllers;

[ApiController]
[Route("categories")]
public class CategoryController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoryController(AppDbContext context)
    {
        _context = context;
    }

    // Helper method to convert a Category entity to a CategoryResponseDTO
    private CategoryResponseDTO ConvertToCategoryResponseDto(Category category)
    {
        return new CategoryResponseDTO
        (
            Id: category.Id,
            Title: category.Title,
            Color: category.Color
        );
    }

    // GET: /categories
    [HttpGet(Name = "GetAllCategories")]
    public async Task<ActionResult<IEnumerable<CategoryResponseDTO>>> Get()
    {
        try
        {
            var categories = await _context.Categories.ToListAsync();
            var categoryDtos = categories.Select(ConvertToCategoryResponseDto).ToList();

            return Ok(categoryDtos);
        }
        catch (Exception)
        {
            return Problem("An error occurred while retrieving the categories.");
        }
    }

    // GET: /categories/{id}
    [HttpGet("{id:int}", Name = "GetCategoryById")]
    public async Task<ActionResult<CategoryResponseDTO>> Get(int id)
    {
        try
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound($"Category with Id = {id} not found.");
            }

            return Ok(ConvertToCategoryResponseDto(category));
        }
        catch (Exception)
        {
            return Problem("An error occurred while retrieving the category.");
        }
    }

    // POST: /categories
    [HttpPost(Name = "CreateCategory")]
    public async Task<ActionResult<CategoryResponseDTO>> Post([FromBody] CategoryDTO categoryDto)
    {
        try
        {
            if (categoryDto == null)
            {
                return BadRequest("Category data is null");
            }

            var categoryToAdd = new Category
            {
                Title = categoryDto.Title,
                Color = categoryDto.Color
            };

            if (!TryValidateModel(categoryToAdd))
            {
                return BadRequest(ModelState);
            }

            _context.Categories.Add(categoryToAdd);
            await _context.SaveChangesAsync();

            var categoryResponseDto = ConvertToCategoryResponseDto(categoryToAdd);

            return CreatedAtRoute("GetCategoryById", new { id = categoryResponseDto.Id }, categoryResponseDto);
        }
        catch (Exception)
        {
            return Problem("An error occurred while creating the category.");
        }
    }

    // PUT: /categories/{id}
    [HttpPut("{id:int}", Name = "UpdateCategory")]
    public async Task<ActionResult<CategoryResponseDTO>> Update(int id, [FromBody] CategoryDTO categoryDto)
    {
        try
        {
            if (categoryDto == null)
            {
                return BadRequest("Category data is null");
            }

            var categoryToUpdate = await _context.Categories.FindAsync(id);

            if (categoryToUpdate == null)
            {
                return NotFound($"Category with Id = {id} not found.");
            }

            categoryToUpdate.Title = !string.IsNullOrEmpty(categoryDto.Title) ? categoryDto.Title : categoryToUpdate.Title;
            categoryToUpdate.Color = !string.IsNullOrEmpty(categoryDto.Color) ? categoryDto.Color : categoryToUpdate.Color;

            if (!TryValidateModel(categoryToUpdate))
            {
                return BadRequest(ModelState);
            }

            await _context.SaveChangesAsync();

            var updatedCategory = await _context.Categories.FindAsync(id);
            return Ok(ConvertToCategoryResponseDto(updatedCategory));
        }
        catch (Exception)
        {
            return Problem("An error occurred while updating the category.");
        }
    }

    // DELETE: /categories/{id}
    [HttpDelete("{id:int}", Name = "DeleteCategory")]
    public async Task<ActionResult<CategoryResponseDTO>> Delete(int id)
    {
        try
        {
            var categoryToDelete = await _context.Categories.FindAsync(id);

            if (categoryToDelete == null)
            {
                return NotFound($"Category with Id = {id} not found.");
            }

            var deletedCategoryDto = ConvertToCategoryResponseDto(categoryToDelete);

            _context.Categories.Remove(categoryToDelete);
            await _context.SaveChangesAsync();

            return Ok(deletedCategoryDto);
        }
        catch (Exception)
        {
            return Problem("An error occurred while deleting the category.");
        }
    }
}
