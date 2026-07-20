using FastEndpoints;

namespace Server.Modules.Users;

public sealed class UsersGroup : Group
{
    public UsersGroup()
    {
        Configure("users", ep =>
        {
            ep.Description(x =>
                x.Produces(StatusCodes.Status401Unauthorized));
        });
    }
}