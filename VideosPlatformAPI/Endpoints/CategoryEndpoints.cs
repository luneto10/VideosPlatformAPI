using Carter;
using VideosPlatformAPI.Utility;

namespace VideosPlatformAPI.Endpoints;

using Microsoft.EntityFrameworkCore;
using Data;
using DTO;
using Models;

public class CategoryEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("categories");
        
        group.MapGet("", GetCategories).WithName(nameof(GetCategories));
        group.MapGet("{id:int}", GetCategoryById).WithName(nameof(GetCategoryById));
        group.MapGet("{id:int}/videos", GetVideosByCategoryId).WithName(nameof(GetVideosByCategoryId));
        group.MapPost("", CreateCategory).WithName(nameof(CreateCategory));
        group.MapPut("{id:int}", UpdateCategory).WithName(nameof(UpdateCategory));
        group.MapDelete("{id:int}", DeleteCategory).WithName(nameof(DeleteCategory));
    }
    
    private static async Task<IResult> GetCategories(AppDbContext context)
    {
        try
        {
            var categories = await context.Categories.ToListAsync();
            var categoryDtos = categories.Select(ConvertToCategoryResponseDto).ToList();
            return Results.Ok(categoryDtos);
        }
        catch
        {
            return Results.Problem("An error occurred while retrieving the categories.");
        }
    }

    private static async Task<IResult> GetCategoryById(
        int id, 
        AppDbContext context)
    {
        try
        {
            var category = await context.Categories.FindAsync(id);
            return category == null 
                ? Results.NotFound($"Category with Id = {id} not found.") 
                : Results.Ok(ConvertToCategoryResponseDto(category));
        }
        catch
        {
            return Results.Problem("An error occurred while retrieving the category.");
        }
    }

    private static async Task<IResult> GetVideosByCategoryId(
        int id, 
        AppDbContext context,
        VideoMappingService videoMappingService )
    {
        try
        {
            var category = await context.Categories.FindAsync(id);
            if (category == null)
            {
                return Results.NotFound($"Category with Id = {id} not found.");
            }

            var videos = await context.Videos
                .Where(v => v.Category.Id == id)
                .Include(v => v.Category)
                .ToListAsync();

            var videoDtos = videos.Select(videoMappingService.ConvertToVideoResponseDto).ToList();
            return Results.Ok(videoDtos);
        }
        catch
        {
            return Results.Problem("An error occurred while retrieving the videos.");
        }
    }

    private static async Task<IResult> CreateCategory(
        CategoryDTO categoryDto, 
        AppDbContext context)
    {
        try
        {
            if (categoryDto == null)
            {
                return Results.BadRequest("Category data is null.");
            }

            var categoryToAdd = new Category
            {
                Title = categoryDto.Title,
                Color = categoryDto.Color
            };

            context.Categories.Add(categoryToAdd);
            await context.SaveChangesAsync();

            var categoryResponseDto = ConvertToCategoryResponseDto(categoryToAdd);

            return Results.Created($"/categories/{categoryResponseDto.Id}", categoryResponseDto);
        }
        catch
        {
            return Results.Problem("An error occurred while creating the category.");
        }
    }

    private static async Task<IResult> UpdateCategory(
        int id, 
        CategoryDTO categoryDto, 
        AppDbContext context)
    {
        try
        {
            var categoryToUpdate = await context.Categories.FindAsync(id);

            if (categoryToUpdate == null)
            {
                return Results.NotFound($"Category with Id = {id} not found.");
            }

            categoryToUpdate.Title = !string.IsNullOrEmpty(categoryDto.Title) ? categoryDto.Title : categoryToUpdate.Title;
            categoryToUpdate.Color = !string.IsNullOrEmpty(categoryDto.Color) ? categoryDto.Color : categoryToUpdate.Color;

            await context.SaveChangesAsync();

            return Results.Ok(ConvertToCategoryResponseDto(categoryToUpdate));
        }
        catch
        {
            return Results.Problem("An error occurred while updating the category.");
        }
    }

    private static async Task<IResult> DeleteCategory(
        int id, 
        AppDbContext context)
    {
        try
        {
            var categoryToDelete = await context.Categories.FindAsync(id);

            if (categoryToDelete == null)
            {
                return Results.NotFound($"Category with Id = {id} not found.");
            }

            context.Categories.Remove(categoryToDelete);
            await context.SaveChangesAsync();

            return Results.Ok(ConvertToCategoryResponseDto(categoryToDelete));
        }
        catch
        {
            return Results.Problem("An error occurred while deleting the category.");
        }
    }
    
    private static CategoryResponseDTO ConvertToCategoryResponseDto(Category category) =>
        new (category.Id, category.Title, category.Color);
}
