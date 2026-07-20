using FastEndpoints;

namespace Server.Modules.Rooms;

public sealed class RoomsGroup : Group
{
    public RoomsGroup()
    {
        Configure("rooms", ep =>
        {
            ep.Description(x =>
                x.Produces(StatusCodes.Status401Unauthorized));
        });
    }
}