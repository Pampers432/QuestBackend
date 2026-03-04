using Application.DTO;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QuestsApi.Data;

namespace Application.Services;

public class CategoryService
{
    private readonly QuestPlatformContext _context;

    public CategoryService(QuestPlatformContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description))
            .ToListAsync();
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return null;

        return new CategoryDto(category.Id, category.Name, category.Description);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim()
        };

        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        return new CategoryDto(category.Id, category.Name, category.Description);
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
            return null;

        category.Name = request.Name.Trim();
        category.Description = request.Description?.Trim();

        await _context.SaveChangesAsync();

        return new CategoryDto(category.Id, category.Name, category.Description);
    }

    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
            return false;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return true;
    }
}

