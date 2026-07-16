using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Server.Domain;
using Server.Infrastructure.Options;

namespace Server.Infrastructure.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IOptions<DatabaseOptions> databaseOptions)
    : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<Room> Rooms { get; set; }
    public DbSet<RoomMember> RoomMembers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(databaseOptions.Value.ConnectionString);
    }
}