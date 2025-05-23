﻿using ApplicationLayer.Interfaces; 
using InfrastructureLayer.Data; 
using InfrastructureLayer.Implementations.Repositories; 
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 5
        });
    });
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PresentationLayerApi", Version = "v1" });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(ApplicationLayer.AssemblyReference).Assembly); 
    config.RegisterServicesFromAssembly(typeof(InfrastructureLayer.AssemblyReference).Assembly);
}); 

builder.Services.AddScoped(typeof(ICreateRepository<>), typeof(CreateRepository<>));
builder.Services.AddScoped(typeof(IUpdateRepository<>), typeof(UpdateRepository<>));
builder.Services.AddScoped(typeof(IDeleteRepository<>), typeof(DeleteRepository<>));
builder.Services.AddScoped(typeof(IGetAllRepository<>), typeof(GetAllRepository<>));
builder.Services.AddScoped(typeof(IGetRepository<>), typeof(GetRepository<>));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PresentationLayerApi v1"));
}

app.UseRateLimiter();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

//[PresentationLayer]
//      |
//[ApplicationLayer]
//  |           |
//Commands    Queries
//  |           |
//Handlers(Mediator)
//      |
//[DomainLayer]
//      |
//[InfrastructureLayer]


//Write Flow:
//Controller → Command → MediatR → Handler → Repository → DB

//Read Flow:
//Controller → Query → MediatR → Handler → Repository → DB → DTO