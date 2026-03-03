using ECommerce.Application.DTOs.Category;
using ECommerce.Application.Entities;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryResponse>> GetAllAsync()
    {
        return await _context.Categories.AsNoTracking().Select(c => ToResponse(c)).ToListAsync();
    }

    public async Task<CategoryResponse?> GetByIdAsync(Guid id)
    {
        var category = await _context
            .Categories.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
        return category == null ? null : ToResponse(category);
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
    {
        var exists = await _context.Categories.AnyAsync(c => c.Name == request.Name);
        if (exists)
            throw new BadRequestException("Category already exists");

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return ToResponse(category);
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            throw new NotFoundException("Category not found");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }

    private static CategoryResponse ToResponse(Category c) =>
        new()
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
        };
}
