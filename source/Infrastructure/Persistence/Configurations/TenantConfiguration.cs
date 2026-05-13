using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.Subdomain)
            .HasColumnName("subdomain")
            .HasMaxLength(63)
            .IsRequired();

        builder.Property(x => x.Tier)
            .HasColumnName("subscription_tier")
            .HasMaxLength(32)
            .HasConversion(v => v.Name, v => SubscriptionTier.Create(v).Value)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired();

        builder.HasIndex(x => x.Subdomain)
            .IsUnique();

        builder.HasIndex(x => x.IsDeleted);

        builder.Ignore(x => x.DomainEvents);
    }
}
