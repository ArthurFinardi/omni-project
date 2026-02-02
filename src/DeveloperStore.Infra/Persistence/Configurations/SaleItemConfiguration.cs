using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeveloperStore.Infra.Persistence.Configurations;

public sealed class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("sale_items");
        builder.HasKey(s => s.Id);

        builder.OwnsOne(s => s.Product, product =>
        {
            product.Property(p => p.ExternalId).HasColumnName("product_external_id").IsRequired();
            product.Property(p => p.Description).HasColumnName("product_description").HasMaxLength(200);
        });

        builder.Property(s => s.Quantity)
            .HasConversion(
                quantity => quantity.Value,
                value => Quantity.From(value))
            .HasColumnName("quantity");

        builder.Property(s => s.UnitPrice)
            .HasConversion(
                money => money.Amount,
                value => new Money(value))
            .HasColumnName("unit_price");

        builder.Property(s => s.Discount)
            .HasConversion(
                discount => discount.Rate,
                value => Discount.FromRate(value))
            .HasColumnName("discount_rate");

        builder.Property(s => s.DiscountAmount)
            .HasConversion(
                money => money.Amount,
                value => new Money(value))
            .HasColumnName("discount_amount");

        builder.Property(s => s.TotalAmount)
            .HasConversion(
                money => money.Amount,
                value => new Money(value))
            .HasColumnName("total_amount");

        builder.Property(s => s.IsCancelled)
            .HasColumnName("is_cancelled");
    }
}
