using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using Server.Infrastructure.Database;

namespace Server.Modules.Rooms.AddMembers;

public sealed class AddMembersEndpoint(ApplicationDbContext dbContext) : Ep.Req<AddMembersRequest>
    .NoRes
{
    public override void Configure()
    {
        Post("/{roomId}/members");
        Group<RoomsGroup>();
    }

    public override async Task HandleAsync(AddMembersRequest req, CancellationToken ct)
    {
        var roomId = Route<Guid>("roomId");
        ThrowIfAnyErrors();

        var room = await dbContext.Rooms.FirstOrDefaultAsync(r => r.Id == roomId, ct);
        if (room == null)
        {
            ThrowError("The specified room does not exist.", "RoomNotFound",
                statusCode: StatusCodes.Status404NotFound);
            return;
        }

        room.AddMembers(req.MemberIds);
        await dbContext.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}

public sealed class AddMembersRequest
{
    public required List<string> MemberIds { get; set; }
}

public sealed class AddMembersValidator : Validator<AddMembersRequest>
{
    public AddMembersValidator()
    {
        RuleFor(x => x.MemberIds).NotEmpty()
            .Must(ids => ids.All(id => !string.IsNullOrWhiteSpace(id)))
            .WithMessage("MemberIds must not contain empty or whitespace strings.");
    }
}