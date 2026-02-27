using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> 
    where TEntity : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // UUID Primary Key
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
               .HasDefaultValueSql("gen_random_uuid()");

        // Timestamps
        builder.Property(x => x.CreatedAt)
               .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
               .HasDefaultValueSql("now()")
               .ValueGeneratedOnAddOrUpdate();
    }
}