using DeveloperStore.Application.Contracts;
using DeveloperStore.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(typeof(DeveloperStore.Application.Sales.Commands.CreateSaleCommand).Assembly);
builder.Services.AddAutoMapper(typeof(DeveloperStore.Application.Sales.SaleMappingProfile).Assembly);

builder.Services.AddDbContext<DeveloperStore.Infra.Persistence.SalesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddScoped<DeveloperStore.Application.Abstractions.ISaleRepository, DeveloperStore.Infra.Repositories.SaleRepository>();
builder.Services.AddScoped<DeveloperStore.Application.Abstractions.ISaleReadRepository, DeveloperStore.Infra.Mongo.Repositories.SaleReadRepository>();
builder.Services.AddScoped<DeveloperStore.Application.Abstractions.IEventPublisher, DeveloperStore.Api.Services.LogEventPublisher>();

builder.Services.AddSingleton(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("Mongo");
    return new MongoDB.Driver.MongoClient(connectionString);
});
builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<MongoDB.Driver.MongoClient>();
    var databaseName = builder.Configuration.GetValue<string>("Mongo:Database");
    return client.GetDatabase(databaseName);
});

var app = builder.Build();

app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        var exception = feature?.Error;

        var (type, error, detail, statusCode) = exception switch
        {
            DomainException domainEx => ("ValidationError", "Invalid input data", domainEx.Message, StatusCodes.Status400BadRequest),
            _ => ("InternalError", "Unexpected error", "An unexpected error occurred.", StatusCodes.Status500InternalServerError)
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new ErrorResponse(type, error, detail));
    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapScalarApiReference(options =>
    {
        options.Title = "DeveloperStore API (Scalar)";
        options.OpenApiRoutePattern = "/swagger/{documentName}/swagger.json";
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

public sealed record ErrorResponse(string Type, string Error, string Detail);
