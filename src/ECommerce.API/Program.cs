using System.Text;
using ECommerce.API.Middleware;
using ECommerce.API.Validators;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Rate limiting for auth endpoints
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(
        "auth",
        opt =>
        {
            opt.PermitLimit = 5; // 5 requests
            opt.Window = TimeSpan.FromMinutes(1); // per minute
            opt.QueueLimit = 0;
        }
    );
});

// Controllers
builder.Services.AddControllers();

// Validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateProductRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateCategoryRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ProductPagedRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RefreshTokenRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AddToCartRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateCartItemRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateReviewRequestValidator>();

builder.Services.AddEndpointsApiExplorer();

// OpenAPI with Bearer auth
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer(
        (document, context, ct) =>
        {
            document.Components ??= new();
            document.Components.SecuritySchemes = new Dictionary<
                string,
                Microsoft.OpenApi.IOpenApiSecurityScheme
            >
            {
                ["Bearer"] = new Microsoft.OpenApi.OpenApiSecurityScheme()
                {
                    Type = Microsoft.OpenApi.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                },
            };
            return Task.CompletedTask;
        }
    );
});

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Redis
var redisConnection = builder.Configuration.GetConnectionString("Redis")!;
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnection)
);
builder.Services.AddSingleton<IRedisService, RedisService>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// JWT Auth
var jwtSecret = builder.Configuration["Jwt:Secret"]!;
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        };
    });

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>(); // global error handling

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseRateLimiter();

// auto migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
