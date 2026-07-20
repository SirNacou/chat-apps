global using FluentValidation;

global using LanguageExt;

global using static LanguageExt.Prelude;

using System.Text.Json;
using System.Text.Json.Nodes;

using FastEndpoints;
using FastEndpoints.OpenApi;

using LanguageExt.Common;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

using Scalar.AspNetCore;

using Server;
using Server.Domain;
using Server.Infrastructure.Database;
using Server.Infrastructure.Options;
using Server.Infrastructure.Serializers;
using Server.Middlewares;
using Server.Modules.Chat;
using Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints()
    .OpenApiDocument(o =>
    {
        o.ShortSchemaNames = true;
        o.ConfigureOpenApi = options => options.AddOperationTransformer((operation, context, _) =>
        {
            var path = context.Description.RelativePath;

            // Add example request for the /register endpoint
            if (path == "register" && operation.RequestBody != null)
            {
                var registerExample = new { email = "admin@local.host", password = "Test1234!" };

                if (operation.RequestBody.Content?.TryGetValue("application/json", out var mediaType) ?? false)
                {
                    mediaType.Example = JsonSerializer.SerializeToNode(registerExample);
                }
            }

            // Add example request for the /login endpoint
            if (path == "login" && operation.RequestBody != null)
            {
                var loginExample = new
                {
                    email = "admin@local.host",
                    password = "Test1234!",
                    twoFactorCode = "",
                    twoFactorRecoveryCode = ""
                };

                if (operation.RequestBody.Content?.TryGetValue("application/json", out var mediaType) ?? false)
                {
                    mediaType.Example = JsonSerializer.SerializeToNode(loginExample);
                }
            }

            return Task.CompletedTask;
        });
    });

builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection(DatabaseOptions.SectionName));

builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<ChatUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddSingleton<IPresenceService, MemoryPresenceService>();

builder.Services.AddSignalR();

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

app.MapIdentityApi<ChatUser>();

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
    c.Errors.UseProblemDetails();

    c.Endpoints.Configurator = ep =>
    {
        if (ep.ResDtoType.IsAssignableTo(typeof(Error)))
        {
            ep.DontAutoSendResponse();
            ep.PostProcessors(Order.After, typeof(ResponseSender<>));
            ep.Description(b => b.ClearDefaultProduces()
                .Produces(200, ep.ResDtoType.GetGenericArguments()[0])
                .ProducesProblemDetails());
        }
    };
    c.Endpoints.ShortNames = true;

    c.Serializer.Options.Converters.Add(TrimmingStringConverter.Instance);
    c.Serializer.Options.AddSerializerContextsFromServer();
});

app.MapHub<ChatHub>("/chathub");

await app.RunAsync();