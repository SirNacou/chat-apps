using FastEndpoints;

namespace Server.Modules.Rooms.CreateDirectMessage;

public sealed class CreateDirectMessageEndpoint : Ep.Req<CreateDirectMessageRequest>.NoRes
{
    public override void Configure()
    {
        Post("/dm");
        Group<RoomsGroup>();
    }

    public override async Task HandleAsync(CreateDirectMessageRequest req, CancellationToken ct)
    {
        await Send.OkAsync("OK", ct);
    }
}