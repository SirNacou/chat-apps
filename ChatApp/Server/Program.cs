global using FluentValidation;

using System.Text.Json;
using System.Text.Json.Serialization;

using FastEndpoints;
using FastEndpoints.OpenApi;
using FastEndpoints.Swagger;

using Microsoft.AspNetCore.Identity;

using Scalar.AspNetCore;

using Server;
using Server.Infrastructure.Database;
using Server.Infrastructure.Options;
using Server.Infrastructure.Serializers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints()
    .OpenApiDocument();

builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection(DatabaseOptions.SectionName));

builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/docs", options =>
    {
        options.AddDocument("v1", "My API", "/openapi/{documentName}.json");
    });
}

app.UseHttpsRedirection();

app.MapIdentityApi<IdentityUser>();

app.MapPost("/logout", async (SignInManager<IdentityUser> signInManager) =>
    {
        await signInManager.SignOutAsync();
        return Results.Ok();
    })
    .RequireAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .RequireAuthorization();

app.UseFastEndpoints(c =>
{
    c.Endpoints.ShortNames = true;

    c.Serializer.Options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    c.Serializer.Options.Converters.Add(TrimmingStringConverter.Instance);
    c.Serializer.Options.AddSerializerContextsFromServer();
});

await app.RunAsync();