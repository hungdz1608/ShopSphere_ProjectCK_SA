using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ShopSphere.Infrastructure.db;
using ShopSphere.Application.Services;
using ShopSphere.Application.Repositories;
using ShopSphere.Infrastructure.Repositories;
using ShopSphere.Domain.Errors;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ShopSphere.Api",
        Version = "v1"
    });
});

builder.Services.AddDbContext<ShopSphereDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
);

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ProductService>();

var app = builder.Build();


app.UseSwagger();

app.UseSwaggerUI(c =>
{

    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShopSphere.Api v1");
    c.RoutePrefix = "swagger";  
});

// Global error handling
app.Use(async (context, next) =>
{
    try { await next(); }
    catch (BadRequestException ex)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new { message = ex.Message });
    }
    catch (NotFoundException ex)
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsJsonAsync(new { message = ex.Message });
    }
});


app.UseHttpsRedirection();

app.MapControllers();
app.Run();
