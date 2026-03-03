using ECommerce.Application.DTOs.Category;

namespace ECommerce.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse?> GetByIdAsync(Guid id);
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request);
    Task DeleteAsync(Guid id);
}
