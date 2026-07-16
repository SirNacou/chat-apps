using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Server.Domain;

namespace Server.Infrastructure.Configuration;

internal class RoomMemberConfiguration : IEntityTypeConfiguration<RoomMember>
{
    public void Configure(EntityTypeBuilder<RoomMember> builder)
    {
        builder.HasKey(rm => new { rm.RoomId, rm.UserId });

        builder.HasOne(rm => rm.Room)
            .WithMany(r => r.Members)
            .HasForeignKey(rm => rm.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<IdentityUser>()
            .WithMany()
            .HasForeignKey(rm => rm.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}