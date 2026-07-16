using FastEndpoints;

namespace Server.Modules.Rooms.CreateRoom;

public sealed class CreateRoomValidator : Validator<CreateRoomRequest>
{
    public CreateRoomValidator()
    {
        RuleFor(x => x.Name)
            .MinimumLength(3)
            .WithMessage("Room name must be at least 3 characters long.")
            .MaximumLength(100)
            .WithMessage("Room name must not exceed 100 characters.");
    }
}