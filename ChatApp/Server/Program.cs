global using FluentValidation;

using FastEndpoints;
using FastEndpoints.Swagger;

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
    .SwaggerDocument(o =>
    {
        o.ShortSchemaNames = true;
        o.DocumentSettings = s =>
        {
            s.Title = "My API";
            s.Version = "v1";
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
    c.Serializer.Options.AddSerializerContextsFromServer();
});

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi(c => c.Path = "/openapi/{documentName}.json");
    app.MapScalarApiReference("/docs", options =>
    {
        options.AddDocument("v1", "My API");
    });
}

await app.RunAsync();