using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Product;
using ECommerce.Application.DTOs.Review;
using ECommerce.Application.Entities;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private readonly IFileService _fileService;
    private readonly IRedisService _redisService;

    // cache keys
    private const string ProductsCacheKey = "products";

    private static string ProductCacheKey(Guid id) => $"product:{id}";

    public ProductService(
        AppDbContext context,
        IFileService fileService,
        IRedisService redisService
    )
    {
        _context = context;
        _fileService = fileService;
        _redisService = redisService;
    }

    public async Task<PagedResponse<ProductResponse>> GetAllAsync(ProductPagedRequest request)
    {
        // build unique cache key based on all query params
        var cacheKey =
            $"{ProductsCacheKey}:{request.Page}:{request.Limit}:{request.Search}:"
            + $"{request.CategoryId}:{request.MinPrice}:{request.MaxPrice}:"
            + $"{request.SortBy}:{request.SortOrder}";

        // check cache first
        var cached = await _redisService.GetAsync<PagedResponse<ProductResponse>>(cacheKey);
        if (cached != null)
        {
            Console.WriteLine($"⚡ Cache HIT: {cacheKey}");
            return cached;
        }

        Console.WriteLine($"🔍 Cache MISS: {cacheKey}");

        // cache miss — query DB
        var query = _context
            .Products.AsNoTracking() // no tracking since we won't update entities
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
                .ThenInclude(r => r.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p =>
                p.Name.ToLower().Contains(request.Search.ToLower())
                || p.Description.ToLower().Contains(request.Search.ToLower())
            );

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId);

        if (request.MinPrice.HasValue)
            query = query.Where(p => p.Price >= request.MinPrice);

        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= request.MaxPrice);

        query = request.SortBy?.ToLower() switch
        {
            "price" => request.SortOrder == "asc"
                ? query.OrderBy(p => p.Price)
                : query.OrderByDescending(p => p.Price),
            "name" => request.SortOrder == "asc"
                ? query.OrderBy(p => p.Name)
                : query.OrderByDescending(p => p.Name),
            _ => query.OrderByDescending(p => p.CreatedAt),
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            .Select(p => ToResponse(p))
            .ToListAsync();

        var result = new PagedResponse<ProductResponse>
        {
            Items = items,
            Page = request.Page,
            Limit = request.Limit,
            TotalCount = totalCount,
        };

        // save to cache for 10 minutes
        await _redisService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));

        return result;
    }

    public async Task<ProductResponse?> GetByIdAsync(Guid id)
    {
        var cacheKey = ProductCacheKey(id);

        var cached = await _redisService.GetAsync<ProductResponse>(cacheKey);
        if (cached != null)
        {
            Console.WriteLine($"⚡ Cache HIT: {cacheKey}");
            return cached;
        }

        Console.WriteLine($"🔍 Cache MISS: {cacheKey}");

        var product = await _context
            .Products.AsNoTracking() // no tracking since we won't update entities
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return null;

        var response = ToResponse(product);

        // cache for 10 minutes
        await _redisService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

        return response;
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
            throw new NotFoundException("Category not found");

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            CategoryId = request.CategoryId,
            CreatedAt = DateTime.UtcNow,
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        await InvalidateProductCacheAsync(product.Id);

        return ToResponse(
            await _context.Products.Include(p => p.Category).FirstAsync(p => p.Id == product.Id)
        );
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            throw new NotFoundException("Product not found");

        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
            throw new NotFoundException("Category not found");

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.CategoryId = request.CategoryId;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await InvalidateProductCacheAsync(product.Id);

        return ToResponse(
            await _context.Products.Include(p => p.Category).FirstAsync(p => p.Id == product.Id)
        );
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            throw new NotFoundException("Product not found");

        _context.Products.Remove(product);
        await InvalidateProductCacheAsync(product.Id);
        await _context.SaveChangesAsync();
    }

    public async Task<ProductResponse> AddImageAsync(Guid productId, IFormFile file, bool isMain)
    {
        var product = await _context
            .Products.Include(p => p.Images)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null)
            throw new NotFoundException("Product not found");

        // auto set as main if it's the first image
        if (!product.Images.Any())
            isMain = true; // first image is always main

        // validate file
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(extension))
            throw new Exception("Only .jpg, .jpeg, .png and .webp images are allowed");

        if (file.Length > 5 * 1024 * 1024) // 5MB limit
            throw new Exception("Image size must be less than 5MB");

        // if this is main image, unset previous main
        if (isMain)
            foreach (var img in product.Images)
                img.IsMain = false;

        var url = await _fileService.SaveImageAsync(file.OpenReadStream(), file.FileName);

        var image = new ProductImage
        {
            Id = Guid.NewGuid(),
            Url = url,
            IsMain = isMain,
            ProductId = productId,
            UploadedAt = DateTime.UtcNow,
        };

        _context.ProductImages.Add(image);
        await _context.SaveChangesAsync();
        await InvalidateProductCacheAsync(productId);

        return ToResponse(product);
    }

    public async Task DeleteImageAsync(Guid productId, Guid imageId)
    {
        var image = await _context.ProductImages.FirstOrDefaultAsync(i =>
            i.Id == imageId && i.ProductId == productId
        );

        if (image == null)
            throw new Exception("Image not found");

        _fileService.DeleteImage(image.Url);
        _context.ProductImages.Remove(image);
        await _context.SaveChangesAsync();
        await InvalidateProductCacheAsync(productId);
    }

    private async Task InvalidateProductCacheAsync(Guid? productId = null)
    {
        // invalidate specific product cache
        if (productId.HasValue)
            await _redisService.DeleteAsync(ProductCacheKey(productId.Value));

        // invalidate all product listing caches
        await _redisService.DeleteByPatternAsync($"{ProductsCacheKey}:*");
    }

    private static ProductResponse ToResponse(Product p) =>
        new()
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Stock = p.Stock,
            CategoryName = p.Category?.Name ?? string.Empty,
            CategoryId = p.CategoryId,
            CreatedAt = p.CreatedAt,
            Images =
            [
                .. p.Images.Select(i => new ProductImageResponse
                {
                    Id = i.Id,
                    Url = i.Url,
                    IsMain = i.IsMain,
                }),
            ],
            MainImageUrl =
                p.Images.FirstOrDefault(i => i.IsMain)?.Url ?? p.Images.FirstOrDefault()?.Url,

            Reviews =
            [
                .. p.Reviews.Select(r => new ReviewResponse
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CustomerName = r.User?.FullName ?? string.Empty,
                    CreatedAt = r.CreatedAt,
                }),
            ],
            AverageRating = p.Reviews.Any() ? Math.Round(p.Reviews.Average(r => r.Rating), 1) : 0,
            ReviewCount = p.Reviews.Count,
        };
}
