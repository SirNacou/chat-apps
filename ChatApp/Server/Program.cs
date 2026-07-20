global using FluentValidation;

using System.Text.Json;

using FastEndpoints;
using FastEndpoints.OpenApi;

using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;

using Scalar.AspNetCore;

using Server;
using Server.Domain;
using Server.Infrastructure.Database;
using Server.Infrastructure.Options;
using Server.Infrastructure.Serializers;
using Server.Modules.Chat;
using Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints(DiscoveredTypes.All)
    .OpenApiDocument(o =>
    {
        o.ShortSchemaNames = true;
        o.ConfigureOpenApi = options =>
        {
            options.AddOperationTransformer((operation, context, _) =>
            {
                var path = context.Description.RelativePath;

                if (path == "register" && operation.RequestBody != null)
                {
                    var registerExample = new { email = "admin@local.host", password = "Test1234!" };

                    if (operation.RequestBody.Content?.TryGetValue("application/json", out var mediaType) ?? false)
                    {
                        mediaType.Example = JsonSerializer.SerializeToNode(registerExample);
                    }
                }

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
        };
    });

builder.Services.AddServiceHealthChecks();
builder.Services.AddCors();
builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection(DatabaseOptions.SectionName));

builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<ChatUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddSingleton<IPresenceService, MemoryPresenceService>();

builder.Services.AddSignalR();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(policy => policy
    .WithOrigins("http://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials());
app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityApi<ChatUser>();
app.MapPost("/logout", async (SignInManager<IdentityUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Ok();
}).RequireAuthorization();

app.MapHub<ChatHub>("/chathub");

app.UseFastEndpoints(c =>
{
    c.Errors.UseProblemDetails();
    c.Endpoints.ShortNames = true;
    c.Endpoints.NameGenerator = ctx => ctx.EndpointType.Name.TrimEnd("Endpoint").ToString();
    c.Serializer.Options.Converters.Add(TrimmingStringConverter.Instance);
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/docs", options =>
    {
        options.AddDocument("v1", "My API");
    });
}

await app.RunAsync();