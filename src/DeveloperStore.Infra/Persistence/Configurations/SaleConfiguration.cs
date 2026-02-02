using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeveloperStore.Infra.Persistence.Configurations;

public sealed class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("sales");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.SaleNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.SaleDate).IsRequired();

        builder.Property(s => s.TotalAmount)
            .HasConversion(
                money => money.Amount,
                value => new Money(value))
            .HasColumnName("total_amount");

        builder.OwnsOne(s => s.Customer, customer =>
        {
            customer.Property(c => c.ExternalId).HasColumnName("customer_external_id").IsRequired();
            customer.Property(c => c.Description).HasColumnName("customer_description").HasMaxLength(200);
        });

        builder.OwnsOne(s => s.Branch, branch =>
        {
            branch.Property(c => c.ExternalId).HasColumnName("branch_external_id").IsRequired();
            branch.Property(c => c.Description).HasColumnName("branch_description").HasMaxLength(200);
        });

        builder.Property(s => s.IsCancelled)
            .HasColumnName("is_cancelled");

        builder.Navigation(s => s.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(s => s.Items)
            .WithOne()
            .HasForeignKey("sale_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
