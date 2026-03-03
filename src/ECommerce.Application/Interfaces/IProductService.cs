using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Product;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Interfaces;

public interface IProductService
{
    Task<PagedResponse<ProductResponse>> GetAllAsync(ProductPagedRequest request);
    Task<ProductResponse?> GetByIdAsync(Guid id);
    Task<ProductResponse> CreateAsync(CreateProductRequest request);
    Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request);
    Task DeleteAsync(Guid id);

    // images
    Task<ProductResponse> AddImageAsync(Guid productId, IFormFile file, bool isMain);
    Task DeleteImageAsync(Guid productId, Guid imageId);
}
