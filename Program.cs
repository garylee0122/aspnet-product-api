using DemoAPI.Data;
using DemoAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<ProductService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// for global model validation error response
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        return new BadRequestObjectResult(new
        {
            status = "error",
            errors = context.ModelState
        });
    };
});

// sqlite
//builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=products.db"));

// mysql
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();