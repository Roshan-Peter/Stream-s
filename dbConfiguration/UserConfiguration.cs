using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stream.Schema;

public class UserConfiguration : BaseEntityConfiguration<Users>
{
    public override void Configure(EntityTypeBuilder<Users> builder)
    {
        // 1. Apply the Base ID and Timestamp logic
        base.Configure(builder);

        // 2. Add User-specific constraints
        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.LastName).IsRequired(false);
    }
}